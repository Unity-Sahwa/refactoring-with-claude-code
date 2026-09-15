using UnityEngine;

namespace Refactoring
{
    // 책임: 켜고 끌 씬 오브젝트 하나를 key와 함께 알린다.
    public interface IToggleTarget
    {
        ToggleTargetKey Key { get; }
        GameObject Target { get; }
    }
}
