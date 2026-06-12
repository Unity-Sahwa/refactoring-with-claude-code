using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Refactoring
{
    public interface IHitboxProvider
    {
        GameObject Rent(AssetReferenceGameObject key);

        void Return(GameObject instance);

        // 미리 캐싱해 둔 MeshRenderer들을 켜고 끈다(조정용 가시화).
        void SetMeshVisible(GameObject instance, bool visible);

        // 미리 캐싱해 둔 감지 컴포넌트를 돌려준다. 핸들러가 켤 때 전투값을 주입하는 데 쓴다.
        IHitboxDamageReporter GetReporter(GameObject instance);
    }
}
