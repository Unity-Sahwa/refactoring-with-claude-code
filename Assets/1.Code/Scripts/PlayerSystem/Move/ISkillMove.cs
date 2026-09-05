using UnityEngine;

namespace Refactoring
{
    // 책임: 핸들러가 스킬 이동을 켜는 데 필요한 정보 계약. (핸들러는 구체 타입이 아니라 이 인터페이스에만 의존한다)
    public interface ISkillMove
    {
        // 이동을 켜고 끄기까지 지속시간(초)
        float Duration { get; }
        // 캐릭터 로컬 기준 이동 방향(정규화 전이어도 됨)
        Vector3 Direction { get; }
        // 초당 이동 속도
        float Speed { get; }
    }
}
