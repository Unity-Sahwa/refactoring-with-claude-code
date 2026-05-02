using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WisuAttackPatternB1 : MonoBehaviour
{
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
        var pattern = wisu.pattern_B1;

        yield return ExecuteAreaPattern(pattern.B1_area1, wisu.dangerZone_LargePillar, pattern.B1_area1_Interval, pattern.B1_area1_prefab);
        yield return ExecuteAreaPattern(pattern.B1_area2, wisu.dangerZone_LargePillar, pattern.B1_area2_Interval, pattern.B1_area2_prefab);
        yield return ExecuteAreaPattern(pattern.B1_area3, wisu.dangerZone_LargePillar, pattern.B1_area3_Interval, pattern.B1_area3_prefab);
        yield return ExecuteAreaPattern(pattern.B1_area4, wisu.dangerZone_LargePillar, pattern.B1_area4_Interval, pattern.B1_area4_prefab);
        yield return ExecuteAreaPattern(pattern.B1_area5, wisu.dangerZone_LargePillar, pattern.B1_area5_Interval, pattern.B1_area5_prefab);

        yield return new WaitForSeconds(pattern.B1_waitingTime);
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

//    private WisuMainRe wisu;
//    private GameObject firePillarPrefab;
//    private List<Transform> waaog2_Area1;
//    private List<Transform> waaog2_Area2;
//    private List<Transform> waaog2_Area3;
//    private List<Transform> waaog2_Area4;
//    private List<Transform> waaog2_Area5;
//    private float damage;
//    private float lifeTime;
//    private float interval;
//    private Animator animator;
//    private float fillarMakeTime2;

//    // wisu 변수를 참조하기 위한 초기화
//    public void Initialize(WisuMainRe wisuMain)
//    {
//        wisu = wisuMain;
//        animator = wisu.GetComponent<Animator>();
//        firePillarPrefab = wisu.FirePillarPrefab2;
//        waaog2_Area1 = wisu.WAAOG2_Area1;
//        waaog2_Area2 = wisu.WAAOG2_Area2;
//        waaog2_Area3 = wisu.WAAOG2_Area3;
//        waaog2_Area4 = wisu.WAAOG2_Area4;
//        waaog2_Area5 = wisu.WAAOG2_Area5;

//        damage = wisu.reinforcePillarDamage;
//        lifeTime = wisu.reinforcePillarLifetime;
//        interval = wisu.B_interval;
//        fillarMakeTime2 = wisu.fillarMakeTime2;
//}

//    // Fire Pillar 패턴 시작
//    public void StartPattern()
//    {
//        StartCoroutine(pattern.B1());
//    }
//    public void StopPattern()
//    {
//        StopAllCoroutines();
//    }

//    // Fire Pillar 패턴 진행
//    IEnumerator pattern.B1()
//    {    
//        yield return new WaitForSeconds(interval);

//        //area에서 불기둥 소환
//        SpawnPillarsInArea(waaog2_Area1);
//        yield return new WaitForSeconds(fillarMakeTime2);

//        SpawnPillarsInArea(waaog2_Area2);
//        yield return new WaitForSeconds(fillarMakeTime2);

//        SpawnPillarsInArea(waaog2_Area3);
//        yield return new WaitForSeconds(fillarMakeTime2);

//        SpawnPillarsInArea(waaog2_Area4);
//        yield return new WaitForSeconds(fillarMakeTime2);

//        SpawnPillarsInArea(waaog2_Area5);
//        yield return new WaitForSeconds(fillarMakeTime2);

//        wisu.isPatternFinished = true; // 패턴이 완료됨
//    }

//    // 주어진 영역에서 불기둥 소환
//    private void SpawnPillarsInArea(List<Transform> area)
//    {
//        foreach (Transform spawnPoint in area)
//        {
//            GameObject projectile = Instantiate(firePillarPrefab, spawnPoint.position, firePillarPrefab.transform.rotation);

//            // 기둥의 데미지와 lifeTime 설정
//            FirePillar firePillarScript = projectile.GetComponent<FirePillar>();
//            if (firePillarScript != null)
//            {
//                firePillarScript.damage = damage;
//                firePillarScript.lifeTime = lifeTime;
//            }
//        }
//    }
