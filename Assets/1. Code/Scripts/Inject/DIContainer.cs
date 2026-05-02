using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 씬 내 MonoBehaviour를 자동 수집하고 IInjectable 구현체에 주입
    public class DIContainer : MonoBehaviour
    {
        private Dictionary<Type, List<object>> _targets = new();

        //SETTINGS: Project Settings > Script Execution Order > -100
        private void Awake()
        {
            Inject();
        }

        private void Inject()
        {
            //씬의 모든 MonoBehaviour를 가져옴 (비활성 포함)
            MonoBehaviour[] injectableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var item in injectableObjects)
            {
                //interfaceType에 맞게 target 들을 dictionary 변수인 _targets에 저장
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

            // 의존성 주입 요청자들이 요청한 인터페이스 구현체들을 _targets에서 key로 찾기
            // filteredTarges에 담아 요청자의 메서드를 호출할 때 매개변수 인수로 전달
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
