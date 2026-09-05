using UnityEngine;

namespace Refactoring
{
    // 책임: 핸들러가 히트박스를 켜는 데 필요한 정보 계약. (핸들러는 구체 타입이 아니라 이 인터페이스에만 의존한다)
    public interface IPlayerHitbox
    {
        float Duration { get; }
        Vector3 Position { get; }
        Vector3 Rotation { get; }
        HitboxShape Shape { get; }
        Vector3 ShapeScale { get; }
        // 맞았을 때 줄 전투값 묶음
        CombatInfo Combat { get; }
    }
}
