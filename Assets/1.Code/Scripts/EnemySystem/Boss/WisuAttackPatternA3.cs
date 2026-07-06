using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
public class WisuAttackPatternA3 : MonoBehaviour
{
    [Inject] ICurrentCharacterProvider currentCharacterProvider;
    private WisuMainRe wisu;

    private void Start()
    {
        wisu = GetComponent<WisuMainRe>();
    }

    public void StartPattern()
    {
        StartCoroutine(A3());
    }
    public void StopPattern()
    {
        StopAllCoroutines();
    }
    private IEnumerator A3()
    {
        var pattern = wisu.pattern_A3;

        yield return ExecuteAreaPattern(pattern.A3_area1, wisu.dangerZone_SmallPillar, pattern.A3_area1_Interval, pattern.A3_area1_prefab);
        yield return ExecuteAreaPattern(pattern.A3_area2, wisu.dangerZone_SmallPillar, pattern.A3_area2_Interval, pattern.A3_area2_prefab);
        yield return ExecuteAreaPattern(pattern.A3_area3, wisu.dangerZone_SmallPillar, pattern.A3_area3_Interval, pattern.A3_area3_prefab);
        yield return ExecuteAreaPattern(pattern.A3_area4, wisu.dangerZone_SmallPillar, pattern.A3_area4_Interval, pattern.A3_area4_prefab);
        yield return ExecuteAreaPattern(pattern.A3_area5, wisu.dangerZone_SmallPillar, pattern.A3_area5_Interval, pattern.A3_area5_prefab);

        yield return new WaitForSeconds(pattern.A3_waitingTime);
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
            Vector3 TopPosition = position.position;
            TopPosition.y = 17.0f;

            GameObject pillarInst = Instantiate(pillar, TopPosition, pillar.transform.rotation);
            pillarInst.GetComponent<FirePillar>().Init(currentCharacterProvider);
        }
    }   
}
}