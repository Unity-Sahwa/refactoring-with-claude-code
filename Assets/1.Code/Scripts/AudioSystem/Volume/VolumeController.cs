using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 설정된 볼륨값을 AudioMixer에 반영한다. (값 저장은 ISoundSettings 담당)
    // 흐름: 설정 변경 감지 → 카테고리별 0~1 값 조회 → 데시벨 변환 → 믹서 파라미터 설정
    public class VolumeController : MonoBehaviour
    {
        private const float MinDb = -80f;
        [SerializeField] private AudioMixer _mixer;

        [Preserve, Inject] private ISoundSettings _soundSettings;

        private void Awake()
        {
            _soundSettings.OnChanged += ApplySettings;
            ApplySettings();
        }

        private void OnDestroy()
        {
            _soundSettings.OnChanged -= ApplySettings;
        }

        private void ApplySettings()
        {
            foreach (VolumeCategory category in Enum.GetValues(typeof(VolumeCategory)))
            {
                SetVolume(category, _soundSettings.GetVolume(category));
            }
        }

        
        private void SetVolume(VolumeCategory category, float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);

            // VolumeCategory 이름을 그대로 믹서 파라미터 이름으로 쓴다. 둘은 항상 같아야 한다.
            if (!_mixer.SetFloat(category.ToString(), LinearToDb(volume01)))
            {
                Debug.LogWarning($"[VolumeController] 믹서에 {category} 파라미터가 노출돼 있지 않음", this);
            }
        }

        private float LinearToDb(float volume01)
        {
            // 0 근처는 log 무한대라 최저로 표현
            if (volume01 <= 0.0001f)
            {
                //대원TODO: -80이 최소값인 이유가 뭐더라
                return MinDb; 
            }
            // -80dB ~ 0 dB 까지만 사용(그 이상은 증폭)
            return Mathf.Log10(volume01) * 20f;
        }
    }
}
