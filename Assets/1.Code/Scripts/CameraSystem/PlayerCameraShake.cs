using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 타격 성공(HitChannel)과 피격(Damaged) 신호를 받아 그 순간 플레이어 상태에 맞는 카메라 임펄스를 쏜다.
    // 흐름: 신호 수신 → 상태별 셰이크 값 조회 → 같은 상태 중첩 보정 → 임펄스 발사
    public class PlayerCameraShake : MonoBehaviour
    {
        [Tooltip("같은 상태에서 연속으로 흔들 수 있는 최대 횟수")]
        [SerializeField] private int _maxSameStateStack = 3;

        [Tooltip("중첩 한 번마다 더해지는 흔들림 세기")]
        [SerializeField] private float _amplitudeGainPerStack = 0.05f;

        [Preserve, Inject] private CinemachineImpulseSource _impulseSource;
        [Preserve, Inject(true)] private PlayerCameraShakeData _shakeData;
        [Preserve, Inject(true)] private HitChannel _hitChannel;
        [Preserve, Inject(true)] private IStateTriggerSubscriber _triggerSubscriber;
        [Preserve, Inject(true)] private ICurrentStateProvider _currentStateProvider;

        private IDisposable _hitEventDisposable;
        private int _sameStateStack;

        private void Awake()
        {
            if (_hitChannel != null)
            {
                _hitEventDisposable = _hitChannel.Register(HandleHit);
            }

            _triggerSubscriber?.SubscribeTrigger(HandleTrigger);

            if (_currentStateProvider != null)
            {
                _currentStateProvider.StateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            _hitEventDisposable?.Dispose();
            _triggerSubscriber?.UnsubscribeTrigger(HandleTrigger);

            if (_currentStateProvider != null)
            {
                _currentStateProvider.StateChanged -= HandleStateChanged;
            }
        }

        // 실제 상태 전환일 때만 온다(같은 상태 재진입 제외) → 여기서 중첩 카운트를 리셋한다.
        private void HandleStateChanged(PlayerStateType state)
        {
            _sameStateStack = 0;
        }

        // 타격 성공: 지금 플레이어 상태(몇 번째 공격인지)에 맞는 셰이크를 쏜다.
        private void HandleHit(HitReport report)
        {
            Shake(_currentStateProvider != null ? _currentStateProvider.CurrentState : PlayerStateType.Locomotion);
        }

        // Damaged 트리거가 Hit 상태 진입 신호라 이것만 받는다.
        private void HandleTrigger(StateTriggerType trigger)
        {
            if (trigger == StateTriggerType.Damaged)
            {
                Shake(PlayerStateType.Hit);
            }
        }

        private void Shake(PlayerStateType state)
        {
            if (_impulseSource == null || _shakeData == null)
            {
                return;
            }

            if (!_shakeData.TryGetShake(state, out ShakeData shake))
            {
                return;
            }

            // 같은 상태에서 연속으로 들어온 타격은 카운트만 쌓고, 최대치를 넘으면 더 흔들지 않는다.
            _sameStateStack++;
            if (_sameStateStack > _maxSameStateStack)
            {
                return;
            }

            float amplitudeGain = shake.AmplitudeGain + _amplitudeGainPerStack * (_sameStateStack - 1);

            _impulseSource.ImpulseDefinition.ImpulseShape = shake.ImpulseShape;
            _impulseSource.ImpulseDefinition.ImpulseDuration = shake.Duration;
            _impulseSource.ImpulseDefinition.AmplitudeGain = amplitudeGain;
            _impulseSource.ImpulseDefinition.FrequencyGain = shake.FrequencyGain;
            _impulseSource.GenerateImpulseWithVelocity(new Vector3(shake.Velocity.x, shake.Velocity.y, 0f));
        }
    }
}
