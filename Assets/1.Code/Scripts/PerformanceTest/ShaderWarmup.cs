using UnityEngine;

namespace Refactoring
{
    // 로딩/부트스트랩 씬에 빈 오브젝트 만들어 붙이고 Inspector에 .shadervariants 에셋 연결.
    // Awake 시점에 GPU 셰이더 컴파일을 미리 끝내서 첫 등장 오브젝트의 스터터를 없앤다.
    public class ShaderWarmup : MonoBehaviour
    {
        [SerializeField]
        private ShaderVariantCollection _variantCollection;

        private void Awake()
        {
            if (_variantCollection == null)
            {
                Debug.LogWarning("[ShaderWarmup] ShaderVariantCollection이 연결되지 않음.");
                return;
            }

            _variantCollection.WarmUp();
        }
    }
}
