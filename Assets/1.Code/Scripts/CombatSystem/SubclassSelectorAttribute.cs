using UnityEngine;

namespace Refactoring
{
    // [SerializeReference] 필드/리스트에 함께 붙이면, 인스펙터에서 구현 타입을 드롭다운으로 골라 넣을 수 있다.
    // 예: [SerializeReference, SubclassSelector] private List<IHitReaction> reactions = new();
    public class SubclassSelectorAttribute : PropertyAttribute { }
}
