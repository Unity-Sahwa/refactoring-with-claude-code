using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 역할: 상태 이벤트(CameraLock) 구간 동안 마우스 회전 입력을 꺼서 카메라를 고정한다.
    //       셰이크·줌(PlayerCameraEffectHandler)과는 별개 채널 — 이쪽은 "구간 동안 고정"만 다룬다.
    //       LockOnCamera는 원래 자동으로 대상을 보고 마우스 회전 축이 없어 대상이 아니다(DefaultCamera 전용).
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class PlayerCameraLockHandler : MonoBehaviour
    {
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;

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
