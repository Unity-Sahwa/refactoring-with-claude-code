using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 외부에서 ApplyDamage로 전달되는 데이터로 체력감소 및 플레이어 상태를 전환
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDamageReceiver : MonoBehaviour, IDamageable, IPlayerDamageable
    {
        [Inject(true)] private IHealthModifier _health;
        [Inject(true)] private IStateTriggerRaiser _triggerRaiser;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;
        [Tooltip("피격 후 이 시간(초) 동안은 추가 피격을 무시한다. 슈퍼아머 중 다단히트로 순삭당하는 것 방지")]
        [SerializeField] private float _damageCooldown = 0.5f;
        private IDisposable _invincibleEventDisposable;
        private IDisposable _superArmorEventDisposable;
        private bool _invincible;
        private bool _superArmor;
        private bool _dead;
        private float _lastDamageTime = -999f;

        // 사망 순간 딱 한 번 발행. UI 연출/씬 전환 등은 이걸 구독해서 처리(PlayerDeathTrigger 참고).
        public event Action OnPlayerDied;

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Player");

            if (_eventSubscriber == null) 
            {
                return;
            }

            // 게이트로 등록하면 채널이 End/Reset 시 알아서 닫아준다(끄기 누락으로 영구 무적 되는 버그 자체가 불가능).
            _invincibleEventDisposable = _eventSubscriber.Register(StateEventCategory.Invincible, HandleInvincibleOn, HandleInvincibleClose);
            _superArmorEventDisposable = _eventSubscriber.Register(StateEventCategory.SuperArmor, HandleSuperArmorOn, HandleSuperArmorClose);
        }
        private void OnDestroy()
        {
            _invincibleEventDisposable?.Dispose();
            _superArmorEventDisposable?.Dispose();
        }
        public void ApplyDamage(DamageInfo info) // 적 히트박스가 호출한다.
        {
            if (_invincible || _dead)
            {
                return;
            }

            // 슈퍼아머 등 Hit 상태로 안 넘어가는 경우, 지속 피격(TriggerStay)에 매 프레임 데미지가 쌓이는 것 방지
            if (Time.time - _lastDamageTime < _damageCooldown)
            {
                return;
            }
            _lastDamageTime = Time.time;

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
                    _dead = true;
                    _triggerRaiser?.RaiseTrigger(StateTriggerType.Died);
                    OnPlayerDied?.Invoke();
                }
                else if (!_superArmor)
                {
                    // 넉백은 Damaged 상태의 로컬 이동(뒤로)이라 캐릭터가 보는 방향이 곧 넉백 방향이다.
                    // 피격 순간 공격자 쪽을 보게 돌려서 결과적으로 "공격자 반대"로 밀리게 만든다.
                    // ponytail: 회전으로 방향을 대신함. 회전 없이 밀어야 하면 넉백 전용 IVelocitySource가 필요.
                    LookAtDamager(info.Damager);
                    _triggerRaiser?.RaiseTrigger(StateTriggerType.Damaged);
                }
            }
        }
        // 수평 성분만 사용한다(위/아래에서 맞아도 캐릭터가 눕지 않게).
        private void LookAtDamager(GameObject damager)
        {
            if (damager == null || damager == gameObject)
            {
                return;
            }

            Vector3 toDamager = damager.transform.position - transform.position;
            toDamager.y = 0f;
            if (toDamager.sqrMagnitude < 0.0001f)   // 완전히 겹쳐 있으면 방향을 못 정하니 그대로 둔다.
            {
                return;
            }

            transform.rotation = Quaternion.LookRotation(toDamager);
        }

        private void HandleInvincibleOn(IStartData data) => _invincible = true;
        private void HandleInvincibleClose(CloseEventType reason) => _invincible = false;
        private void HandleSuperArmorOn(IStartData data) => _superArmor = true;
        private void HandleSuperArmorClose(CloseEventType reason) => _superArmor = false;
    }
}
