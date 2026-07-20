using UnityEngine;

namespace Refactoring
{
    // 핸들러가 히트박스를 켜는 데 필요한 정보 계약.
    // 핸들러는 구체 타입(HitboxDataEntry)이 아니라 이 인터페이스에만 의존한다.
    // 충돌 판정(피해량 등)은 별도 시스템에서 다루므로 여기에 포함하지 않는다.
    public interface IPlayerHitbox
    {
        bool UntilFinish { get; }
        float Duration { get; }
        Vector3 Position { get; }
        Vector3 Rotation { get; }   // 오일러각
        Vector3 Scale { get; }
        HitboxId HitboxId { get; }  // 카탈로그(HitboxCatalog)에서 프리팹을 찾을 이름표
        HitboxAttachPointType AttachKey { get; }
        bool StopInPlace { get; }      // 도중에 부모에서 떨어져 그 자리 정지할지
        float StopTime { get; }         // 멈추는 시점(초)
        bool ShowMesh { get; }          // true면 히트박스의 MeshRenderer를 켜서 눈에 보이게 함(조정용)
    }
}
