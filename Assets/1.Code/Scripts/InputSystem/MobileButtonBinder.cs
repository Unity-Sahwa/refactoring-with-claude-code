// UI 버튼에 붙이는 컴포넌트. Inspector에서 ActionType 지정.
// OnButtonDown / OnButtonUp은 UI 시각 피드백(애니메이션 등) 전용.
// 게임플레이 입력은 Unity OnScreenButton 컴포넌트가 Input System 파이프라인으로 자동 처리.
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Refactoring
{
    public class MobileButtonBinder : MonoBehaviour, IMobileButton, IInjectTarget, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        private InputActionType _actionType;

        public InputActionType ActionType => _actionType;
        public Type[] InterfaceTypes => new[] { typeof(IMobileButton) };

        public event Action OnButtonDown;
        public event Action OnButtonUp;

        public void OnPointerDown(PointerEventData eventData) => OnButtonDown?.Invoke();
        public void OnPointerUp(PointerEventData eventData) => OnButtonUp?.Invoke();
    }
}
