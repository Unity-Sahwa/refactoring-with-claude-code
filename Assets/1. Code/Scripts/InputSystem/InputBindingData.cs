// 모든 입력 액션을 중앙 관리하는 ScriptableObject.
// InputActionAsset을 보유하며, InputActionType enum과 액션 이름을 매핑.
// 이름이 enum과 동일하면 _mappings 설정 없이도 자동 탐색.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    [Serializable]
    public struct InputActionMapping
    {
        public InputActionType ActionType;

        [Tooltip("InputActionAsset 내 액션 이름. 비워두면 enum 이름으로 자동 탐색.")]
        public string ActionName;
    }

    [CreateAssetMenu(menuName = "Refactoring/InputBindingData", fileName = "InputBindingData")]
    public class InputBindingData : ScriptableObject
    {
        [SerializeField]
        private InputActionAsset _actionAsset;

        [Tooltip("InputActionType → 액션 이름 커스텀 매핑. 비워두면 enum 이름으로 자동 탐색.")]
        [SerializeField]
        private List<InputActionMapping> _mappings = new();

        private Dictionary<InputActionType, InputAction> _actionCache;

        private void OnEnable()
        {
            BuildCache();
            _actionAsset?.Enable();
        }

        private void OnDisable()
        {
            _actionAsset?.Disable();
        }

        private void BuildCache()
        {
            _actionCache = new Dictionary<InputActionType, InputAction>();

            foreach (var mapping in _mappings)
            {
                string actionName = string.IsNullOrEmpty(mapping.ActionName)
                    ? mapping.ActionType.ToString()
                    : mapping.ActionName;

                InputAction action = _actionAsset?.FindAction(actionName);

                if (action != null)
                {
                    _actionCache[mapping.ActionType] = action;
                }
            }
        }

        public InputAction GetAction(InputActionType actionType)
        {
            if (_actionCache != null && _actionCache.TryGetValue(actionType, out InputAction cached))
            {
                return cached;
            }

            // 커스텀 매핑 없으면 enum 이름으로 직접 탐색
            return _actionAsset?.FindAction(actionType.ToString());
        }

        public string SaveOverrideJson()
        {
            return _actionAsset?.SaveBindingOverridesAsJson() ?? string.Empty;
        }

        public void ApplyOverrideJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            _actionAsset?.LoadBindingOverridesFromJson(json);
        }
    }
}
