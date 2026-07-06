using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
public class WisuAttackPatternA1 : MonoBehaviour
{
    [Inject] ICurrentCharacterProvider currentCharacterProvider;
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

        List<GameObject> randomFireBoltPrefabs = new List<GameObject>(pattern.A1_prefabs);
        List<Transform> randomFirePoints = new List<Transform>(pattern.A1_points); 

        System.Random random = new System.Random();

        for (int i = randomFirePoints.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (randomFirePoints[i], randomFirePoints[j]) = (randomFirePoints[j], randomFirePoints[i]);
        }

        for (int i = randomFireBoltPrefabs.Count - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (randomFireBoltPrefabs[i], randomFireBoltPrefabs[j]) = (randomFireBoltPrefabs[j], randomFireBoltPrefabs[i]);
        }

        var iterationCount = Mathf.Min(randomFirePoints.Count, randomFireBoltPrefabs.Count);

        for (int i = 0; i < iterationCount; i++)
        {
            GameObject projectile = Instantiate(randomFireBoltPrefabs[i], randomFirePoints[i].position, randomFireBoltPrefabs[i].transform.rotation);

            FireBolt fireBoltScript = projectile.GetComponent<FireBolt>();
            if (fireBoltScript == null)
            {
                Debug.LogError("FireBolt 스크립트가 발사체 프리팹에 붙지 않음");
                continue;
            }
            fireBoltScript.Init(currentCharacterProvider);

            float speed = fireBoltScript.speed;
            Vector3 targetPosition = wisu.target.transform.position;
            Vector3 direction = (targetPosition - projectile.transform.position).normalized;
            projectile.GetComponent<Rigidbody>().linearVelocity = direction * speed;

            float scaleFactor = randomFireBoltPrefabs[i].transform.localScale.x;
            GameObject dangerZoneInst = Instantiate(wisu.dangerZone_Bolt, targetPosition, Quaternion.identity);
            dangerZoneInst.transform.localScale *= scaleFactor; 

            DangerZone dangerZoneScript = dangerZoneInst.GetComponent<DangerZone>();
            dangerZoneScript.scalingTime = Vector3.Distance(randomFirePoints[i].position, targetPosition) / speed;

            fireBoltScript.destroyTime = dangerZoneScript.scalingTime;

            float interval = (i < pattern.A1_intervals.Count) ? pattern.A1_intervals[i] : pattern.A1_defaultInterval;
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(pattern.A1_waitingTime);
        wisu.isPatternFinished = true;
    }

}
}