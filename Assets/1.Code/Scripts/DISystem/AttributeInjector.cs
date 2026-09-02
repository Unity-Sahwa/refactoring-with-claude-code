using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Refactoring
{
    // [흐름] 타입 수집(CollectType) → 씬 객체·SO 등록(RegisterInstances) → 어트리뷰트 필드에 주입(Inject)
    // [책임/핵심] 씬의 의존성을 모아, [Inject]가 붙은 필드에 자동으로 꽂아주는 DI 컨테이너.
    // [확장성] 새 주입 대상이 생겨도 필드에 어트리뷰트만 붙이면 됨. 이 클래스는 안 고친다.
    // [작동 예시] Awake() 이전에 CollectType() 호출 > 게임에 존재하는 필터링된 Type들 수집 > order가 -100인 Awake() 호출, Inject() > PlayerMovement의 `[Inject] IInputEventProvider` 필드에 주입

    
    public class AttributeInjector : MonoBehaviour
    {
        private class InjectField
        {
            public FieldInfo Field;
            public bool Optional;
        }
        private static AttributeInjector _instance;
        // static인 이유: CollectType이 static 메서드(RuntimeInitializeOnLoadMethod)라 여기서만 채울 수 있다.
        // 아래 _instanceMap은 씬마다 새로 채우는 값이라 인스턴스 필드로 둔다.
        private static Dictionary<Type, List<InjectField>> _injectFields = new(); //클래스 내의 어트리뷰트 정보를 저장
        private static Dictionary<Type, List<Type>> _classTypeMap = new(); // <클래스 타입, key 타입에 포함된 타입들>
        private Dictionary<Type, List<object>> _instanceMap = new(); // <주입 가능한 타입, 실제 등록된 인스턴스들(MonoBehaviour + ScriptableObject)>
        private MonoBehaviour[] _sceneObjects;

        // [왜] 런타임 리플렉션 비용을 감소시키기 위해, 1회만 타입 관계를 훑어 캐싱한다
        // 대원_Q: (출시 쯤에 구현)게임 시작시 한 번 호출되고 변수로 저장하게 만들면 다음 게임 시작부터는 안해도 되지 않을까?
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void CollectType()
        {
            // 도메인 리로드 안전장치
            _classTypeMap.Clear();
            _injectFields.Clear();

            var allTypes = Assembly.GetExecutingAssembly().GetTypes()
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

        // 어트리뷰트타입의 필드 중 [Inject]가 붙은 것만 찾아 캐싱한다
        private static void CollectInjectFields(Type type)
        {
            var fields = type.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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

        // 왜: 매 씬마다 존재하는 객체가 달라지므로 Awake()에서 Inject를 수행해야 함. (DontDestroy 못씀)
        // Project Settings > Script Execution Order > -100 필수
        void Awake()
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

        // SO는 씬 객체가 아니라서 FindObjects로 못 잡는다. IDataProvider를 통해서만 모을 수 있다.
        private void RegisterInstances()
        {
            _sceneObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var obj in _sceneObjects)
            {
                Register(obj);

                if (obj is not IDataProvider provider) continue;
                foreach (var data in provider.ProvideData())
                {
                    if (data == null)
                    {
                        Debug.LogWarning("SOContainer에서 Missing을 확인하세요");
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
                if (!_injectFields.TryGetValue(obj.GetType(), out var fields)) continue;

                foreach (var info in fields)
                {
                    InjectInstance(obj, info.Field, info.Optional);
                }
            }
        }

        // [Inject] 어트리뷰트용. 단일/리스트 타입의 변수만 사용하기 때문에 판별해서 객체를 주입.
        private void InjectInstance(object target, FieldInfo field, bool optional)
        {
            //어트리뷰트가 붙은 변수의 타입을 얻기 위함
            bool isList = field.FieldType.IsGenericType
                          && field.FieldType.GetGenericTypeDefinition() == typeof(List<>);
            Type wantType = isList ? field.FieldType.GetGenericArguments()[0] : field.FieldType;

            if (!_instanceMap.TryGetValue(wantType, out var instances) || instances.Count == 0)
            {
                Warn(target, field, $"{wantType.Name} 구현을 찾지 못함", optional);
                return;
            }

            if (isList)
            {
                var typedList = (IList)Activator.CreateInstance(field.FieldType);
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

        // 객체 주입 상태를 눈에 띄게 하기 위함.(필수 기능 미주입은 에러, 그 외 미주입은 경고)
        private void Warn(object target, FieldInfo field, string msg, bool optional)
        {
            string full = $"{target.GetType().Name}.{field.Name}: {msg}";
            if (optional)
            {
                Debug.LogWarning(full);
            }
            else
            {
                Debug.LogError(full);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
