// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Reflection;
// using System.Linq;

// namespace Refactoring
// {
//     //Project Settings > Script Execution Order > -100
//     public class DependencyInjector_Old : MonoBehaviour
//     {
//         private static DependencyInjector _instance;
//         private static Dictionary<Type, List<Type>> interfaceTypeMap = new(); 
//         private static Dictionary<Type, List<Type>> classTypeMap = new(); 
//         private static Dictionary<Type, List<Type>> abstractTypeMap = new(); 
        
//         private MonoBehaviour[] injectableObjects;
//         private Dictionary<Type, List<object>> interfaceMap = new(); 
//         private Dictionary<Type, List<object>> abstractMap = new(); 
//         private List<ScriptableObject> dataList = new();
//         private Dictionary<Type, List<ScriptableObject>> dataMap = new();
        
//         [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
//         public static void CollectType()
//         {
//             Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes()
//                                 .Where(t => t.Namespace == "Refactoring" && t.IsClass && !t.IsAbstract)
//                                 .ToArray();

//             foreach (var type in allTypes)
//             {
//                 var types = new List<Type>(type.GetInterfaces());
//                 if(type.BaseType != null && type.BaseType.IsAbstract)
//                 {
//                     types.Add(type.BaseType);
//                 }

//                 classTypeMap[type] = types;
//             }

//             foreach (var (classType, types) in classTypeMap)
//             {
//                 foreach (var type in types)
//                 {
//                     var map = type.IsInterface ? interfaceTypeMap : abstractTypeMap;

//                     if(!map.ContainsKey(type))
//                     {
//                         map[type] = new List<Type>();
//                     }
//                     map[type].Add(classType);
//                 }
//             }
//         }
//         void Awake()
//         {
//             if(_instance == null)
//             {
//                 _instance = this;
//             }
//             else
//             {
//                 if (_instance != this)
//                 {
//                     Destroy(gameObject);
//                 }
//             }

//             InitializeMaps();
//         }
//         private void InitializeMaps()
//         {
//             RegisterMonoBehaviours();
//             RegisterImplementations();
//             RegisterSO();

//             InjectImplementations();
//             InjectSO();
//             InjectAbstractType();
//         }
//         private void RegisterMonoBehaviours()
//         {
//             injectableObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
//         }
//         //씬에 있는 인스턴스의 타입과 classTypeMap을 매치시켜 interfaceMap, abstractMap에 인스턴스 채우기
//         private void RegisterImplementations()
//         {
//             foreach (var obj in injectableObjects)
//             {
//                 var objType = obj.GetType();

//                 if (!classTypeMap.ContainsKey(objType))
//                 {
//                     Debug.LogWarning($"DependencyInjector.RegisterInterfaceMap: classTypeMap에 {objType.Name}가 없습니다.");
//                     continue;
//                 }

//                 foreach (var type in classTypeMap[objType])
//                 {
//                     if(type.IsInterface)
//                     {
//                         if (!interfaceMap.ContainsKey(type))
//                         {
//                             interfaceMap[type] = new List<object>();
//                         }
//                         interfaceMap[type].Add(obj);
//                     }
//                     else if(type.IsAbstract)
//                     {
//                         if(!abstractMap.ContainsKey(type))
//                         {
//                             abstractMap[type] = new List<object>();
//                         }
//                         abstractMap[type].Add(obj);
//                     }
//                     else
//                     {
//                         Debug.LogWarning($"RegisterImplementations: 예외발생");
//                     }
//                 }
//             }
//         }
//         //SO 제공자(IDataProvider)를 통해 데이터 등록
//         private void RegisterSO()
//         {
//             //SO데이터 제공자(interfaceMap[IDataProvider]) 로부터 데이터 받아서 dataMap에 넣기
//             if (!interfaceMap.TryGetValue(typeof(IDataProvider), out var targets)) return;

//             foreach (var target in targets)
//             {
//                 IDataProvider provider = (IDataProvider)target;
//                 dataList.AddRange(provider.ProvideData());
//             }

//             //dataList와 classTypeMap를 매칭하여 dataMapByInterface에 저장
//             foreach (var data in dataList)
//             {
//                 if(data ==null)
//                 {
//                     Debug.LogWarning("SOContainer에서 Missing을 확인하세요");
//                     return;
//                 }
//                 var dataType = data.GetType();
//                 foreach (var element in classTypeMap[dataType])
//                 {
//                     if(!dataMap.ContainsKey(element))
//                     {
//                         dataMap[element] = new List<ScriptableObject>();
//                     }
//                     dataMap[element].Add(data);
//                 }
//             }
//         }
        
//         // 객체 꽂아주기(so데이터 에셋, interface , abstract 등)
//         // - 어떤 타입 원해요 ~ 하면 해당 타입의 객체 모두 주기. 여러 개 달라면 여러 개, 한 개 달라고 하면 한 개(여러개가 찾아지면 경고)
//         private void InjectImplementations()
//         {            
//             if (!interfaceMap.TryGetValue(typeof(IInterfaceInjectable), out var targets)) return;
            
//             foreach (var target in targets)
//             {
//                 IInterfaceInjectable interfaceInjected = (IInterfaceInjectable)target;

//                 //이미 등록된 RequiredInterfaceType.Key를 채우는 것으로 Inject
//                 foreach (var key in interfaceInjected.injectedImplements.Keys)
//                 {
//                     if(key.IsInterface)
//                     {
//                         if (!interfaceMap.TryGetValue(key, out var value)) continue;
//                         interfaceInjected.injectedImplements[key].AddRange(value);
//                     }
//                     else if(key.IsAbstract)
//                     {
//                         if (!abstractMap.TryGetValue(key, out var value)) continue;
//                         interfaceInjected.injectedImplements[key].AddRange(value);
//                     }
//                 }
//             }
//         }
//         private void InjectSO()
//         {
//             if (!interfaceMap.TryGetValue(typeof(IDataInjectable), out var targets)) return;
            
//             foreach (var target in targets)
//             {
//                 IDataInjectable dataInjected = (IDataInjectable)target;

//                 foreach (var key in dataInjected.RequiredData.Keys)
//                 {
//                     //key = 원하는 SO 인터페이스 타입
//                     if (!dataMap.TryGetValue(key, out var value)) continue;
//                     dataInjected.RequiredData[key].AddRange(value);
//                 }
//             }
//         }
        
//         // 타입 꽂아주기(객체가 아닌 타입. abstract라던가)
//         // - abstract이라고 한다면 abstract의 자식 모두를 반환(상태 매니저에서 상태 타입을 얻어와서 객체 생성할 때
//         private void InjectAbstractType()
//         {
//             if(!interfaceMap.TryGetValue(typeof(IAbstractTypeInjected), out var targets)) return;

//             foreach (var target in targets)
//             {
//                 IAbstractTypeInjected abstractTypeInjected = (IAbstractTypeInjected)target;
//                 foreach (var key in abstractTypeInjected.AbstractTypeMap.Keys)
//                 {
//                     if(!abstractTypeMap.TryGetValue(key, out var value)) continue;
//                     abstractTypeInjected.AbstractTypeMap[key].AddRange(value);
//                 }
//             }
//         }
//         void OnDestroy()
//         {
//             if(_instance == this)
//             {
//                 _instance = null;
//             }
//         }
//     }
// }

