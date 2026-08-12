using UnityEngine;

namespace Refactoring
{
    // 낙사 구역. 플레이어가 닿으면(EnterTrigger가 Execute를 부름) 체력을 깎고 마지막 저장 지점으로 되돌린다.
    // 여러 번 떨어질 수 있으니 붙이는 EnterTrigger의 isLoop를 켜야 한다.
    public class EventFallRespawn : EventData
    {
        [SerializeField] private float _damage = 1f;

        [Inject(true)] private IHealthModifier _health;
        [Inject(true)] private SaveSlotManager _slots;
        [Inject(true)] private SlotLoadRunner _runner;

        public override void Execute()
        {
            if (_health == null || _slots == null || _runner == null)
            {
                return;
            }

            float remain = _health.Decrease(_damage);

            if (remain <= 0f)
            {
                // ponytail: 사망 처리가 아직 프로젝트에 없어서 비워 둠. 죽는 연출·처리가 정해지면 여기서 부른다.
                return;
            }

            GameSaveData data = _slots.GetCurrentData();

            if (data == null)
            {
                return;
            }

            // 깎인 체력을 슬롯에도 적어 둔다. 안 그러면 되돌아간 뒤 체력이 도로 차서 계속 떨어져도 손해가 없다.
            // 칸을 새로 고르지 않고 지금 칸에 덮어쓴다. 떨어진 것 때문에 저장 지점이 바뀌면 안 된다.
            data.Hp = remain;
            _slots.OverwriteCurrent(data);

            _runner.Begin(data);
        }
    }
}
