using UnityEngine;

namespace Refactoring
{
    // 기믹으로 플레이어에게 데미지를 준다(낙사 구역 등). 실제 처리(무적/슈퍼아머/쿨다운/사망)는 PlayerDamageReceiver가 맡는다.
    // EventFallRespawn과 같은 EnterTrigger에 같이 붙여서 데미지 → 리스폰 순서로 실행한다.
    public class EventDamage : EventData
    {
        [SerializeField] private float _damage = 3f;

        [Inject(true)] private IPlayerDamageable _damageable;

        public override void Execute()
        {
            _damageable?.ApplyDamage(new DamageInfo { Amount = _damage });
        }
    }
}
