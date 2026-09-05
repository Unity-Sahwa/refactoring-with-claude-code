using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 진행률 시점에 맞춰 씬에 등록된 IToggleTarget 오브젝트를 켜거나 끈다. (일회성이라 되돌리지 않는다)
    public class PlayerObjectToggleHandler : MonoBehaviour
    {
        [Preserve, Inject] private IPlayerStateEventSubscriber _eventSubscriber;
        [Preserve, Inject] private List<IToggleTarget> _toggleTargets;
        private readonly Dictionary<ToggleTargetKey, GameObject> _targets = new();
        private IDisposable _toggleEventDisposable;

        private void Awake()
        {
            _toggleEventDisposable = _eventSubscriber.Register(StateEventCategory.ObjectToggle, HandleToggle);

            foreach (var target in _toggleTargets)
            {
                _targets[target.Key] = target.Target;
            }
        }

        private void HandleToggle(IStartData data)
        {
            if (data is not IPlayerObjectToggle toggle)
            {
                Debug.LogError($"[PlayerObjectToggleHandler] IPlayerObjectToggle이 필요한데 {data?.GetType().Name ?? "null"}을 받음");
                return;
            }

            if (!_targets.TryGetValue(toggle.Key, out var target))
            {
                Debug.LogWarning($"[PlayerObjectToggleHandler] {toggle.Key}에 해당하는 ToggleTarget을 씬에서 찾지 못함");
                return;
            }

            target.SetActive(toggle.Activate);
        }

        private void OnDestroy()
        {
            _toggleEventDisposable?.Dispose();
        }
    }
}
