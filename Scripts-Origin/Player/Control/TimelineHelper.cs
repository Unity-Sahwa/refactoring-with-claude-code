using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

public class TimelineHelper : MonoBehaviour
{
    public static TimelineHelper instance;

    #region 스크립트
    [SerializeField] private MaskChange maskChange;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerSound playerSound;

    [SerializeField] private MenuUI menuUI;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private UIEffect cameraUIEffect;
    [SerializeField] private MouseSettingUI mouseSettingUI;
    #endregion

    //[SerializeField] private GameObject[] HUD;
    //TODO: menuUI에서 InputHUD, BattleHUD 가져와서 사용


    #region 시작
    [Space(20)]
    [Header("시작")]
    private bool isTutorialScene;

    public GameObject startNewGameCamera;
    //[SerializeField] private GameObject startNewGameTimeline;
    //private PlayableDirector startNewGamePlayableDirector;
    //[SerializeField] private GameObject startNewGameSkipButton;
    //[SerializeField] private GameObject mainmenuCamera;

    [SerializeField] private GameObject tutorialStartSceneCamera;
    [SerializeField] private GameObject tutorialStartSceneTimeline;
    private PlayableDirector tutorialStartlayableDirector;

    [SerializeField] private GameObject tutorialStartSceneSkipButton;
    [SerializeField] private GameObject[] tutorialStartSceneElement;

    [SerializeField] private GameObject playerGameStartPosition;
    [SerializeField] private GameObject playerTutorialStartPosition;
    #endregion

    [SerializeField] private PlayableDirector[] timelines;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        //해당 스크립트는 기획자의 타임라인 어시스트를 위해 구현됨.
        //씬분리에 의해 tutorial 씬과 그 외의 씬에 역할이 구분되게 되었음.

        //첫번째 씬 + 스타트메뉴 활성화 + 메인메뉴 플레잉 아닐 때
        if (SceneManager.GetActiveScene().buildIndex == 0 && menuUI.MainMenu.activeSelf && !SceneSwitcher.instance.IsNotPlayingMainMenu)
        {
            isTutorialScene = true;
            DisablePlayerControl(true);
            maskChange.HumanMask.transform.position = playerGameStartPosition.transform.position;
            maskChange.HumanMask.transform.rotation = playerGameStartPosition.transform.rotation;

            if (tutorialStartSceneSkipButton != null) tutorialStartSceneSkipButton.SetActive(false);
            if (tutorialStartSceneTimeline != null) tutorialStartlayableDirector = tutorialStartSceneTimeline.GetComponent<PlayableDirector>();
        }

        else
        {
            isTutorialScene = false;
            menuUI.MainMenu.SetActive(false);
            DisablePlayerControl(false);
            //if(mainmenuCamera != null) mainmenuCamera.SetActive(false);

            //메인메뉴 건너뛰기 위한 처리
            SceneSwitcher.instance.IsNotPlayingMainMenu = false;
        }

