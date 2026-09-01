using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
public class WisuAttackPatternA3 : MonoBehaviour
{
    [Preserve, Inject] ICurrentCharacterProvider currentCharacterProvider;
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

        yield return ExecuteAreaPattern(pattern.A3_area1, wisu.dangerZone_SmallPillar, pattern.A3_area1_Interval, WisuSkill.A3);
        yield return ExecuteAreaPattern(pattern.A3_area2, wisu.dangerZone_SmallPillar, pattern.A3_area2_Interval, WisuSkill.A3);
        yield return ExecuteAreaPattern(pattern.A3_area3, wisu.dangerZone_SmallPillar, pattern.A3_area3_Interval, WisuSkill.A3);
        yield return ExecuteAreaPattern(pattern.A3_area4, wisu.dangerZone_SmallPillar, pattern.A3_area4_Interval, WisuSkill.A3);
        yield return ExecuteAreaPattern(pattern.A3_area5, wisu.dangerZone_SmallPillar, pattern.A3_area5_Interval, WisuSkill.A3);

        yield return new WaitForSeconds(pattern.A3_waitingTime);
        wisu.isPatternFinished = true;
    }

    private IEnumerator ExecuteAreaPattern(List<Transform> area, GameObject dangerZone, float interval, WisuSkill skill)
    {
        foreach (Transform position in area)
        {
            float scaleFactor = wisu.skillPool.Prefab(skill).transform.localScale.x;
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

            GameObject pillarInst = wisu.skillPool.Get(skill, TopPosition);
            pillarInst.GetComponent<FirePillar>().Init(currentCharacterProvider);
        }
    }   
}
}