using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preloader: MonoBehaviour
{
    //렉걸리는 요소들 미리 로딩
    [SerializeField] private AudioSource[] audios;
    [SerializeField] private GameObject[] playerEffects;
    [SerializeField] private GameObject[] environmentObjects;
    [SerializeField] private GameObject[] Enemies;

    //TODO: 환경이랑 카메라를 연계해서 로딩해야하지 않을까
    private void Start()
    {
        StartCoroutine(AudioPreload());
        StartCoroutine(PlayerPreload());
        StartCoroutine(EnvironmentPreload());
        StartCoroutine(EnemyPreload());
    }

    private IEnumerator AudioPreload()
    {
        if (audios.Length <= 0)
        {
            yield break;
        }

        yield return null;

        bool[] isInitiallyActive;
        float[] initialVolumes;
        isInitiallyActive = new bool[audios.Length];
        initialVolumes = new float[audios.Length];

        for (int i = 0; i < audios.Length; i++)
        {
            isInitiallyActive[i] = audios[i].gameObject.activeSelf;
            initialVolumes[i] = audios[i].volume;
            audios[i].gameObject.SetActive(true);
            audios[i].volume = 0;
            audios[i].Play();
        }

        
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < audios.Length; i++)
        {
            audios[i].gameObject.SetActive(isInitiallyActive[i]);
            audios[i].volume = initialVolumes[i];
        }

        
    }
    private IEnumerator PlayerPreload()
    {
        if (playerEffects.Length <= 0)
        {
            yield break;
        }

        yield return null;

        bool[] isInitiallyActive;
        isInitiallyActive = new bool[playerEffects.Length];
        for (int i = 0; i < isInitiallyActive.Length; i++)
        {
            isInitiallyActive[i] = playerEffects[i].activeSelf;
            playerEffects[i].SetActive(true);
        }

        yield return new WaitForSeconds(1f);


        for (int i = 0; i < playerEffects.Length; i++)
        {
            playerEffects[i].SetActive(isInitiallyActive[i]);
        }

    }

    private IEnumerator EnvironmentPreload()
    {
        //맵전체를 카메라로 비춤 
        if (environmentObjects.Length <= 0)
        {
            yield break;
        }

        yield return null;
        
        bool[] isInitiallyActive;
        isInitiallyActive = new bool[environmentObjects.Length];
        for (int i = 0; i < isInitiallyActive.Length; i++)
        {
            isInitiallyActive[i] = environmentObjects[i].activeSelf;
            environmentObjects[i].SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < environmentObjects.Length; i++)
        {
            environmentObjects[i].SetActive(isInitiallyActive[i]);
        }


    }

    private IEnumerator EnemyPreload()
    {
        if (Enemies.Length <= 0)
        {
            yield break;
        }

        yield return null;

        bool[] isInitiallyActive;
        isInitiallyActive = new bool[Enemies.Length];
        for (int i = 0; i < isInitiallyActive.Length; i++)
        {
            isInitiallyActive[i] = Enemies[i].activeSelf;
            Enemies[i].SetActive(true);
        }

        yield return new WaitForSeconds(1f);

        for (int i = 0; i < Enemies.Length; i++)
        {
            Enemies[i].SetActive(isInitiallyActive[i]);
        }
    }
}
