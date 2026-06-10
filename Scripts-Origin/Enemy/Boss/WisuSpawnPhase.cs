using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WisuSpawnPhase : MonoBehaviour
{
    private WisuMainRe wisu;

    [HideInInspector] public bool isControllerActive = false;

    // 소환된 적들을 관리할 리스트
    private List<Enemy> spawnedEnemies = new List<Enemy>();

    private void Start()
    {
        wisu = GetComponent<WisuMainRe>();
    }


    public void StartPattern()
    {
        StartCoroutine(SpawnPattern());
    }

    IEnumerator SpawnPattern()
    {
        Debug.Log("SpawnPhase Start");
        wisu.suppressionController.isInvincible = true;
        // 1단계 적 소환
        for (int i = 0; i < wisu.phase1Enemies.Count; i++)
        {
            for (int count = 0; count < wisu.phase1Counts[i]; count++)
            {
                if (wisu.spawnPoints.Count > 0)
                {
                    int randomSpawnPointIndex = Random.Range(0, wisu.spawnPoints.Count);
                    Transform selectedSpawnPoint = wisu.spawnPoints[randomSpawnPointIndex];

                    // 몬스터 생성
                    GameObject spawnedEnemy = Instantiate(wisu.phase1Enemies[i], selectedSpawnPoint.position, Quaternion.identity);

                    // 적의 patrolPointA를 플레이어로 설정
                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.patrolPointA = wisu.target.transform;
                        spawnedEnemies.Add(enemy);
                    }

                    // 소환 대기 시간
                    Debug.Log("Spawn1");
                    yield return new WaitForSeconds(wisu.spawn1CoolTime[i]);
                }
            }
        }

        // 1단계와 2단계 사이 대기 시간
        yield return new WaitForSeconds(wisu.delayBetweenSpawnPhase);


        // 2단계 적 소환        
        for (int i = 0; i < wisu.phase2Enemies.Count; i++)
        {
            for (int count = 0; count < wisu.phase2Counts[i]; count++)
            {
                if (wisu.spawnPoints.Count > 0)
                {
                    int randomSpawnPointIndex = Random.Range(0, wisu.spawnPoints.Count);
                    Transform selectedSpawnPoint = wisu.spawnPoints[randomSpawnPointIndex];

                    // 몬스터 생성
                    GameObject spawnedEnemy = Instantiate(wisu.phase2Enemies[i], selectedSpawnPoint.position, Quaternion.identity);

                    // 적의 patrolPointA를 플레이어로 설정
                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.patrolPointA = wisu.target.transform;
                        spawnedEnemies.Add(enemy);
                    }

                    // 소환 대기 시간
                    Debug.Log("Spawn2");
                    yield return new WaitForSeconds(wisu.spawn2CoolTime[i]);
                }
            }
        }

        yield return new WaitForSeconds(wisu.delayBetweenSpawnPhase);

        Debug.Log("무적풀림");
        wisu.suppressionController.isInvincible = false;
        wisu.suppressionController.ResetController();

        while (!isControllerActive)
        {
            int randomSpawnPointIndex = Random.Range(0, wisu.spawnPoints.Count);
            Transform selectedSpawnPoint = wisu.spawnPoints[randomSpawnPointIndex];

            // 몬스터 생성
            GameObject spawnedEnemy = Instantiate(wisu.phase3Enemy, selectedSpawnPoint.position, Quaternion.identity);

            // 적의 patrolPointA를 플레이어로 설정
            Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.patrolPointA = wisu.target.transform;
                spawnedEnemies.Add(enemy);
            }

            // 소환 대기 시간
            Debug.Log("Spawn3");
            yield return new WaitForSeconds(wisu.spawn3CoolTime);
        }

        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy != null && !enemy.isDead)
            {
                enemy.DieAction();
            }
        }
        spawnedEnemies.Clear();
        wisu.suppressionController.ResetController();
        wisu.isSpawnPhaseFinished = true;
    }
}


//public class WisuSpawnPhase : MonoBehaviour
//{
//    private WisuMainRe wisu;

//    private List<GameObject> phase1Enemies = new List<GameObject>();
//    private List<int> phase1Counts = new List<int>();
//    private List<float> spawn1CoolTime = new List<float>();

//    private List<GameObject> phase2Enemies = new List<GameObject>();
//    private List<int> phase2Counts = new List<int>();
//    private List<float> spawn2CoolTime = new List<float>();

//    private GameObject phase3Enemy;
//    private float spawn3CoolTime;

//    private List<Transform> spawnPoints;
//    private float delayBetweenSpawnPhase;

//    private Animator animator;

//    private WisuSuppressionController controller;
//    [HideInInspector] public bool isControllerActive = false;

