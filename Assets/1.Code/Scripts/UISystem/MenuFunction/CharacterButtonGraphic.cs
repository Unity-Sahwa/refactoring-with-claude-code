using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
    // 책임: 현재 캐릭터(인간/동물)에 맞는 이미지만 켜서 버튼의 Target Graphic으로 넣는다.
    public class CharacterButtonGraphic : MonoBehaviour
    {
        [SerializeField] private Button _button;

        [SerializeField] private Image _humanImage;

        [SerializeField] private Image _animalImage;

        [Preserve, Inject] private ICurrentCharacterProvider _characterProvider;
        [Preserve, Inject] private ICharacterSwapNotifier _swapNotifier;

        private void Start()
        {
            ApplyGraphic();
            _swapNotifier.OnCharacterSwapped += HandleCharacterSwapped;
        }

        private void OnDestroy()
        {
            _swapNotifier.OnCharacterSwapped -= HandleCharacterSwapped;
        }

        private void HandleCharacterSwapped()
        {
            ApplyGraphic();
        }

        // 현재 캐릭터에 맞는 이미지만 켜고 Target Graphic으로 지정한다.
        private void ApplyGraphic()
        {
            bool isHuman = _characterProvider.CurrentType == PlayerCharacterType.HumanCharacter;

            _humanImage.gameObject.SetActive(isHuman);
            _animalImage.gameObject.SetActive(!isHuman);

            _button.targetGraphic = isHuman ? _humanImage : _animalImage;
        }
    }
}
