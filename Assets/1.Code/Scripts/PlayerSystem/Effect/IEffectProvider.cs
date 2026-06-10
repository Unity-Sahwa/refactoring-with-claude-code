using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Refactoring
{
    public interface IEffectProvider
    {
        GameObject Rent(AssetReferenceGameObject key);

        void Return(GameObject instance);
    }
}
