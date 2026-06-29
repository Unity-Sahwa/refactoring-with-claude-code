using System;
using UnityEngine;

namespace Refactoring
{
    // 역할 : 이번 프레임에서 이동을 증감시키는 요인들의 계약
    //       (일반이동·스킬이동·중력 등 각 요인이 이 인터페이스를 구현한다)
    // 왜 존재? : Mover는 구체 요인을 몰라도 되고, 계약된 모든 요인의 속도를 똑같이 합산만 하기 위함.(요인 추가/교체가 쉬움).
    public interface IVelocitySource : IDisposable
    {
        // 해당 요인의 이번 프레임 속도(월드 기준, m/s). Mover가 모두 더해 DeltaTime을 곱해 이동시킨다.
        Vector3 Evaluate(in MoveParams frame);

        void OnCharacterChanged();
    }
}
