using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WisuAttackPatternB3 : MonoBehaviour
{
    private WisuMainRe wisu;

    private void Start()
    {
        wisu = GetComponent<WisuMainRe>();
    }

    public void StartPattern()
    {
        StartCoroutine(B3());
    }
    public void StopPattern()
    {
        StopAllCoroutines();
    }

    private IEnumerator B3()
    {
        var pattern = wisu.pattern_B3;

        yield return ExecuteAreaPattern(pattern.B3_area1, wisu.dangerZone_LargePillar, pattern.B3_area1_Interval, pattern.B3_area1_prefab);
        yield return ExecuteAreaPattern(pattern.B3_area2, wisu.dangerZone_LargePillar, pattern.B3_area2_Interval, pattern.B3_area2_prefab);
        yield return ExecuteAreaPattern(pattern.B3_area3, wisu.dangerZone_LargePillar, pattern.B3_area3_Interval, pattern.B3_area3_prefab);
        yield return ExecuteAreaPattern(pattern.B3_area4, wisu.dangerZone_LargePillar, pattern.B3_area4_Interval, pattern.B3_area4_prefab);
        yield return ExecuteAreaPattern(pattern.B3_area5, wisu.dangerZone_LargePillar, pattern.B3_area5_Interval, pattern.B3_area5_prefab);
        yield return ExecuteAreaPattern(pattern.B3_area6, wisu.dangerZone_LargePillar, pattern.B3_area6_Interval, pattern.B3_area6_prefab);
        yield return ExecuteAreaPattern(pattern.B3_area7, wisu.dangerZone_LargePillar, pattern.B3_area7_Interval, pattern.B3_area7_prefab);

        yield return new WaitForSeconds(pattern.B3_waitingTime);
        wisu.isPatternFinished = true;
    }

    private IEnumerator ExecuteAreaPattern(List<Transform> area, GameObject dangerZone, float interval, GameObject pillar)
    {
        foreach (Transform position in area)
        {
            float scaleFactor = pillar.transform.localScale.x;
            GameObject dangerZoneInst = Instantiate(dangerZone, position.position, Quaternion.identity);
            dangerZoneInst.transform.localScale *= scaleFactor;

            DangerZone dangerZoneScript = dangerZoneInst.GetComponent<DangerZone>();
            dangerZoneScript.scalingTime = interval;
        }

        yield return new WaitForSeconds(interval);

        foreach (Transform position in area)
        {
            Instantiate(pillar, position.position, pillar.transform.rotation);
        }
    }
}
