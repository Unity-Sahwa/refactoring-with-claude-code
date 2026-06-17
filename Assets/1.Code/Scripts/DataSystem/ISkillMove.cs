using UnityEngine;

namespace Refactoring
{
    // 핸들러가 스킬 이동을 켜는 데 필요한 정보 계약.
    // 핸들러는 구체 타입(SkillMoveDataEntry)이 아니라 이 인터페이스에만 의존한다.
    public interface ISkillMove
    {
        float Duration { get; }      // 이동을 켜고 끄기까지 지속시간(초)
        Vector3 Direction { get; }   // 캐릭터 로컬 기준 이동 방향(정규화 전이어도 됨)
        float Speed { get; }         // 초당 이동 속도
    }
}
