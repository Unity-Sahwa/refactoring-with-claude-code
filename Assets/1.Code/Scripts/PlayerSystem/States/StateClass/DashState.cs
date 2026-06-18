namespace Refactoring
{
    // 대쉬 상태(일반/락온 공용). 짧은 가속 이동.
    // 대쉬 중에는 콤보/입력 전환이 없다. 애니가 끝나면 Idle로(베이스 처리).
    // 이동 데이터(SkillMove)는 SO에 들어있어 락온/일반이 다르게 동작한다.
    public class DashState : CharacterState
    {
        // 특수 전환 없음 → 공통(피격/사망/애니완료)만 처리하는 베이스 그대로 사용.
    }
}
