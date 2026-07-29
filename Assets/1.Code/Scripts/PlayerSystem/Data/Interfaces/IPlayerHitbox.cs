using UnityEngine;

namespace Refactoring
{
    public interface IPlayerHitbox
    {
        float Duration { get; }
        Vector3 Position { get; }
        Vector3 Rotation { get; }
        HitboxShape Shape { get; }
        Vector3 ShapeScale { get; }
        CombatInfo Combat { get; }      // 맞았을 때 줄 전투값 묶음
    }
}
