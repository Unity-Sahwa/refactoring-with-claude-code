namespace Refactoring
{
    // 기본 공격 2타. 공격 입력이 또 오면 3타로 콤보 연결.
    public class NormalAttack2State : CharacterState
    {
        public override bool TryHandleTrigger(StateTriggerType trigger, out PlayerStateType next)
        {
            if (trigger == StateTriggerType.Attack)
            {
                next = PlayerStateType.NormalAttack3;
                return true;
            }

            return base.TryHandleTrigger(trigger, out next);
        }
    }
}
