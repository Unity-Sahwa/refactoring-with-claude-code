using UnityEngine;

namespace Refactoring
{
    // 책임: 미리 만들어 둔 이펙트를 대여·반납 형태로 제공하는 계약.
    public interface IEffectProvider
    {
        GameObject Rent(EffectId id);

        void Return(GameObject instance);
    }
}
