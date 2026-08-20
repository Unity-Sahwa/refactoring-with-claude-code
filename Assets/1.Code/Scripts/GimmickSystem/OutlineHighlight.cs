using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 락온 대상, 수정(BrokenObject) 등 강조가 필요한 오브젝트에 붙여서 실루엣 아웃라인을 켜고 끈다.
    // 원본 렌더링은 그대로 두고, 각 렌더러의 머티리얼 배열 끝에 아웃라인 머티리얼을 얹었다 뺐다 한다.
    public class OutlineHighlight : MonoBehaviour
    {
        [SerializeField] private Material _outlineMaterial;

        private Renderer[] _renderers;
        private Material[][] _originalMaterials;
        private bool _isOn;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _originalMaterials = new Material[_renderers.Length][];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _originalMaterials[i] = _renderers[i].sharedMaterials;
            }
        }

        public void SetOutline(bool isOn)
        {
            if (_isOn == isOn || _outlineMaterial == null) return;
            _isOn = isOn;

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                _renderers[i].sharedMaterials = isOn ? AppendOutline(_originalMaterials[i]) : _originalMaterials[i];
            }
        }

        private Material[] AppendOutline(Material[] original)
        {
            var materials = new List<Material>(original) { _outlineMaterial };
            return materials.ToArray();
        }
    }
}