//    private float phase;

//    // 소환된 적들을 관리할 리스트
//    private List<Enemy> spawnedEnemies = new List<Enemy>();

//    public void Initialize(WisuMainRe wisuMain)
//    {
//        wisu = wisuMain;
//        animator = wisu.GetComponent<Animator>();
//        spawnPoints = wisu.spawnPoints;
//        delayBetweenSpawnPhase = wisu.delayBetweenSpawnPhase;
//        controller = wisu.suppressionController;

//        phase1Enemies = wisu.phase1Enemies;
//        phase1Counts = wisu.phase1Counts;
//        spawn1CoolTime = wisu.spawn1CoolTime;

//        phase2Enemies = wisu.phase2Enemies;
//        phase2Counts = wisu.phase2Counts;
//        spawn2CoolTime = wisu.spawn2CoolTime;

//        phase3Enemy = wisu.phase3Enemy;
//        spawn3CoolTime = wisu.spawn3CoolTime;

//        phase = wisu.phase;
//    }

//    public void StartPattern()
//    {
//        StartCoroutine(SpawnPattern());
//    }

//    IEnumerator SpawnPattern()
//    {
//        Debug.Log("SpawnPhase Start");
//        controller.isInvincible = true;
//        // 1단계 적 소환
//        for (int i = 0; i < phase1Enemies.Count; i++)
//        {
//            for (int count = 0; count < phase1Counts[i]; count++)
//            {
//                if (spawnPoints.Count > 0)
//                {
//                    int randomSpawnPointIndex = Random.Range(0, spawnPoints.Count);
//                    Transform selectedSpawnPoint = spawnPoints[randomSpawnPointIndex];

//                    // 몬스터 생성
//                    GameObject spawnedEnemy = Instantiate(phase1Enemies[i], selectedSpawnPoint.position, Quaternion.identity);

//                    // 적의 patrolPointA를 플레이어로 설정
//                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
//                    if (enemy != null)
//                    {
//                        enemy.patrolPointA = wisu.target.transform;
//                        spawnedEnemies.Add(enemy);
//                    }

//                    // 소환 대기 시간
//                    Debug.Log("Spawn1");
//                    yield return new WaitForSeconds(spawn1CoolTime[i]);
//                }
//            }
//        }

//        // 1단계와 2단계 사이 대기 시간
//        yield return new WaitForSeconds(delayBetweenSpawnPhase);


//        // 2단계 적 소환        
//        for (int i = 0; i < phase2Enemies.Count; i++)
//        {
//            for (int count = 0; count < phase2Counts[i]; count++)
//            {
//                if (spawnPoints.Count > 0)
//                {
//                    int randomSpawnPointIndex = Random.Range(0, spawnPoints.Count);
//                    Transform selectedSpawnPoint = spawnPoints[randomSpawnPointIndex];

//                    // 몬스터 생성
//                    GameObject spawnedEnemy = Instantiate(phase2Enemies[i], selectedSpawnPoint.position, Quaternion.identity);

//                    // 적의 patrolPointA를 플레이어로 설정
//                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
//                    if (enemy != null)
//                    {
//                        enemy.patrolPointA = wisu.target.transform;
//                        spawnedEnemies.Add(enemy);
//                    }

//                    // 소환 대기 시간
//                    Debug.Log("Spawn2");
//                    yield return new WaitForSeconds(spawn2CoolTime[i]);
//                }
//            }
//        }

//        yield return new WaitForSeconds(delayBetweenSpawnPhase);

//        Debug.Log("무적풀림");
//        controller.isInvincible = false;
//        controller.ResetController();

//        while (!isControllerActive)
//        {
//            int randomSpawnPointIndex = Random.Range(0, spawnPoints.Count);
//            Transform selectedSpawnPoint = spawnPoints[randomSpawnPointIndex];

//            // 몬스터 생성
//            GameObject spawnedEnemy = Instantiate(phase3Enemy, selectedSpawnPoint.position, Quaternion.identity);

//            // 적의 patrolPointA를 플레이어로 설정
//            Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
//            if (enemy != null)
//            {
//                enemy.patrolPointA = wisu.target.transform;
//                spawnedEnemies.Add(enemy);
//            }

//            // 소환 대기 시간
//            Debug.Log("Spawn3");
//            yield return new WaitForSeconds(spawn3CoolTime);
//        }

//        foreach (Enemy enemy in spawnedEnemies)
//        {
//            if (enemy != null && !enemy.isDead)
//            {
//                enemy.DieAction();
//            }
//        }
//        spawnedEnemies.Clear();
//        controller.ResetController();
//        wisu.isSpawnPhaseFinished = true;
//    }
//}
