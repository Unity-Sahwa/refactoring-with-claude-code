using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WisuAttackPatternA1 : MonoBehaviour
{
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

        // 각각 랜덤 순서로 섞기
        List<GameObject> randomFireBoltPrefabs = new List<GameObject>(pattern.A1_prefabs);
        List<Transform> randomFirePoints = new List<Transform>(pattern.A1_points); 

        System.Random random = new System.Random();

        // 리스트 섞기
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

        // 작은 리스트 크기만큼 반복
        var iterationCount = Mathf.Min(randomFirePoints.Count, randomFireBoltPrefabs.Count);

        for (int i = 0; i < iterationCount; i++)
        {
            GameObject projectile = Instantiate(randomFireBoltPrefabs[i], randomFirePoints[i].position, randomFireBoltPrefabs[i].transform.rotation);

            FireBolt fireBoltScript = projectile.GetComponent<FireBolt>();
            if (fireBoltScript == null)
            {
                Debug.LogError("FireBolt 스크립트가 발사체 프리팹에 연결 안됨.");
                continue;
            }

            //속도 계산해서 DangerZone과 동기화하기
            float speed = fireBoltScript.speed;
            Vector3 targetPosition = wisu.target.transform.position;
            Vector3 direction = (targetPosition - projectile.transform.position).normalized;
            projectile.GetComponent<Rigidbody>().velocity = direction * speed;

            // DangerZone 생성
            float scaleFactor = randomFireBoltPrefabs[i].transform.localScale.x;
            GameObject dangerZoneInst = Instantiate(wisu.dangerZone_Bolt, targetPosition, Quaternion.identity);
            dangerZoneInst.transform.localScale *= scaleFactor; // scaleFactor로 전체 스케일 변경

            DangerZone dangerZoneScript = dangerZoneInst.GetComponent<DangerZone>();
            dangerZoneScript.scalingTime = Vector3.Distance(randomFirePoints[i].position, targetPosition) / speed;

            fireBoltScript.destroyTime = dangerZoneScript.scalingTime;

            // 발사 간격 대기
            float interval = (i < pattern.A1_intervals.Count) ? pattern.A1_intervals[i] : pattern.A1_defaultInterval;
            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(pattern.A1_waitingTime);
        wisu.isPatternFinished = true;
    }

}


//private WisuMainRe wisu;
//private List<GameObject> fireBoltPrefabs;
//private List<Transform> firePoints;
//private Animator animator;
//private GameObject DangerZone;

//// wisu 변수를 참조하기 위한 초기화
//public void Initialize(WisuMainRe wisuMain)
//{
//    wisu = wisuMain;
//    animator = wisu.GetComponent<Animator>();
//    fireBoltPrefabs = wisu.FireBoltPrefabs;
//    firePoints = wisu.FirePoints;
//    DangerZone = wisu.DangerZone_Bolt;
//}

//public void StartPattern()
//{
//    StartCoroutine(A1());
//}

//public void StopPattern()
//{
//    StopAllCoroutines();
//}

//IEnumerator A1()
//{
//    // 프리팹과 발사 위치 설정
//    GameObject fireBoltPrefab = fireBoltPrefabs[Random.Range(0, fireBoltPrefabs.Count)]; // 랜덤 프리팹 선택
//    Transform firePoint = firePoints[Random.Range(0, firePoints.Count)]; // 랜덤 발사 위치 선택

//    // 발사체 생성 및 초기 방향 설정
//    GameObject projectile = Instantiate(fireBoltPrefab, firePoint.position, fireBoltPrefab.transform.rotation);
//    Vector3 targetPosition = wisu.target.transform.position;
//    Vector3 direction = (targetPosition - projectile.transform.position).normalized;

//    // 발사체의 속도 설정
//    FireBolt fireBoltScript = projectile.GetComponent<FireBolt>();

//    float projectileSpeed = fireBoltScript.speed; // FireBolt 스크립트에서 속도 가져오기
//    projectile.GetComponent<Rigidbody>().velocity = direction * projectileSpeed;

//    // 발사체의 거리 계산
//    float distance = Vector3.Distance(firePoint.position, targetPosition);

//    // scalingTime 계산 및 DangerZone 설정
//    float scalingTime = distance / projectileSpeed;
//    GameObject dangerZoneInstance = Instantiate(DangerZone, targetPosition, Quaternion.identity);
//    DangerZone dangerZoneScript = dangerZoneInstance.GetComponent<DangerZone>();
//    if (dangerZoneScript != null)
//    {
//        dangerZoneScript.scalingTime = scalingTime;
//    }

//    // lifeTime만큼 대기 후 패턴 완료
//    yield return new WaitForSeconds(fireBoltScript.lifeTime); // FireBolt 스크립트에서 lifeTime 가져오기

//    wisu.isPatternFinished = true; // 패턴 완료 플래그
//}