        if (startNewGameCamera != null)
        {
            startNewGameCamera.SetActive(isTutorialScene);
        }
        else
        {
            Debug.LogWarning("startNewGameCamera == null");
        }
    }

    #region 튜토리얼 씬
    //public void SkipGameStartScene()
    //{
    //    StartCoroutine(CoSkipGameStartScene());
    //}
    //public void StartNewGameTimeline()
    //{
    //    startNewGamePlayableDirector.Play();
    //}
    //public IEnumerator CoSkipGameStartScene()
    //{
    //    FadeOutScreen(2);

    //    yield return new WaitForSeconds(.5f);

    //    startNewGameCamera.SetActive(false);
    //    startNewGameTimeline.SetActive(false);
    //    startNewGameSkipButton.SetActive(false);

    //    //정면을 바라보도록
    //    maskChange.HumanMask.transform.position = playerTutorialStartPosition.transform.position;
    //    maskChange.HumanMask.transform.rotation = Quaternion.Euler(0, 90, 0);
    //}

    public void StartTutorialTimeline()
    {
        StartCoroutine(CoStartTutorialTimeline());
    }

    public IEnumerator CoStartTutorialTimeline()
    {
        FadeOutScreen(2);
        maskChange.HumanMask.transform.position = playerTutorialStartPosition.transform.position;
        maskChange.HumanMask.transform.rotation = Quaternion.Euler(0, 90, 0);

        yield return new WaitForSeconds(1f);
        
        menuUI.MainMenu.SetActive(false);
        menuUI.NewGameWindow.SetActive(false);
        menuUI.StoryImageWindow.SetActive(false);
        MenuUI.instance.ActivateLetterBox(false);
        yield return new WaitForSeconds(.5f);

        FadeInScreen(.5f);
        tutorialStartlayableDirector.Play();
    }

    public IEnumerator CoSkipTutorialStartScene()
    {
        FadeOutScreen(2);

        yield return new WaitForSeconds(.5f);

        for (int i = 0; i < tutorialStartSceneElement.Length; i++)
        {
            if (tutorialStartSceneElement[i].activeSelf)
            {
                tutorialStartSceneElement[i].SetActive(false);
            }
        }

        //커서 셋팅
        if (PlatformSwitcher.instance.IsPCPlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }

        tutorialStartSceneCamera.SetActive(false);
        tutorialStartSceneTimeline.SetActive(false);
        tutorialStartSceneSkipButton.SetActive(false);

        //정면을 바라보도록
        yield return new WaitForSeconds(1f);

        FadeInScreen(2);
        DisablePlayerControl(false);
    }

    public void SkipTutorialStartScene()
    {
        StartCoroutine(CoSkipTutorialStartScene());
    }
    #endregion

    public bool IsTimelinePlaying()
    {
        bool isTimelinePlaying = false;

        for (int i = 0; i < timelines.Length; i++)
        {
            if (timelines[i] == null)
            {
                Debug.LogWarning($"timelines[{i}] == null");
                continue;
            }

            if (timelines[i].state == PlayState.Playing)
            {
                isTimelinePlaying = true;
            }
        }

        return isTimelinePlaying;
    }

    public void DisablePlayerControl(bool disableControl)
    {
        menuUI.DisablePlayerControl(disableControl);

        //타임라인 동안에도 퍼즈메뉴사용가능
        menuUI.CanShowPauseMenu(true);

        //menuUI.InputHUD.SetActive(!disableControl);
        //menuUI.BattleHUD.SetActive(!disableControl);

        if (disableControl)
        {//움직일 수 없음
            playerSound.StopLoopingAudio();
            playerState.ChangePlayerState(PlayerStateType.NONE);
            playerState.ChangePlayerSubState(PlayerSubStateType.NONE);

            mouseSettingUI.SetXAxisValue(0);
            mouseSettingUI.SetYAxisValue(0);

            if (!PlatformSwitcher.instance.IsPCPlatform)
            {
                menuUI.InputHUD.SetActive(false);
            }

            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {//움직일 수 있음
            mouseSettingUI.LoadMouseData();
            maskChange.HumanRigidbody.isKinematic = false;

            //타임라인 진행 끝에 인게임 진입할 때
            if (IsTimelinePlaying())
            {
                if (PlatformSwitcher.instance.IsPCPlatform)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;
                }
            }

            if (!PlatformSwitcher.instance.IsPCPlatform)
            {
                menuUI.InputHUD.SetActive(true);
            }
        }
    }

    //public void ActivateHUD(bool activate)
    //{
    //    menuUI.InputHUD.SetActive(activate);
    //    menuUI.BattleHUD.SetActive(activate);
    //}

    public void FadeInScreen(float timeRate)
    {
        //원래 화면으로 전환
        cameraUIEffect.FadeInScreen(timeRate);
    }

    public void FadeOutScreen(float timeRate)
    {
        //검게 전환
        cameraUIEffect.FadeOutScreen(timeRate);
    }
}