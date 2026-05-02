using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;


public class PlayerSound : MonoBehaviour
{
    #region 외부
    [SerializeField] private PlayerState playerState;
    private PlayerHumanMaskData humanData;
    #endregion


    [SerializeField] private AudioMixer playerAudioMixer;
    [SerializeField] private AudioMixerGroup SFXGroup;
    [SerializeField] private GameObject[] audioObject;
    private AudioSource[] audioSources;

    private bool[] isPaused;

    //이전 소리 기록
    private int emptyIndex; //비어있는 인덱스
    private int currentIndex; //현재 사용하는 인덱스
    private PlayerStateType lastPlayerType; //더 세세하게 구분하려면 소리타입으로 바꿔서 적용
    private bool lastLoop;

    private bool stopSoundCoroutine = false;

    private void Start()
    {
        StartSet();
    }

    //소리 초기화 함수
    public void Initialize()
    {
        stopSoundCoroutine = true;
        StopLoopingAudio();
    }
    public void StartSet()
    {
        humanData = PlayerHumanMaskData.Instance;

        SFXGroup = playerAudioMixer.FindMatchingGroups("SFX")[0];

        stopSoundCoroutine = false;

        audioObject = new GameObject[10];
        audioSources = new AudioSource[10];
        isPaused = new bool[10];

        #region soundObject 오브젝트 생성
        for (int i = 0; i < 10; i++)
        {
            //스크립트 오브젝트 자식에 위치
            audioObject[i] = new GameObject("SoundObject");
            audioObject[i].transform.parent = transform;

            //오디오소스 컴포넌트 할당
            audioSources[i] = audioObject[i].AddComponent<AudioSource>();
            audioSources[i].playOnAwake = false;
            isPaused[i] = false;
        }
        #endregion

        currentIndex = 0;
        emptyIndex = 0;
        lastLoop = false;
        lastPlayerType = PlayerStateType.IDLE;
    }

    //vector3 타겟도 생각
    public void SetPlayerSound(SoundStruct soundStruct, Vector3 soundTarget, float skillStartTime)
    {
        #region 반환하는 경우
        if (!soundStruct.useFunction)
        {
            return;
        }

        //loop 소리 중복차단
        if (lastLoop)
        {
            if (lastPlayerType == playerState.playerCurrentState)
            {
                return;
            }
        }
        #endregion

        #region 작동안하는 오디오 찾아서 선택
        for (int i = 0; i < audioObject.Length; i++)
        {
            //플레이 중인 소스 패스
            if (audioSources[(i + emptyIndex) % audioObject.Length].isPlaying)
            {
                continue;
            }

            //해당 오디오 플레이 기록
            currentIndex = emptyIndex;
            emptyIndex = (emptyIndex+1) % audioObject.Length;

            lastLoop = soundStruct.loop;
            lastPlayerType = playerState.playerCurrentState;
            break;
        }
        #endregion

        #region 클립 설정
        audioObject[currentIndex].transform.position = soundTarget;
        audioSources[currentIndex].clip = soundStruct.audioClip[UnityEngine.Random.Range(0, soundStruct.audioClip.Length)];
        audioSources[currentIndex].loop = soundStruct.loop;
        audioSources[currentIndex].pitch = soundStruct.pitch;
        audioSources[currentIndex].volume = soundStruct.volume;
        audioSources[currentIndex].spatialBlend = soundStruct.spatialBlend;
        audioSources[currentIndex].minDistance = soundStruct.MinDistance;
        audioSources[currentIndex].maxDistance = soundStruct.MaxDistance;
        #endregion

        //
        audioSources[currentIndex].outputAudioMixerGroup = SFXGroup;

        stopSoundCoroutine = false;

        StartCoroutine(TogglePlayerSound(soundStruct, soundTarget, skillStartTime));
    }
    public IEnumerator TogglePlayerSound(SoundStruct soundStruct , Vector3 soundTarget, float skillStartTime)
    {
        bool activeSoundOnce = false;
        bool isPlaying = false;

        //혹시 lastIndex 가 변하는 경우 대비
        AudioSource targetAudio = audioSources[currentIndex];

        while (true)
        {
            //상태가 변하는대로 멈추기
            if (!soundStruct.untilFinish && stopSoundCoroutine)
            {
                targetAudio.Stop();
                yield break;
            }
            
            //플레이가 끝났다면 코루틴 중지
            if (isPlaying && !targetAudio.isPlaying)
            {
                yield break;
            }

            if (!activeSoundOnce && (Time.time >= skillStartTime + soundStruct.waitTime))
            {
                targetAudio.Play();

                isPlaying = true;
                activeSoundOnce = true;
            }

            yield return null;
        }

    }

    public void StopLoopingAudio()
    {
        if (audioSources == null || audioSources.Length <= 0)
        {
            return;
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i].loop)
            {
                audioSources[i].Stop();
            }
        }

        lastLoop = false;
        lastPlayerType = PlayerStateType.IDLE;
    }

    public void TogglePlayingAudioPause(bool turnSwitch)
    {
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (turnSwitch)
            {
                //플레이중인 오디오 일시정지, 배열 기록남기기
                if (audioSources[i].isPlaying)
                {
                    audioSources[i].Pause();
                    isPaused[i] = turnSwitch;
                }
            }
            else
            {
                //일시정지된 배열만 다시 정지해제
                if (isPaused[i])
                {
                    audioSources[i].UnPause();
                    isPaused[i] = turnSwitch;
                }
            }
        }
    }
}