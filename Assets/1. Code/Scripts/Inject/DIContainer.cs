using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 씬 내 MonoBehaviour를 자동 수집하고 IInjectable 구현체에 주입
    public class DIContainer : MonoBehaviour
    {
        private Dictionary<Type, List<object>> _targets = new();

        private void Awake()
        {
            Inject();
        }

        private void Inject()
        {
            MonoBehaviour[] injectableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var item in injectableObjects)
            {
                if (item is IInjectTarget target)
                {
                    foreach (var interfaceType in target.InterfaceTypes)
                    {
                        if (!_targets.ContainsKey(interfaceType))
                        {
                            _targets[interfaceType] = new List<object> { item };
                        }
                        else
                        {
                            _targets[interfaceType].Add(item);
                        }
                    }
                }
            }

            foreach (var item in injectableObjects)
            {
                if (item is IInjectRequester requester)
                {
                    var filteredTargets = new Dictionary<Type, List<object>>();

                    foreach (var type in requester.TargetTypes)
                    {
                        if (_targets.TryGetValue(type, out var objs))
                        {
                            filteredTargets[type] = objs;
                        }
                    }
                    requester.Inject(filteredTargets);
                }
            }
        }
    }
}
