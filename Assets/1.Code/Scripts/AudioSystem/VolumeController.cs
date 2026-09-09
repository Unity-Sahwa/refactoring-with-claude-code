using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting;
using UnityEngine.Serialization; //대원TODO: 삭제 필요. 안씀

namespace Refactoring
{
    // 책임: 설정된 볼륨값을 AudioMixer에 반영한다. (값 저장은 ISoundSettings 담당)
    // 흐름: 설정 변경 감지 → 카테고리별 0~1 값 조회 → 데시벨 변환 → 믹서 파라미터 설정
    public class VolumeController : MonoBehaviour
    {
        private const float MinDb = -80f;
        [SerializeField] private AudioMixer _mixer;

        //대원TODO: 웬만하면 변수에다가 주석을 달지는 말자. 행동에 달아야함. 
        // 설정창이 바꾼 소리 크기를 여기서 읽어서 믹서에 넣는다. 설정 쪽은 믹서를 모른다.
        [Preserve, Inject] private ISoundSettings _soundSettings;

        private void Start()
        {
            if (_soundSettings == null)
            {
                return;
            }

            //주입이 Awake에서 일어나기 때문에 Start에서 구독
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

        //대원TODO: 주석이 적절하지 않음
        // 전부 다시 넣는다. 뭐가 바뀌었는지 따지는 것보다 이게 싸고 단순하다.
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

            //대원TODO: 하드코딩스럽긴 한데 이 방식 말고는 생각이 안난다. 애니메이션 State, 믹서파라미터 이런 데이터와 Enum을 모두 SO에 포함시켜서 서로 맞는지 비교할 수 있게 만들 순 없나? 매번 enum 바뀌어서 오브젝트에 컴포넌트 연결해놓고 enum 설정했던거 나중에 일괄 수정하기 귀찮다고
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

            // 대원TODO: Log10(0.0001) = -80dB, Log10(1) = 0 맞제?
            return Mathf.Log10(volume01) * 20f;
        }
    }
}
