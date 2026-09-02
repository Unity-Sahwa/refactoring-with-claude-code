using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 설정된 볼륨값을 AudioMixer에 반영한다. (값 저장은 ISoundSettings 담당)
    // 흐름: 설정 변경 감지 → 카테고리별 0~1 값 조회 → 데시벨 변환 → 믹서 파라미터 설정
    public class VolumeController : MonoBehaviour
    {
        // 믹서 최소값 = 완전 무음
        private const float MinDb = -80f;
        [SerializeField] private AudioMixer _mixer;

        // 설정창이 바꾼 소리 크기를 여기서 읽어서 믹서에 넣는다. 설정 쪽은 믹서를 모른다.
        [Preserve, Inject] private ISoundSettings _soundSettings;

        // Awake가 아니라 Start인 이유: 주입이 Awake에 일어나서, Awake에 읽으면 아직 비어 있다.
        private void Start()
        {
            if (_soundSettings == null)
            {
                return;
            }

            _soundSettings.OnChanged += ApplySettings;
            ApplySettings();
        }

        private void OnDestroy()
        {
            if (_soundSettings != null)
            {
                _soundSettings.OnChanged -= ApplySettings;
            }
        }

        // 전부 다시 넣는다. 뭐가 바뀌었는지 따지는 것보다 이게 싸고 단순하다.
        private void ApplySettings()
        {
            foreach (VolumeCategory category in Enum.GetValues(typeof(VolumeCategory)))
            {
                SetVolume(category, _soundSettings.GetVolume(category));
            }
        }

        // VolumeCategory 이름을 그대로 믹서 파라미터 이름으로 쓴다. 둘은 항상 같아야 한다.
        private void SetVolume(VolumeCategory category, float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);
            if (!_mixer.SetFloat(category.ToString(), LinearToDb(volume01)))
            {
                Debug.LogWarning($"[VolumeController] 믹서에 {category} 파라미터가 노출돼 있지 않음", this);
            }
        }

        // 볼륨 범위는 -80dB ~ 0dB. 1을 넣으면 0dB(원본 소리), 0.0001을 넣으면 -80dB이 나옴.
        private float LinearToDb(float volume01)
        {
            // 0 근처는 log 무한대라 최저로 표현
            if (volume01 <= 0.0001f)
            {
                return MinDb;
            }

            return Mathf.Log10(volume01) * 20f;
        }
    }
}
