using UnityEngine;

namespace Refactoring
{
    // 역할: 씬에 미리 배치된 UI 오브젝트에 부착하여 어떤 효과를 줄지 미리 수치 작성.
    // 효과가 여러 개일 경우, UIEffectTarget을 개수만큼 부착하기
    public class UIEffectTarget : MonoBehaviour
    {
        [SerializeField] private UIEffectId id;
        [SerializeReference] private UIEffectConfig config; // EffectConfigDrawer가 드롭다운 그려줌(방식+수치)

        public UIEffectId Id => id;
        public UIEffectConfig Config => config;
        public GameObject Target => gameObject;
    }
}
