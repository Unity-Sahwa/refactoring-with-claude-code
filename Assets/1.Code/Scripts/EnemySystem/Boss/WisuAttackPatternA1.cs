using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
public class WisuAttackPatternA1 : MonoBehaviour
{
    [Preserve, Inject] ICurrentCharacterProvider currentCharacterProvider;
    private WisuMainRe wisu;


    public void Start()
    {
        wisu = GetComponent<WisuMainRe>();
    }

    public void StartPattern()
    {
        StartCoroutine(A1());
    }

    public void StopPattern()
    {
        StopAllCoroutines();
    }

    IEnumerator A1()
    {
        var pattern = wisu.pattern_A1;

        List<Transform> randomFirePoints = new List<Transform>(pattern.A1_points);

        System.Random random = new System.Random();

        for (int i = randomFirePoints.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (randomFirePoints[i], randomFirePoints[j]) = (randomFirePoints[j], randomFirePoints[i]);
        }

        GameObject boltPrefab = wisu.skillPool.Prefab(WisuSkill.A1);
        float scaleFactor = boltPrefab.transform.localScale.x;
        float speed = boltPrefab.GetComponent<FireBolt>().speed;

        for (int i = 0; i < randomFirePoints.Count; i++)
        {
            Vector3 targetPosition = wisu.target.transform.position;
            // 발사체 수명 = 목표 지점까지 걸리는 시간. 이 시간이 곧 반납 시점이자 위험 표시 시간이다.
            float flightTime = Vector3.Distance(randomFirePoints[i].position, targetPosition) / speed;

            GameObject projectile = wisu.skillPool.Get(WisuSkill.A1, randomFirePoints[i].position, flightTime);
            projectile.GetComponent<FireBolt>().Init(currentCharacterProvider);

            Vector3 direction = (targetPosition - projectile.transform.position).normalized;
            projectile.GetComponent<Rigidbody>().linearVelocity = direction * speed;

            GameObject dangerZoneInst = Instantiate(wisu.dangerZone_Bolt, targetPosition, Quaternion.identity);
            dangerZoneInst.transform.localScale *= scaleFactor;
            dangerZoneInst.GetComponent<DangerZone>().scalingTime = flightTime;

            float interval = (i < pattern.A1_intervals.Count) ? pattern.A1_intervals[i] : pattern.A1_defaultInterval;
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(pattern.A1_waitingTime);
        wisu.isPatternFinished = true;
    }

}
}