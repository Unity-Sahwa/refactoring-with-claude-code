using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SoundSettingUI : MonoBehaviour
{
    private SaveManager saveManager;

    [SerializeField] private AudioMixer enemyAudioMixer;
    [SerializeField] private AudioMixer playerAudioMixer;
    [SerializeField] private AudioMixer envAudioMixer;

    [SerializeField] private Slider masterAudioSlider;
    [SerializeField] private Slider BGMAudioSlider;
    [SerializeField] private Slider enemySFXSlider;
    [SerializeField] private Slider playerSFXSlider;

    //private float currentMasterVolume;
    //public float CurrentMasterVolume 
    //{
    //    get
    //    {
    //        return currentMasterVolume;
    //    }
        
    //    private set 
    //    {
    //        if (value <= 0)
    //        {
    //            currentMasterVolume = 0;
    //        }
    //        else if (value > 1)
    //        {
    //            currentMasterVolume = 1;
    //        }
    //        else
    //        {
    //            currentMasterVolume = value;
    //        }
    //    }
    //}
    //private float currentBGMVolume;
    //public float CurrentBGMVolume
    //{
    //    get
    //    {
    //        return currentBGMVolume;
    //    }

    //    private set
    //    {
    //        if (value <= 0)
    //        {
    //            currentBGMVolume = 0;
    //        }
    //        else if (value > 1)
    //        {
    //            currentBGMVolume = 1;
    //        }
    //        else
    //        {
    //            currentBGMVolume = value;
    //        }
    //    }
    //}
    //private float currentSFXVolume;
    //public float CurrentSFXVolume
    //{
    //    get
    //    {
    //        return currentSFXVolume;
    //    }

    //    private set
    //    {
    //        if (value <= 0)
    //        {
    //            currentSFXVolume = 0;
    //        }
    //        else if (value > 1)
    //        {
    //            currentSFXVolume = 1;
    //        }
    //        else
    //        {
    //            currentSFXVolume = value;
    //        }
    //    }
    //}

    private void Awake()
    {
        //float 매개변수 1개 가져야함.
        masterAudioSlider.onValueChanged.AddListener(SetMasterVolume);
        BGMAudioSlider.onValueChanged.AddListener(SetBGM);
        enemySFXSlider.onValueChanged.AddListener(SetEnemySFX);
        playerSFXSlider.onValueChanged.AddListener(SetPlayerSFX);
    }

    private void Start()
    {
        saveManager = SaveManager.instance;
        LoadVolumeData();
    }

    #region SetVolume
    public void SetMasterVolume(float value)
    {//슬라이더 값을 바로 받아서 사용
        //float logValue = Mathf.Log10(value) * 20;

        //if (value <= 0.005)
        //{
        //    logValue = -80;
        //}

        enemyAudioMixer.SetFloat("Master", value);
        playerAudioMixer.SetFloat("Master", value);
        envAudioMixer.SetFloat("Master", value);
    }
    public void SetBGM(float value)
    {//슬라이더 값을 바로 받아서 사용

        envAudioMixer.SetFloat("BGM", value);
        envAudioMixer.SetFloat("SFX", value);
    }
    public void SetEnemySFX(float value)
    {//슬라이더 값을 바로 받아서 사용

        enemyAudioMixer.SetFloat("SFX", value);
    }

    public void SetPlayerSFX(float value)
    {//슬라이더 값을 바로 받아서 사용

        playerAudioMixer.SetFloat("SFX", value);
    }

    #endregion

    public void MuteAudio()
    {
        SetMasterVolume(0);
    }

    public void SaveVolumeData()
    {//셋팅 창에서 조정한 볼륨값을 저장할 경우

        saveManager.ChangeVolumeSetting(masterAudioSlider.value, BGMAudioSlider.value, enemySFXSlider.value, playerSFXSlider.value);
        saveManager.SaveSoundData();
    }

    public void LoadVolumeData()
    {//saveManager 스크립트의 볼륨수치 불러오기(파일의 값을 불러오진 않음)

        //저장된 값을 재변환 후 0~1값을 만들어 준 후 
        SetMasterVolume(saveManager.masterVolume);
        SetBGM(saveManager.BGMVolume);

        SetEnemySFX(saveManager.enemySFXVolume);
        SetPlayerSFX(saveManager.playerSFXVolume);

        //재변환 후 UI 슬라이더 값 설정.
        masterAudioSlider.value = saveManager.masterVolume;
        BGMAudioSlider.value = saveManager.BGMVolume;
        enemySFXSlider.value = saveManager.enemySFXVolume;
        playerSFXSlider.value = saveManager.playerSFXVolume;
    }
}
