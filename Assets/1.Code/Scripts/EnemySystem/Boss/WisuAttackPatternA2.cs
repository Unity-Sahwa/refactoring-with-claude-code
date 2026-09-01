using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
public class WisuAttackPatternA2 : MonoBehaviour
{
    [Preserve, Inject] ICurrentCharacterProvider currentCharacterProvider;
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

        List<Transform> randomizedPoints = new List<Transform>(pattern.A2_points);

        System.Random random = new System.Random();

        for (int i = randomizedPoints.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (randomizedPoints[i], randomizedPoints[j]) = (randomizedPoints[j], randomizedPoints[i]);
        }

        for (int i = 0; i < randomizedPoints.Count; i++)
        {
            if (randomizedPoints[i] == null)
            {
                Debug.LogWarning($"발사 위치가 비었음 {i}");
                continue;
            }

            GameObject projectile = wisu.skillPool.Get(WisuSkill.A2, randomizedPoints[i].position);
            FirePillar firePillarScript = projectile.GetComponent<FirePillar>();
            firePillarScript.Init(currentCharacterProvider);
            
            float interval = (i < pattern.A2_intervals.Count) ? pattern.A2_intervals[i] : pattern.A2_defaultInterval;
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(pattern.A2_waitingTime);
        wisu.isPatternFinished = true;
    }
}
}