using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 낙사 등으로 부활 지점을 다시 불러온다. 데미지는 EventDamage가 따로 처리하고,
    // 여기서는 지금 체력 그대로 저장 슬롯에 반영한 뒤 씬을 다시 띄운다.
    // 여러 번 떨어질 수 있으니 붙이는 EnterTrigger의 isLoop를 켜야 한다.
    public class EventFallRespawn : EventData
    {
        [Preserve, Inject(true)] private IHealthInfo _health;
        [Preserve, Inject(true)] private SaveSlotManager _slots;
        [Preserve, Inject(true)] private SlotLoadRunner _runner;

        public override void Execute()
        {
            if (_health == null || _slots == null || _runner == null)
            {
                return;
            }

            if (_health.Current <= 0)
            {
                // 체력이 0이면 사망 처리가 따로 진행되므로 여기서 씬을 다시 띄우지 않는다.
                return;
            }

            GameSaveData data = _slots.GetCurrentData();

            if (data == null)
            {
                return;
            }

            // 현재 체력을 그대로 슬롯에 적어 둔다. 칸을 새로 고르지 않고 지금 칸에 덮어쓴다.
            data.Hp = _health.Current;
            _slots.OverwriteCurrent(data);

            // LoadSlot은 같은 씬이어도 반드시 새로 띄운다. 방금 덮어쓴 칸을 그대로 읽으므로 체력이 따라온다.
            _runner.LoadSlot(_slots.CurrentSlot);
        }
    }
}
