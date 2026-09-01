using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
public class WisuSpawnPhase : MonoBehaviour
{
    private WisuMainRe wisu;

    // AttributeInjector는 씬 시작 시 1회만 돌아서 런타임 스폰 개체는 못 받는다. 스포너 몫으로 미리 받아서 넘겨준다.
    [Preserve, Inject(true)] private ICurrentCharacterProvider _currentCharacterProvider;
    [Preserve, Inject(true)] private ICharacterSwapNotifier _swapNotifier;

    [HideInInspector] public bool isControllerActive = false;

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
        wisu.suppressionController.isInvincible = true;
        for (int i = 0; i < wisu.phase1Enemies.Count; i++)
        {
            for (int count = 0; count < wisu.phase1Counts[i]; count++)
            {
                if (wisu.spawnPoints.Count > 0)
                {
                    int randomSpawnPointIndex = Random.Range(0, wisu.spawnPoints.Count);
                    Transform selectedSpawnPoint = wisu.spawnPoints[randomSpawnPointIndex];

                    GameObject spawnedEnemy = Instantiate(wisu.phase1Enemies[i], selectedSpawnPoint.position, Quaternion.identity);

                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.patrolPointA = wisu.target.transform;
                        enemy.InitCharacterRefs(_currentCharacterProvider, _swapNotifier);
                        spawnedEnemies.Add(enemy);
                    }

                    yield return new WaitForSeconds(wisu.spawn1CoolTime[i]);
                }
            }
        }

        yield return new WaitForSeconds(wisu.delayBetweenSpawnPhase);

        for (int i = 0; i < wisu.phase2Enemies.Count; i++)
        {
            for (int count = 0; count < wisu.phase2Counts[i]; count++)
            {
                if (wisu.spawnPoints.Count > 0)
                {
                    int randomSpawnPointIndex = Random.Range(0, wisu.spawnPoints.Count);
                    Transform selectedSpawnPoint = wisu.spawnPoints[randomSpawnPointIndex];

                    GameObject spawnedEnemy = Instantiate(wisu.phase2Enemies[i], selectedSpawnPoint.position, Quaternion.identity);

                    Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.patrolPointA = wisu.target.transform;
                        enemy.InitCharacterRefs(_currentCharacterProvider, _swapNotifier);
                        spawnedEnemies.Add(enemy);
                    }

                    yield return new WaitForSeconds(wisu.spawn2CoolTime[i]);
                }
            }
        }

        yield return new WaitForSeconds(wisu.delayBetweenSpawnPhase);

        wisu.suppressionController.isInvincible = false;
        wisu.suppressionController.ResetController();

        while (!isControllerActive)
        {
            int randomSpawnPointIndex = Random.Range(0, wisu.spawnPoints.Count);
            Transform selectedSpawnPoint = wisu.spawnPoints[randomSpawnPointIndex];

            GameObject spawnedEnemy = Instantiate(wisu.phase3Enemy, selectedSpawnPoint.position, Quaternion.identity);

            Enemy enemy = spawnedEnemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.patrolPointA = wisu.target.transform;
                enemy.InitCharacterRefs(_currentCharacterProvider, _swapNotifier);
                spawnedEnemies.Add(enemy);
            }

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
}