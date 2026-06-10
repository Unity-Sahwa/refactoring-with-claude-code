using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WisuAttackPatternA2 : MonoBehaviour
{
    private WisuMainRe wisu;

    private void Start()
    {
        wisu = GetComponent<WisuMainRe>();
    }
    public void StartPattern()
    {
        StartCoroutine(A2());
    }
    public void StopPattern()
    {
        StopAllCoroutines();
    }

    private IEnumerator A2()
    {
        var pattern = wisu.pattern_A2;

        List<GameObject> randomizedPrefabs = new List<GameObject>(pattern.A2_prefabs);
        List<Transform> randomizedPoints = new List<Transform>(pattern.A2_points);

        System.Random random = new System.Random();

        for (int i = randomizedPoints.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (randomizedPoints[i], randomizedPoints[j]) = (randomizedPoints[j], randomizedPoints[i]);
        }

        for (int i = randomizedPrefabs.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (randomizedPrefabs[i], randomizedPrefabs[j]) = (randomizedPrefabs[j], randomizedPrefabs[i]);
        }

        var count = Mathf.Min(randomizedPoints.Count, randomizedPrefabs.Count);

        for (int i = 0; i < count; i++)
        {
            if (pattern.A2_prefabs[i] == null || randomizedPoints[i] == null)
            {
                Debug.LogWarning($"프리팹과 위치가 없음 {i}");
                continue;
            }

            GameObject projectile = Instantiate(pattern.A2_prefabs[i], randomizedPoints[i].position, pattern.A2_prefabs[i].transform.rotation);
            FirePillar firePillarScript = projectile.GetComponent<FirePillar>();

            float interval = (i < pattern.A2_intervals.Count) ? pattern.A2_intervals[i] : pattern.A2_defaultInterval;
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(pattern.A2_waitingTime);
        wisu.isPatternFinished = true;
    }
}
