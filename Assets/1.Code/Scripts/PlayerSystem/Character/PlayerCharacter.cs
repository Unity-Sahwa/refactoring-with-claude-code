using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Refactoring
{
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField] private PlayerCharacterType type;
        public PlayerCharacterType Type => type;
        private readonly Dictionary<Type, Component> _componentCache = new Dictionary<Type, Component>();

        public T GetCharacterComponent<T>() where T : Component
        {
            var type = typeof(T);
            if(!_componentCache.TryGetValue(type,out var component))
            {
                component = GetComponent<T>();
                _componentCache[type] = component;
            }

            if(component == null)
            {
                Debug.LogError($"{type.Name}이 존재하지 않습니다");
            }

            return component as T;
        }
    }
}