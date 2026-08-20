using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 역할: 이벤트 발생 시 지정한 렌더러 머티리얼 배열 끝에 아웃라인 머티리얼을 붙인다.
    public class EventOutlineOn : EventData
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Material _outlineMaterial;

        public override void Execute()
        {
            if (_renderer == null || _outlineMaterial == null) return;

            var materials = new List<Material>(_renderer.sharedMaterials);
            if (materials.Contains(_outlineMaterial)) return;

            materials.Add(_outlineMaterial);
            _renderer.sharedMaterials = materials.ToArray();
        }
    }
}
