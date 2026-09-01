using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 모바일 UI 버튼에 붙임. Inspector에서 어떤 액션인지 고르면, 누를 때 그 키를 누른 것과 똑같이 처리됨.
    public class MobileInputButton : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private InputActionType _actionType;
        [Preserve , Inject(true)] private IInputSender _inputSender;

        public void OnPointerDown(PointerEventData eventData) => _inputSender?.RoutePressed(_actionType);
    }
}
