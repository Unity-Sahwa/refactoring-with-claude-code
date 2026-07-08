using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 외부에서 ApplyDamage로 전달되는 데이터로 체력감소 및 플레이어 상태를 전환
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        [Inject(true)] private IHealthModifier _health;
        [Inject(true)] private IStateTriggerRaiser _triggerRaiser;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        private IDisposable _invincibleEventDisposable;
        private IDisposable _superArmorEventDisposable;
        private bool _invincible;
        private bool _superArmor;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Player");

            if (_eventSubscriber == null) 
            {
                return;
            }

            // 게이트로 등록하면 채널이 End/Reset 시 알아서 닫아준다(끄기 누락으로 영구 무적 되는 버그 자체가 불가능).
            _invincibleEventDisposable = _eventSubscriber.RegisterEventSwitch(StateEventCategory.Invincible, HandleInvincibleOn, HandleInvincibleClose);
            _superArmorEventDisposable = _eventSubscriber.RegisterEventSwitch(StateEventCategory.SuperArmor, HandleSuperArmorOn, HandleSuperArmorClose);
        }
        private void OnDestroy()
        {
            _invincibleEventDisposable?.Dispose();
            _superArmorEventDisposable?.Dispose();
        }
        public void ApplyDamage(DamageInfo info) // 적 히트박스가 호출한다.
        {
            if (_invincible) 
            {
                return;
            }

            //체력 클래스가 없어도 리액션은 볼 수 있어야 함.
            float remaining = 100;
            if (_health != null)
            {
                remaining = _health.Decrease(info.Amount);
            }

            if (_triggerRaiser != null)
            {
                if (remaining <= 0f)
                {
                    _triggerRaiser?.RaiseTrigger(StateTriggerType.Died);
                }
                else if (!_superArmor)
                {
                    _triggerRaiser?.RaiseTrigger(StateTriggerType.Damaged);
                }
            }
        }
        private void HandleInvincibleOn(PlayerCharacter source, IStartData data) => _invincible = true;
        private void HandleInvincibleClose(CloseEventType reason) => _invincible = false;
        private void HandleSuperArmorOn(PlayerCharacter source, IStartData data) => _superArmor = true;
        private void HandleSuperArmorClose(CloseEventType reason) => _superArmor = false;
    }
}
