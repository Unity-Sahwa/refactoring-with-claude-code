using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace Refactoring
{
    //Project Settings > Script Execution Order > -100
    public class DependencyInjector : MonoBehaviour
    {
        private static DependencyInjector _instance;
        //리플렉션에서 어떤 클래스 타입이 어떤 인터페이스 타입을 가졌는지 저장. Dictionary<인터페이스, 클래스 리스트>
        private static Dictionary<Type, List<Type>> interfaceTypeMap = new(); 
        private static Dictionary<Type, List<Type>> classTypeMap = new(); 
        private static Dictionary<Type, List<Type>> abstractTypeMap = new(); 
        
        private MonoBehaviour[] injectableObjects;
        private Dictionary<Type, List<object>> interfaceMap = new(); 
        private Dictionary<Type, List<object>> abstractMap = new(); 
        private List<ScriptableObject> dataList = new();
        private Dictionary<Type, List<ScriptableObject>> dataMapByInterface = new();
        

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void CollectType()
        {
            //기존 구현: [InjectAttribute]를 가진 클래스를 순회해서 interfaceTypeMap 채우기 -> 클래스마다 [Inject] 작성이 번거로워 namespace 기준으로 리플렉션 실시
            //기존 구현 장점: 내가 타겟한 class(일반, SO, abstract, interface)나 변수들에 정보를 메모하고, 리플렉션으로 가져올 수 있다.
            //기존 구현 단점: 번거롭다. namespace 적는것도 가끔 깜빡하는데 [Inject]는 강제성이 없어서 그냥 지나갈때가 많다. 
            //개선 : namespace를 기준으로 모두 순회. Refactoring이라는 namespace 내에는 모두 내가 구현한 것이기 때문에 순회범위가 적당하다.
            
            //현재 어셈블리에서 namespace가 Refactoring인 것들 중, 모든 클래스 타입 가져오기
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes()
                                .Where(t => t.Namespace == "Refactoring" && t.IsClass && !t.IsAbstract)
                                .ToArray();

            foreach (var type in allTypes)
            {
                var types = new List<Type>(type.GetInterfaces());
                if(type.BaseType != null && type.BaseType.IsAbstract)
                {
                    types.Add(type.BaseType);
                }

                classTypeMap[type] = types;
            }

            foreach (var (classType, types) in classTypeMap)
            {
                foreach (var type in types)
                {
                    var map = type.IsInterface ? interfaceTypeMap : abstractTypeMap;

                    if(!map.ContainsKey(type))
                    {
                        map[type] = new List<Type>();
                    }
                    map[type].Add(classType);
                }
            }
        }


        void Awake()
        {
            if(_instance != null && _instance != this)
            {
                Destroy(gameObject); 
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeMaps();
 
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void InitializeMaps()
        {
            RegisterMonoBehaviours();
            RegisterImplementations();
            RegisterSO();

            InjectImplementations();
            InjectSO();
            InjectAbstractType();
        }
        private void RegisterMonoBehaviours()
        {
            injectableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        //씬에 있는 인스턴스의 타입과 classTypeMap을 매치시켜 interfaceMap, abstractMap에 인스턴스 채우기
        private void RegisterImplementations()
        {
            foreach (var obj in injectableObjects)
            {
                var objType = obj.GetType();

                if (!classTypeMap.ContainsKey(objType))
                {
                    Debug.LogWarning($"DependencyInjector.RegisterInterfaceMap: classTypeMap에 {objType.Name}가 없습니다.");
                    continue;
                }

                foreach (var type in classTypeMap[objType])
                {
                    if(type.IsInterface)
                    {
                        if (!interfaceMap.ContainsKey(type))
                        {
                            interfaceMap[type] = new List<object>();
                        }
                        interfaceMap[type].Add(obj);
                    }
                    else if(type.IsAbstract)
                    {
                        if(!abstractMap.ContainsKey(type))
                        {
                            abstractMap[type] = new List<object>();
                        }
                        abstractMap[type].Add(obj);
                    }
                    else
                    {
                        Debug.LogWarning($"RegisterImplementations: 예외발생");
                    }
                }
            }
        }

        //SO 제공자(IDataProvider)를 통해 데이터 등록
        private void RegisterSO()
        {
            //SO데이터 제공자(interfaceMap[IDataProvider]) 로부터 데이터 받아서 dataMap에 넣기
            if (!interfaceMap.TryGetValue(typeof(IDataProvider), out var targets)) return;

            foreach (var target in targets)
            {
                IDataProvider provider = (IDataProvider)target;
                dataList.AddRange(provider.ProvideData());
            }

            //dataList와 classTypeMap를 매칭하여 dataMapByInterface에 저장
            foreach (var data in dataList)
            {
                var dataType = data.GetType();
                foreach (var iface in classTypeMap[dataType])
                {
                    if(!iface.IsInterface) continue;
                    if(!dataMapByInterface.ContainsKey(iface))
                    {
                        dataMapByInterface[iface] = new List<ScriptableObject>();
                    }
                    dataMapByInterface[iface].Add(data);
                }
            }
        }
        
        private void InjectImplementations()
        {            
            if (!interfaceMap.TryGetValue(typeof(IInterfaceInjectable), out var targets)) return;
            
            foreach (var target in targets)
            {
                IInterfaceInjectable interfaceInjected = (IInterfaceInjectable)target;

                //이미 등록된 RequiredInterfaceType.Key를 채우는 것으로 Inject
                foreach (var key in interfaceInjected.injectedImplements.Keys)
                {
                    if(key.IsInterface)
                    {
                        if (!interfaceMap.TryGetValue(key, out var value)) continue;
                        interfaceInjected.injectedImplements[key].AddRange(value);
                    }
                    else if(key.IsAbstract)
                    {
                        if (!abstractMap.TryGetValue(key, out var value)) continue;
                        interfaceInjected.injectedImplements[key].AddRange(value);
                    }
                }
            }
        }
        private void InjectSO()
        {
            if (!interfaceMap.TryGetValue(typeof(IDataInjectable), out var targets)) return;
            
            foreach (var target in targets)
            {
                IDataInjectable dataInjected = (IDataInjectable)target;

                foreach (var key in dataInjected.RequiredData.Keys)
                {

                    //key = 원하는 SO 인터페이스 타입
                    if (!dataMapByInterface.TryGetValue(key, out var value)) continue;
                    dataInjected.RequiredData[key].AddRange(value);
                }
            }
        }
        private void InjectAbstractType()
        {
            if(!interfaceMap.TryGetValue(typeof(IAbstractTypeInjected), out var targets)) return;

            foreach (var target in targets)
            {
                IAbstractTypeInjected abstractTypeInjected = (IAbstractTypeInjected)target;
                foreach (var key in abstractTypeInjected.AbstractTypeMap.Keys)
                {
                    if(!abstractTypeMap.TryGetValue(key, out var value)) continue;
                    abstractTypeInjected.AbstractTypeMap[key].AddRange(value);
                }
            }
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            // 인스턴스 맵 초기화 후 재등록
            interfaceMap.Clear();
            abstractMap.Clear();
            dataList.Clear();
            dataMapByInterface.Clear();

            InitializeMaps();
        }
        void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}

