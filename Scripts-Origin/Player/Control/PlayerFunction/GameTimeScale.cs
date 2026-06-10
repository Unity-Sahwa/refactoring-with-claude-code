using System;
using System.Collections;
using UnityEngine;

public class GameTimeScale : MonoBehaviour
{
    private CameraController cameraController;
    [SerializeField] private CheatMode cheatMode;

    [SerializeField] private PlayerHumanMaskData humanData;

    private bool stopTimeScaleCoroutine = false;
    [SerializeField] private float savedTimeScaleValue;
        

    private void Start()
    {
        savedTimeScaleValue = 1;
        cameraController = CameraController.instance;
    }

    public void Initialize()
    {
        if (cheatMode.isGameSlowed)
        {
            return;
        }

        Time.timeScale = 1;
    }


    public IEnumerator CoSetTimeScale(TimeScaleStruct timeScaleStruct)
    {
        if (!timeScaleStruct.useFunction)
        {
            yield break;
        }

        if (cheatMode.isGameSlowed)
        {
            yield break;
        }

        float coroutineStarTime = Time.time;
        float waitTime;
        float duration;
        savedTimeScaleValue = Time.timeScale;

        //측정 단위를 프레임 / 초 구분
        if (timeScaleStruct.useFrame)
        {
            waitTime = timeScaleStruct.waitTimeFrames * Time.deltaTime;
            duration = timeScaleStruct.durationFrames * Time.deltaTime;
        }
        else
        {
            waitTime = timeScaleStruct.waitTimeSeconds;
            duration = timeScaleStruct.durationSeconds;
        }

        yield return new WaitForSecondsRealtime(waitTime);

        Time.timeScale = timeScaleStruct.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1;
    }

    public void SetTimeScale(float timeScaleValue)
    {
        Time.timeScale = timeScaleValue;
    }
}
