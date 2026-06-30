using UnityEngine;

namespace Refactoring
{
    // 책임: 외부에서 ApplyDamage로 전달되는 데이터로 체력감소 및 플레이어 상태를 전환

    public class PlayerDamageReceiver : MonoBehaviour, IDamageable
    {
        [Inject(true)] private IHealthModifier _health;
        [Inject(true)] private IStateTriggerRaiser _triggerRaiser;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        private bool _invincible;
        private bool _superArmor;

        private void Awake()
        {
            if (_eventSubscriber == null) return;

            _eventSubscriber.Subscribe(StateEventCategory.Invincible, HandleInvincibleOn);
            _eventSubscriber.SubscribeEnd(StateEventCategory.Invincible, HandleInvincibleOff);

            _eventSubscriber.Subscribe(StateEventCategory.SuperArmor, HandleSuperArmorOn);
            _eventSubscriber.SubscribeEnd(StateEventCategory.SuperArmor, HandleSuperArmorOff);

            // 상태가 강제 중단될 때 끄는 이벤트가 누락돼 영구 무적이 되는 걸 막는다.
            _eventSubscriber.SubscribeReset(HandleReset);
        }
        private void OnDestroy()
        {
            if (_eventSubscriber == null) return;

            _eventSubscriber.Unsubscribe(StateEventCategory.Invincible, HandleInvincibleOn);
            _eventSubscriber.UnsubscribeEnd(StateEventCategory.Invincible, HandleInvincibleOff);

            _eventSubscriber.Unsubscribe(StateEventCategory.SuperArmor, HandleSuperArmorOn);
            _eventSubscriber.UnsubscribeEnd(StateEventCategory.SuperArmor, HandleSuperArmorOff);

            _eventSubscriber.UnsubscribeReset(HandleReset);
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
        private void HandleInvincibleOff() => _invincible = false;
        private void HandleSuperArmorOn(PlayerCharacter source, IStartData data) => _superArmor = true;
        private void HandleSuperArmorOff() => _superArmor = false;
        private void HandleReset()
        {
            _invincible = false;
            _superArmor = false;
        }
    }
}
