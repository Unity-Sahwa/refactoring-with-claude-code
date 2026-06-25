namespace Refactoring
{
    // 상태 전환을 일으키는 사건. 입력·피격·물리·애니완료를 한 타입으로 통일한다.
    // 모든 사건은 StateMachine.SendTrigger(trigger) 하나로 들어온다.
    public enum StateTriggerType
    {
        Move,
        Dash,            // 대쉬 입력
        LockOnDash,      // 락온 대쉬 입력(외부 입력처리가 락온 판단해 발행)

        Attack,          // 공격 입력(콤보도 이 트리거)
        SpecialAttack,   // 특수 스킬 입력
        FinishAttack,    // 처형 스킬 입력

        Damaged,         // 피격(무적이면 외부가 아예 안 보냄)
        Died,            // 사망

        AnimationFinished, // 현재 애니 종료(애니 추적기가 발행)
    }
}
