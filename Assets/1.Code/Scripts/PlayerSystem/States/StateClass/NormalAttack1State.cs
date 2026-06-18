namespace Refactoring
{
    // 기본 공격 1타. 공격 입력이 또 오면 2타로 콤보 연결.
    public class NormalAttack1State : CharacterState
    {
        public override bool TryHandleTrigger(StateTriggerType trigger, out PlayerStateType next)
        {
            if (trigger == StateTriggerType.Attack)
            {
                next = PlayerStateType.NormalAttack2;
                return true;
            }

            return base.TryHandleTrigger(trigger, out next);
        }
    }
}
