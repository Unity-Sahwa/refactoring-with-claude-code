using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    private bool isChanging;

    //bool isKorean을 비파괴 <SceneSwitcher>에 넣어주기
    //TODO: 게임 끝날때까지 유지되어야하는 변수들은 한곳에 모아놓기

    //저장은 해당 버튼을 누를 때
    //불러오기는 Awake 시점에


    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        if (PlatformSwitcher.instance.IsKorean)
        {
            ChangeLanguage(0);
        }
        else
        {
            ChangeLanguage(1);
        }

    }

    public void ChangeLanguage(int index)
    {
        if (isChanging)
        {
            return;
        }

        StartCoroutine(CoChangeLanguage(index));
    }

    private IEnumerator CoChangeLanguage(int index)
    {
        isChanging = true;

        if (index == 0)
        {
            PlatformSwitcher.instance.IsKorean = true;
        }
        else if (index == 1)
        {
            PlatformSwitcher.instance.IsKorean = false;
        }

        //스피커없어서 소리 못들음. 집에서 다시 듣기
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

        isChanging = false;
    }
}
