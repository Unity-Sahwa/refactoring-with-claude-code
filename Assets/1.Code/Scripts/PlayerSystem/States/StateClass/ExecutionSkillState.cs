namespace Refactoring
{
    // 처형 스킬. 상태 전체가 무적(SO 이벤트로 외부가 처리)이라 피격 트리거는 들어오지 않는다.
    // 애니 끝나면 Idle로(베이스).
    public class ExecutionSkillState : CharacterState
    {
        // 특수 전환 없음 → 베이스 그대로 사용.
    }
}
