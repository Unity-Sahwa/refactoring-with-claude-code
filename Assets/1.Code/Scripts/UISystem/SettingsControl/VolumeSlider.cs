using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 소리 슬라이더 하나. 어떤 소리인지만 고르고, 값은 소리 주인에게 쓴다.
    [RequireComponent(typeof(Slider))]
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] private VolumeCategory _volumeType;

        [Preserve, Inject(true)] private ISoundSettings _soundSettings;

        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(HandleValueChanged);
        }

        private void OnEnable()
        {
            if (_soundSettings == null)
            {
                Debug.LogWarning($"{name}: {nameof(ISoundSettings)}가 없어 음량 표시를 건너뜀.");
                return;
            }

            _slider.SetValueWithoutNotify(_soundSettings.GetVolume(_volumeType));
        }

        private void HandleValueChanged(float value)
        {
            if (_soundSettings == null)
            {
                Debug.LogWarning($"{name}: {nameof(ISoundSettings)}가 없어 음량 적용을 건너뜀.");
                return;
            }

            _soundSettings.SetVolume(_volumeType, value);
        }
    }
}
