using UnityEngine;

namespace Refactoring
{
    // 책임: 캐릭터 자신의 컴포넌트를 찾아 돌려주는 계약.
    public interface ICharacterComponentSource
    {
        T GetCharacterComponent<T>() where T : Component;
    }
}
