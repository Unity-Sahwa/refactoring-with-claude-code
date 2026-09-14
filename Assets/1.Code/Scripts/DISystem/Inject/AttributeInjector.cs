using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Refactoring
{
    // 책임: [Inject]가 붙은 필드에 씬의 의존성을 찾아 꽂아주는 DI 컨테이너.
    // 흐름: 타입 수집(CollectType) → 씬 객체·SO 등록(RegisterInstances) → 어트리뷰트 필드에 주입(Inject)
    public class AttributeInjector : MonoBehaviour
    {
        private static AttributeInjector _instance;

        // static인 이유: 게임 시작시 전체 타입을 저장하고 각 Scene의 Awake() 마다 활용하기 위함.
        private static Dictionary<Type, List<InjectField>> _injectFields = new(); // 클래스 내의 어트리뷰트 정보를 저장
        private static Dictionary<Type, List<Type>> _classTypeMap = new(); // <클래스 타입, key 타입에 포함된 타입들>

        // 씬에 존재하는 타입별 인스턴스 모음
        private Dictionary<Type, List<object>> _instanceMap = new(); 

        private MonoBehaviour[] _sceneObjects;

        // 왜 Awake()에서 Inject 하는지? : 매 씬마다 존재하는 객체가 달르므로 Awake()에서 주입함.
        // Project Settings > Script Execution Order > -100 필수
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            RegisterInstances();
            Inject();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        // [왜] 런타임 리플렉션 비용을 감소시키기 위해, 어셈블리 로드 이후(Awake 이전) 1회만 타입을 훑어 캐싱한다
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void CollectType()
        {
            // 도메인 리로드 안전장치
            _classTypeMap.Clear();
            _injectFields.Clear();

            IEnumerable<Type> allTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "Refactoring" && t.IsClass && !t.IsAbstract);

            foreach (var type in allTypes)
            {
                var bases = new List<Type>(type.GetInterfaces());
                if (type.BaseType != null && type.BaseType.IsAbstract)
                {
                    bases.Add(type.BaseType);
                }
                _classTypeMap[type] = bases;

                CollectInjectFields(type);
            }
        }

        private static void CollectInjectFields(Type type)
        {
            // 주의: 자식 클래스를 통해 부모 클래스의 필드를 Inject 하기 위해선 private가 아닌 protected, public 필드로 둬야 한다.
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                var inject = field.GetCustomAttribute<InjectAttribute>();
                if (inject != null)
                {
                    AddInjectField(type, new InjectField { Field = field, Optional = inject.Optional });
                }
            }
        }


        private static void AddInjectField(Type type, InjectField info)
        {
            if (!_injectFields.ContainsKey(type))
            {
                _injectFields[type] = new List<InjectField>();
            }
            _injectFields[type].Add(info);
        }

        // SO는 씬 객체가 아니라서 FindObjects로 못 잡는다. IDataProvider를 통해서만 모을 수 있다.
        private void RegisterInstances()
        {
            _sceneObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var obj in _sceneObjects)
            {
                Register(obj);

                if (obj is not IDataProvider provider)
                {
                    continue;
                }

                foreach (var data in provider.ProvideData())
                {
                    if (data == null)
                    {
                        Debug.LogWarning("DataContainer에서 Missing을 확인하세요");
                        continue;
                    }
                    Register(data);
                }
            }
        }

        // 인터페이스/추상 타입도 키로 등록해야 Interface/Abstract 타입 필드로 주입받을 수 있다.
        private void Register(object instance)
        {
            var type = instance.GetType();
            AddToMap(type, instance);

            if (_classTypeMap.TryGetValue(type, out var bases))
            {
                foreach (var b in bases)
                {
                    AddToMap(b, instance);
                }
            }
        }

        private void AddToMap(Type key, object instance)
        {
            if (!_instanceMap.ContainsKey(key))
            {
                _instanceMap[key] = new List<object>();
            }
            _instanceMap[key].Add(instance);
        }

        // CollectType에서 미리 캐싱해둔 필드 정보를 쓰므로, 여기선 리플렉션 재조회가 없다
        private void Inject()
        {
            foreach (var obj in _sceneObjects)
            {
                if (!_injectFields.TryGetValue(obj.GetType(), out var fields))
                {
                    continue;
                }

                foreach (var info in fields)
                {
                    InjectInstance(obj, info.Field, info.Optional);
                }
            }
        }

        // [Inject] 어트리뷰트용. 단일/리스트 타입의 변수만 사용하기 때문에 판별해서 객체를 주입.
        private void InjectInstance(object target, FieldInfo field, bool optional)
        {
            // 어트리뷰트가 붙은 변수의 타입을 얻기 위함
            bool isList = field.FieldType.IsGenericType
                          && field.FieldType.GetGenericTypeDefinition() == typeof(List<>);
            Type wantType = isList ? field.FieldType.GetGenericArguments()[0] : field.FieldType;

            if (!_instanceMap.TryGetValue(wantType, out List<object> instances) || instances.Count == 0)
            {
                // 필수 기능 미주입은 에러, 그 외는 경고로 눈에 띄게 한다.
                string msg = $"{target.GetType().Name}.{field.Name}: {wantType.Name} 구현을 찾지 못함";
                if (optional)
                {
                    Debug.LogWarning(msg);
                }
                else
                {
                    Debug.LogError(msg);
                }

                return;
            }

            if (isList)
            {
                IList typedList = (IList)Activator.CreateInstance(field.FieldType);
                foreach (var item in instances)
                {
                    typedList.Add(item);
                }
                field.SetValue(target, typedList);
            }
            else
            {
                if (instances.Count > 1)
                {
                    Debug.LogWarning($"{target.GetType().Name}.{field.Name}: {wantType.Name} 구현이 {instances.Count}개. 첫 번째만 주입.");
                }
                field.SetValue(target, instances[0]);
            }
        }

        // 이 클래스 안에서만 도는 값 묶음이라 프로퍼티 없이 public 필드로 둔다.
        private class InjectField
        {
            public FieldInfo Field;
            public bool Optional;
        }
    }
}
