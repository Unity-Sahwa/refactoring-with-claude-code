using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 상태 이벤트(CameraLock) 구간 동안 마우스 회전 입력을 꺼서 카메라를 고정한다.
    //       (셰이크·줌은 별개 채널, LockOnCamera는 회전 축이 없어 DefaultCamera 전용)
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class PlayerCameraLockHandler : MonoBehaviour
    {
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;

        private CinemachineInputAxisController _controller;
        private IDisposable _eventDisposable;

        private void Awake()
        {
            _controller = GetComponent<CinemachineInputAxisController>();
            _eventDisposable = _eventSubscriber?.Register(StateEventCategory.CameraLock, HandleLock, HandleUnlock);
        }

        private void OnDestroy()
        {
            _eventDisposable?.Dispose();
        }

        private void HandleLock(IStartData data)
        {
            _controller.enabled = false;
        }

        private void HandleUnlock(CloseEventType reason)
        {
            _controller.enabled = true;
        }
    }
}
