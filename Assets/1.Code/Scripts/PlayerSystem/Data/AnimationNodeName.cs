namespace Refactoring
{
    // 애니메이터 컨트롤러의 state(노드) 이름을 그대로 enum으로 옮긴 것.
    // 인스펙터에서 문자열을 손으로 적지 않고 드롭다운으로 고르게 하기 위함.
    // 주의: enum 멤버 이름 = 실제 노드 이름과 정확히 일치해야 ToString()으로 CrossFade에 쓸 수 있다.
    //       (C# PascalCase 컨벤션보다 노드 이름 일치를 우선해 소문자 시작 그대로 둔다.)
    // 캐릭터별로 노드 이름이 다르고(human*/animal*) 구성도 달라, 한 enum에 혼재한다. 추후 정리 대상.
    public enum AnimationNodeName
    {
        Locomotion,   // H/A 공통

        Hit,
        Dead,

        FrontDash,
        BackDash,

        NormalAttack1,
        NormalAttack2,
        NormalAttack3,

        SpecialAttack,
        
        FinishAttack
    }
}
