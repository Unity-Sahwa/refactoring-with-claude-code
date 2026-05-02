using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public abstract class PlayerSkill : MonoBehaviour
{
    #region 외부
    protected PlayerController playerController;
    protected PlayerMovement playerMovement;
    protected MaskChange maskChange;
    protected Player player;

    protected CameraController cameraController;
    protected UIEffect UIEffect;

    //데이터
    protected PlayerCommonData commonData;
    protected PlayerHumanMaskData humanData;
    protected PlayerAnimalMaskData animalData;

    protected CameraData cameraData;
    #endregion

    private bool activeStartOnce = false;

    #region 셋팅
    protected void StartSet()
    {
        if (!activeStartOnce)
        {
            #region 외부
            playerController = PlayerController.instance;
            playerMovement = playerController.playerMovement;
            maskChange = playerController.maskChange;
            player = Player.instance;

            cameraController = CameraController.instance;
            UIEffect = UIEffect.instance;

            //데이터
            commonData = PlayerCommonData.Instance;
            humanData = PlayerHumanMaskData.Instance;
            animalData = PlayerAnimalMaskData.Instance;

            cameraData = CameraData.Instance;
            #endregion

            activeStartOnce = true;
        }
        else
        {
            return;
        }
    }

    #endregion

    #region 스킬
    //기능이 너무 조잡하다 필요한 기능에 맞게 다시 구성해야함
    

    #region 스킬 실행중 기능
    //오브젝트 활성화 제어
    //밖의 값을 바꾸려면 ref
    protected void ControlObject(ref GameObject controlObject, ref bool activeOnce, ref bool inActiveOnce, float gameTime = 0, float startTime = 0, float waitTime = 99, float duration = 99)
    {
        //ref bool activeOnce 외부에서 변환이 안됨. 
        if (!inActiveOnce && (gameTime >= startTime + waitTime + duration))
        {
            controlObject.SetActive(false);
            inActiveOnce = true;
        }

        if (!activeOnce && (gameTime >= startTime + waitTime))
        {
            controlObject.SetActive (true);
            activeOnce = true;
        }
    }

    //오브젝트 위치 제어
    protected void SetEffectPosition(ref GameObject effectObject, Transform effectPosition)
    {
        effectObject.transform.position = effectPosition.transform.position;
        effectObject.transform.rotation = effectPosition.transform.rotation;
    }
    #endregion

    #endregion
}

