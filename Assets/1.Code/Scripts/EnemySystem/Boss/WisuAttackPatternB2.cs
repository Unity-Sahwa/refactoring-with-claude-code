using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
public class WisuAttackPatternB2 : MonoBehaviour
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
        var pattern = wisu.pattern_B2;

        yield return ExecuteAreaPattern(pattern.B2_area1, wisu.dangerZone_LargePillar, pattern.B2_area1_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area2, wisu.dangerZone_LargePillar, pattern.B2_area2_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area3, wisu.dangerZone_LargePillar, pattern.B2_area3_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area4, wisu.dangerZone_LargePillar, pattern.B2_area4_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area5, wisu.dangerZone_LargePillar, pattern.B2_area5_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area6, wisu.dangerZone_LargePillar, pattern.B2_area6_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area7, wisu.dangerZone_LargePillar, pattern.B2_area7_Interval, WisuSkill.B2);
        yield return ExecuteAreaPattern(pattern.B2_area8, wisu.dangerZone_LargePillar, pattern.B2_area8_Interval, WisuSkill.B2);

        yield return new WaitForSeconds(pattern.B2_waitingTime);
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
            GameObject pillarInst = wisu.skillPool.Get(skill, position.position);
            pillarInst.GetComponent<FirePillar>().Init(currentCharacterProvider);
        }
    }
}
}