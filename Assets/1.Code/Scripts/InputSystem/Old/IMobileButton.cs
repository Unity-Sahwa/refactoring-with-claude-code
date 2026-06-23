// // 모바일 UI 버튼의 눌림/뗌 이벤트를 노출하는 인터페이스.
// // MobileButtonBinder가 구현. UI 버튼 시각 피드백(애니메이션 등) 전용.
// // 게임플레이 입력은 OnScreenButton → Input System 파이프라인으로 별도 처리됨.
// using System;

// namespace Refactoring
// {
//     public interface IMobileButton
//     {
//         InputActionType ActionType { get; }
//         event Action OnButtonDown;
//         event Action OnButtonUp;
//     }
// }
