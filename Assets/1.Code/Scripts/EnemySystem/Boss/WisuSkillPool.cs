using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public enum WisuSkill
    {
        A1,
        A2,
        A3,
        B1,
        B2,
        B3
    }

    // 역할: 보스 스킬 프리팹을 미리 만들어 두고 대여/반납으로 제공한다.
    public class WisuSkillPool : MonoBehaviour, IPreloadTargetProvider
    {
        [System.Serializable]
        public class Entry
        {
            public WisuSkill skill;
            public GameObject prefab;
            public int count;
            public float lifeTime = 10f;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private readonly Dictionary<WisuSkill, Entry> entryBySkill = new Dictionary<WisuSkill, Entry>();
        // 큐는 skill이 아니라 prefab 기준이라, 같은 프리팹을 쓰는 skill끼리 인스턴스를 돌려 쓴다.
        private readonly Dictionary<GameObject, Queue<GameObject>> idle = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> prefabOfInstance = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<GameObject, Coroutine> returnRoutines = new Dictionary<GameObject, Coroutine>();
        private readonly List<GameObject> preloadTargets = new List<GameObject>();

        // 프리로드가 미리 렌더할 대상. 만들어 둔 풀 인스턴스를 그대로 넘긴다.
        public IReadOnlyList<GameObject> PreloadTargets => preloadTargets;

        private void Awake()
        {
            foreach (var entry in entries)
            {
                if (entry.prefab == null)
                {
                    Debug.LogError($"WisuSkillPool: {entry.skill} 프리팹이 비었음");
                    continue;
                }

                entryBySkill[entry.skill] = entry;

                if (!idle.TryGetValue(entry.prefab, out var queue))
                {
                    queue = new Queue<GameObject>(entry.count);
                    idle[entry.prefab] = queue;
                }

                // 같은 프리팹을 쓰는 skill이 여럿이면 count는 합산된다.
                for (var i = 0; i < entry.count; i++)
                {
                    queue.Enqueue(CreateInstance(entry.prefab));
                }
            }
        }

        public GameObject Prefab(WisuSkill skill)
        {
            return entryBySkill[skill].prefab;
        }

        public GameObject Get(WisuSkill skill, Vector3 position)
        {
            return Get(skill, position, entryBySkill[skill].lifeTime);
        }

        public GameObject Get(WisuSkill skill, Vector3 position, float lifeTime)
        {
            var prefab = entryBySkill[skill].prefab;
            var queue = idle[prefab];
            // ponytail: 모자라면 그냥 더 만듦. count 튜닝은 인스펙터에서.
            var instance = queue.Count > 0 ? queue.Dequeue() : CreateInstance(prefab);

            instance.transform.SetPositionAndRotation(position, prefab.transform.rotation);
            instance.SetActive(true);
            prefabOfInstance[instance] = prefab;
            returnRoutines[instance] = StartCoroutine(ReturnAfter(instance, lifeTime));
            return instance;
        }

        public void Return(GameObject instance)
        {
            if (instance == null || !prefabOfInstance.TryGetValue(instance, out var prefab))
            {
                return;
            }

            if (returnRoutines.TryGetValue(instance, out var routine))
            {
                returnRoutines.Remove(instance);
                if (routine != null)
                {
                    StopCoroutine(routine);
                }
            }

            prefabOfInstance.Remove(instance);
            instance.SetActive(false);
            idle[prefab].Enqueue(instance);
        }

        private IEnumerator ReturnAfter(GameObject instance, float lifeTime)
        {
            yield return new WaitForSeconds(lifeTime);
            returnRoutines.Remove(instance);
            Return(instance);
        }

        private GameObject CreateInstance(GameObject prefab)
        {
            var instance = Instantiate(prefab);
            instance.SetActive(false);
            preloadTargets.Add(instance);
            return instance;
        }
    }
}
