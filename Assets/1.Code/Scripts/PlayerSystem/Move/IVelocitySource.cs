using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 이동 속도를 만드는 요인(일반이동·스킬이동·중력)의 계약. Mover는 구체 요인을 모른 채 합산만 한다.
    public interface IVelocitySource : IDisposable
    {
        // 해당 요인의 이번 프레임 속도(월드 기준, m/s). Mover가 모두 더해 DeltaTime을 곱해 이동시킨다.
        Vector3 Evaluate(in MoveParams frame);

        void OnCharacterChanged();
    }
}
