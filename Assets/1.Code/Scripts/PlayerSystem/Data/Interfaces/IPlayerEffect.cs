using UnityEngine;

namespace Refactoring
{
    // 핸들러가 이펙트를 켜는 데 필요한 정보 계약.
    // 핸들러는 구체 타입(SkillEffectDataEntry)이 아니라 이 인터페이스에만 의존한다.
    public interface IPlayerEffect
    {
        bool UntilFinish { get; }
        float Duration { get; }
        Vector3 Position { get; }
        Vector3 Rotation { get; }   // 오일러각
        Vector3 Scale { get; }
        EffectId EffectId { get; }  // 카탈로그(EffectCatalog)에서 프리팹을 찾을 이름표
        EffectAttachPointType AttachKey { get; }
        bool StopInPlace { get; }      // 도중에 부모에서 떨어져 그 자리 정지할지
        float StopTime { get; }         // 멈추는 시점(초)
    }
}
