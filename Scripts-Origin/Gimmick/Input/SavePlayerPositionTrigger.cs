using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavePlayerPositionTrigger : EventData
{
    private SaveManager saveManager;

    [SerializeField] AreaName areaName;

    private void Start()
    {
        saveManager = SaveManager.instance;
    }

    public override void Execute()
    {
        //문제1:
        //currentIndex가 최근경로를 가리키고 있고, 슬롯 UI에서 가장 최근 경로를 불러올 때,
        //캐릭터가 트리거 밟아서 실행되는 함수(Execute())가 데이터를 읽어들이는 함수보다 먼저 실행되면서 없는 저장경로를 불러오는 에러 발생
        //-> NotMoveNextIndex 라는 bool 변수를 만들어 못넘어가도록 임시조치

        saveManager.CurrentAreaIndex = (int)areaName;
        saveManager.CurrentPosition = this.transform.position;
        saveManager.SaveSloatData();
        
        //TODO: 일단 시간차 저장으로 해결했으나 저장시스템 바꿔야함
        //시간차로 한다면 기기 성능에 따라 문제가 생길수도 있음
        //if(SceneSwitcher.instance.SkipRespawnSave)
        //{
        //    SceneSwitcher.instance.SkipRespawnSave = false;
        //    return;
        //}

        saveManager.MoveToNextIndex();
    }
}
