using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 책임: 캐릭터 자신의 종류를 알리고, 자기 컴포넌트를 찾아 캐시해 돌려준다.
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterType type;

        private readonly Dictionary<Type, Component> _componentCache = new Dictionary<Type, Component>();

        public PlayerCharacterType Type => type;

        public T GetCharacterComponent<T>() where T : Component
        {
            Type componentType = typeof(T);
            if (!_componentCache.TryGetValue(componentType, out Component component))
            {
                component = GetComponent<T>();
                _componentCache[componentType] = component;
            }

            // 없으면 없는 대로 null을 돌려준다. 그게 오류인지는 부르는 쪽이 판단한다.
            return component as T;
        }
    }
}
