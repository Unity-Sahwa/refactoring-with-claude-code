using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 상태 이벤트(CameraShake)를 받아 스킬 연출용 카메라 셰이크를 실행한다.
    //       (타격·피격 셰이크는 PlayerCameraShake 담당이라 별개 채널)
    public class PlayerCameraShakeHandler : MonoBehaviour
    {
        [Preserve, Inject(true)] private CinemachineImpulseSource _impulseSource;
        [Preserve, Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;

        private IDisposable _eventDisposable;

        private void Awake()
        {
            _eventDisposable = _eventSubscriber?.Register(StateEventCategory.CameraShake, HandleShake);
        }

        private void OnDestroy()
        {
            _eventDisposable?.Dispose();
        }

        private void HandleShake(IStartData data)
        {
            if (data is not IPlayerCameraShake shakeData)
            {
                Debug.LogError($"[PlayerCameraShakeHandler] IPlayerCameraShake가 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            if (_impulseSource == null)
            {
                return;
            }

            ShakeData shake = shakeData.Shake;
            _impulseSource.ImpulseDefinition.ImpulseShape = shake.ImpulseShape;
            _impulseSource.ImpulseDefinition.ImpulseDuration = shake.Duration;
            _impulseSource.ImpulseDefinition.AmplitudeGain = shake.AmplitudeGain;
            _impulseSource.ImpulseDefinition.FrequencyGain = shake.FrequencyGain;
            _impulseSource.GenerateImpulseWithVelocity(new Vector3(shake.Velocity.x, shake.Velocity.y, 0f));
        }
    }
}
