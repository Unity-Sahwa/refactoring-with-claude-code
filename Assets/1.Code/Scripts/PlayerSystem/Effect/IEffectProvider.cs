using UnityEngine;

namespace Refactoring
{
    public interface IEffectProvider
    {
        GameObject Rent(EffectId id);

        void Return(GameObject instance);
    }
}
