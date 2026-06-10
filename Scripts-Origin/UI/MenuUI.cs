using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuUI : MonoBehaviour
{
    public static MenuUI instance;

    #region 외부
    [Header("외부")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private MaskChange maskChange;
    [SerializeField] private PlayerSound playerSound;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private SoundSettingUI soundUI;
    [SerializeField] private InputKeySettingUI inputKeyUI;
    [SerializeField] private MouseSettingUI mouseUI;
    [SerializeField] private GameTimeScale gameTimeScale;

    private TimelineHelper timelineHelper;
    private LoadingUI loadingUI;

    private PlayerCommonData commonData;
    #endregion

    #region 메뉴
    [Space(20)]
    [Header("메뉴")]
    [SerializeField] private GameObject mainMenu;
    public GameObject MainMenu
    {
        get
        {
            return mainMenu;
        }
    }

    [SerializeField] GameObject pauseMenu;
    public GameObject PauseMenu
    {
        get
        {
            return pauseMenu;
        }
    }
    #endregion

    #region Window
    [Space(20)]
    [Header("기능창")]
    [SerializeField] private GameObject newGameWindow;
    public GameObject NewGameWindow
    {
        get
        {
            return newGameWindow;
        }
    }

    [SerializeField] private Button newGameButton;
    [SerializeField] private Button[] StoryImage;
    [SerializeField] private GameObject storyImageWindow;
    public GameObject StoryImageWindow
    {
        get
        {
            return storyImageWindow;
        }
    }

    [SerializeField] private GameObject loadSlotWindow;
    [SerializeField] private GameObject loadWindow;
    [SerializeField] private Button loadButton;

    [SerializeField] private GameObject settingWindow;
    [SerializeField] private GameObject soundWindow;
    [SerializeField] private GameObject inputKeyWindow;
    public GameObject InputKeyWindow
    {
        get { return inputKeyWindow; }
        set { inputKeyWindow = value; }
    }

    [SerializeField] private GameObject goToMainMenuWindow;
    [SerializeField] private Button goToMainMenuButton;

    [SerializeField] private GameObject quitWindow;
    [SerializeField] private Button quitButton;

    [SerializeField] private Button openSettingButton;

    [SerializeField] private Button setPCButton;
    [SerializeField] private Button setMobileButton;

    //TODO: 배틀 HUD에서 PC HUD 도 작성하기
    #endregion

    public bool isPlayerControlDisabled { get; private set; }
    public bool canShowPauseMenu { get; private set; }

    #region Save Slot
    [Header("슬롯(순서대로 넣기)")]
    [SerializeField] private Button[] loadSlots;
    [SerializeField] private GameObject[] selectImage;
    [SerializeField] private TextMeshProUGUI[] areaText;
    [SerializeField] private TextMeshProUGUI[] saveTypeText;
    [SerializeField] private TextMeshProUGUI[] playTimeText;
    [SerializeField] private TextMeshProUGUI[] dateText;
    [SerializeField] private TextMeshProUGUI[] noDataText;
    //슬롯 순서 정하기 위함
    private int?[] timeIndex;
    #endregion

    #region HUD 
    [Space(20)]
    [Header("HUD")]
    [SerializeField] private GameObject inputHUD;
    public GameObject InputHUD
    {
        get { return inputHUD; }
        set { inputHUD = value; }
    }

    [SerializeField] private GameObject battleHUD;
    public GameObject BattleHUD
    {
        get { return battleHUD; }
        set { battleHUD = value; }
    }

    [SerializeField] private GameObject battleGuideHUD;
    public GameObject BattleGuideHUD
    {
        get { return battleGuideHUD; }
        set { battleGuideHUD = value; }
    }

    #endregion

    #region 레터박스
    [Space(20)]
    [Header("LetterBox")]
    [SerializeField] private Image letterBoxImage;
    [SerializeField] private Image letterBoxMaskImage;
    [SerializeField] private Mask letterBoxMask;
    #endregion

    #region 언어설정
    [Space(20)]
    [Header("언어 교체")]
    [SerializeField] private Button changeToKorean;
    [SerializeField] private Button changeToEnglish;
    #endregion

    #region 설정 버튼 입력
    [Space(20)]
    [Header("설정 버튼")]
    [SerializeField] private GameObject[] settingElements;
    [SerializeField] private Image[] settingTextBG;
    [SerializeField] private TextMeshProUGUI[] settingText;
    [SerializeField] private Button[] settingButton;

    #endregion


    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);

        timeIndex = new int?[loadSlots.Length];
        for (int i = 0; i < timeIndex.Length; i++)
        {
            timeIndex[i] = null;
        }
    }

    void Start()
    {
        timelineHelper = TimelineHelper.instance;
        loadingUI = LoadingUI.instance;

        //메뉴창 위치정렬  
        SetPosition();
        SetUIElements();
        SetButtonFunction();

        //PlayGuide가 DontDestroy라서
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayGuide.instance.IsTutorialStart = false;
        }


        if (mainMenu.activeSelf && !SceneSwitcher.instance.IsNotPlayingMainMenu)
        {
            //메인메뉴인 경우
            canShowPauseMenu = false;
            isPlayerControlDisabled = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            //바로 인게임 시작
            canShowPauseMenu = true;
            isPlayerControlDisabled = false;

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
    }
    #region Set
    private void SetPosition()
    {
        RectTransform starMenuRT = mainMenu.GetComponent<RectTransform>();
        //RectTransform cartoonRT = cartoon.GetComponent<RectTransform>();
        RectTransform pauseMenuRT = pauseMenu.GetComponent<RectTransform>();
        RectTransform settingWindowRT = settingWindow.GetComponent<RectTransform>();

        SetRectTransform(starMenuRT);
        SetRectTransform(settingWindowRT);
        SetRectTransform(pauseMenuRT);
    }
    private void SetRectTransform(RectTransform rectTransform)
    {
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition3D = Vector3.zero;

        //rectTransform.anchorMin = Vector2.zero;
        //rectTransform.anchorMax = Vector2.zero;
        //rectTransform.pivot = new Vector2(0.5f, 0.5f);
        //rectTransform.rotation = Quaternion.identity;
        //rectTransform.localScale = new Vector3(1, 1, 1);

        //https://stackoverflow.com/questions/46756823/positioning-ui-elements-with-anchor-presets-via-code
    }
    private void SetUIElements()
    {
        newGameWindow.SetActive(false);

        pauseMenu.SetActive(false);
        goToMainMenuWindow.SetActive(false);

        settingWindow.SetActive(false);
        //soundWindow.SetActive(false); //다른 설정 UI가 없다
        inputKeyWindow.SetActive(false);
        loadSlotWindow.SetActive(false);
        loadWindow.SetActive(false);
        quitWindow.SetActive(false);
    }

    private void SetButtonFunction()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            //새게임 시작 이벤트
            newGameButton.onClick.AddListener(() => { ShowStory(); });

            //스토리 끝에 게임 시작 기능들을 넣음
            StoryImage[StoryImage.Length - 1].onClick.AddListener(() => { SaveManager.instance.ResetData(); });
            StoryImage[StoryImage.Length - 1].onClick.AddListener(() => { timelineHelper.StartTutorialTimeline(); });
            StoryImage[StoryImage.Length - 1].onClick.AddListener(() => { SaveManager.instance.SelectIndex(0); });
            StoryImage[StoryImage.Length - 1].onClick.AddListener(() => { PlayGuide.instance.IsTutorialStart = true; });

            for (int i = 0; i < StoryImage.Length - 1; i++)
            {
                int index = i;

                StoryImage[i].onClick.AddListener(() => { StoryImage[index].gameObject.SetActive(false); });
            }
        }
        

        //newGameButton.onClick.AddListener(() => { ShowMouseCursor(false); });

        //불러오기 이벤트(불러오기 슬롯 이벤트는 따로)
        for (int i = 0; i < loadSlots.Length; i++)
        {
            loadSlots[i].onClick.AddListener(() =>
            {
                for (int j = 0; j < loadSlots.Length; j++)
                {
                    if (i == j)
                    {
                        selectImage[j].SetActive(true);
                    }
                    else
                    {
                        selectImage[j].SetActive(false);
                    }
                }
            }
            );
        }

        //선택된 슬롯의 index 확정짓고 씬불러오기
        loadButton.onClick.AddListener(() => SaveManager.instance.SetCurrentIndex());
        loadButton.onClick.AddListener(() => SceneSwitcher.instance.LoadScene());
        //불러오기를 통해 플레이어가 배치될 때 MoveToNextIndex()가 실행되는 것을 방지
        loadButton.onClick.AddListener(() => SceneSwitcher.instance.SkipRespawnSave = true);


        //일시정지 메뉴에서 메인메뉴로 돌아갈 때의 함수
        goToMainMenuButton.onClick.AddListener(() => { SceneManager.LoadScene(0); });
        goToMainMenuButton.onClick.AddListener(() => { Time.timeScale = 1; });
        goToMainMenuButton.onClick.AddListener(() => { SceneSwitcher.instance.IsNotPlayingMainMenu = false; });


        quitButton.onClick.AddListener(() => { Quit(); });

        openSettingButton.onClick.AddListener(() => { MobileInput.instance.OpenMenu(); });
        openSettingButton.onClick.AddListener(() => { ActivateLetterBox(true); });

        //플랫폼 스위치
        setPCButton.onClick.AddListener(() => { PlatformSwitcher.instance.SetPCPlatform(true); });
        setMobileButton.onClick.AddListener(() => { PlatformSwitcher.instance.SetPCPlatform(false); });

        //언어교체
        changeToKorean.onClick.AddListener(() => { LanguageManager.Instance.ChangeLanguage(0); });
        changeToKorean.onClick.AddListener(() => { ChangeSlotLanguage(); });
        changeToEnglish.onClick.AddListener(() => { LanguageManager.Instance.ChangeLanguage(1); });
        changeToEnglish.onClick.AddListener(() => { ChangeSlotLanguage(); });

        for (int i = 0; i < settingText.Length; i++)
        {
            int firstIndex = i;
            for (int j = 0; j < settingText.Length; j++)
            {
                int secondIndex = j;
                //본인이 클릭되었을 때
                if (firstIndex == secondIndex)
                {
                    settingButton[firstIndex].onClick.AddListener(() => { settingElements[secondIndex].SetActive(true); });
                    settingButton[firstIndex].onClick.AddListener(() => { settingTextBG[secondIndex].enabled = true; });
                    settingButton[firstIndex].onClick.AddListener(() => { settingText[secondIndex].color = Color.white; });
                }

                else if (firstIndex != secondIndex)
                {
                    settingButton[firstIndex].onClick.AddListener(() => { settingElements[secondIndex].SetActive(false); });
                    settingButton[firstIndex].onClick.AddListener(() => { settingTextBG[secondIndex].enabled = false; });
                    settingButton[firstIndex].onClick.AddListener(() => { settingText[secondIndex].color = Color.black; });
                }
            }
        }

    }
    #endregion
    
    public void ShowStory()
    {
        storyImageWindow.SetActive(true);

        for (int i = 0;i < StoryImage.Length;i++)
        {
            StoryImage[i].gameObject.SetActive(true);  
        }
    }

    public void DisablePlayerControl(bool actSwitch)
    {
        isPlayerControlDisabled = actSwitch;
    }

    public void MenuSwitch()
    {//Esc눌럿을 때 반응. StartMenu는 꺼지지 않음.
        //로딩중에는 간섭받지 않음.
        if (loadingUI.LoadBG.activeSelf)
        {
            return;
        }
        else if (settingWindow.activeSelf)
        {
            if (soundWindow.activeSelf)
            {
                //설정창이 닫힐 때 파일에 저장(+버튼에도 함수 등록)
                soundUI.SaveVolumeData();
            }

            else if (inputKeyWindow.activeSelf)
            {
                if (inputKeyUI.completeEditingKey)
                {
                    inputKeyUI.CompleteEditingKey(false);
                    return;
                }

                mouseUI.SaveMouseData();
            }

            //셋팅창 자체가 닫힘
            settingWindow.SetActive(false);
        }
        else if (loadSlotWindow.activeSelf)
        {
            Debug.Log(3);

            SaveManager.instance.SelectedIndex = null;

            if (loadWindow.activeSelf)
            {
                loadWindow.SetActive(false);
            }
            else
            {
                loadSlotWindow.SetActive(false);
            }
        }
        else if (quitWindow.activeSelf)
        {
            quitWindow.SetActive(false);
        }
        else if (pauseMenu.activeSelf)
        {//일시정지 메뉴
            if (goToMainMenuWindow.activeSelf)
            {
                goToMainMenuWindow.SetActive(false);
            }
            else
            {
                //일시정지 -> 플레이
                gameTimeScale.SetTimeScale(1);

                if (!timelineHelper.IsTimelinePlaying())
                {
                    //ShowMouseCursor(false);
                    isPlayerControlDisabled = false;
                    playerSound.TogglePlayingAudioPause(false);

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
                else
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.Confined;
                }

                pauseMenu.SetActive(false);
                ActivateLetterBox(false);
            }
        }
        else if (mainMenu.activeSelf)
        {//스타트 메뉴
            SaveManager.instance.SelectedIndex = null;

            if (settingWindow.activeSelf)
            {
                settingWindow.SetActive(false);
            }
            else if (newGameWindow.activeSelf)
            {
                newGameWindow.SetActive(false);
            }
        }
        else
        {//pauseMenu 활성화 시키기(pauseMenu 활성화 안되는 상황이 존재함)
            if (!canShowPauseMenu)
            {
                return;
            }

            gameTimeScale.SetTimeScale(0);

            isPlayerControlDisabled = true;

            //ShowMouseCursor(true);

            playerSound.TogglePlayingAudioPause(true);

            pauseMenu.SetActive(true);
            ActivateLetterBox(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
    public void CanShowPauseMenu(bool canShow)
    {
        canShowPauseMenu = canShow;
    }

    //public void OnOffHelpWindow()
    //{
    //    guideText.SetActive(!guideText.activeSelf);
    //}

    //public void HideAllHUD(bool hide)
    //{
    //    inputHUD.SetActive(!hide);
    //    battleHUD.SetActive(!hide);
    //}

    public void ActivateLetterBox(bool activate)
    {
        letterBoxImage.enabled = activate;

        letterBoxMaskImage.enabled = activate;
        letterBoxMask.enabled = activate;
    }

    public void SetPCPlatform(bool activate)
    {
        inputKeyWindow.transform.GetChild(0).gameObject.SetActive(activate);
        battleGuideHUD.SetActive(activate);

        inputHUD.SetActive(!activate);
    }

    #region Button
    public void Restart()
    {
        Time.timeScale = 1f;
        isPlayerControlDisabled = true;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
		    Application.Quit();
#endif
    }
    #endregion

    #region Load, Save(기록 창을 공용으로 만들어야겠음)
    public void RecordSlot(int filePathIndex, int sceneIndex, int areaIndex, string time)
    {
        //파일 경로 index와 동일한 index의 recordButtons[filePathIndex] 
        //시간 순서를 timeIndex에 기록
        loadSlots[filePathIndex].gameObject.SetActive(true);
        timeIndex[filePathIndex] = sceneIndex * 100 + areaIndex;

        //오브젝트 활성화 및 이벤트 부여
        loadSlots[filePathIndex].interactable = true;
        loadSlots[filePathIndex].onClick.RemoveAllListeners();
        loadSlots[filePathIndex].onClick.AddListener(() => loadWindow.SetActive(true));
        loadSlots[filePathIndex].onClick.AddListener(() => SaveManager.instance.SelectIndex(filePathIndex));

        #region 지역 텍스트
        string areaName;
        if (PlatformSwitcher.instance.IsKorean)
        {
            areaName = areaIndex switch
            {
                0 => "덕굴 옆 샛길",
                1 => "덕굴 입구 1",

                2 => "덕굴 입구 2",
                3 => "현무 중앙길",

                4 => "서쪽으로 가는 길",
                5 => "위수의 다리 1",
                6 => "위수의 다리 2",
                7 => "위수의 다리 3",
                8 => "위수의 다리 4",

                9 => "뱀의 정원 1",
                10 => "뱀의 정원 2",
                11 => "뱀의 정원 3",
                12 => "뱀의 정원 4",
                13 => "뱀의 정원 5",

                14 => "위수의 방",

                _ => "오류지역"
            };
        }
        else
        {
            areaName = areaIndex switch
            {
                0 => "Grace Cave Side Path",
                1 => "Grace Cave Entrance 1",

                2 => "Grace Cave Entrance 2",
                3 => "Hyeonmu Central Road",

                4 => "The path to the west",
                5 => "Wisu's Bridge 1",
                6 => "Wisu's Bridge 2",
                7 => "Wisu's Bridge 3",
                8 => "Wisu's Bridge 4",

                9 => "Ruined Wisu's Garden",
                10 => "Ruined Wisu's Garden",
                11 => "Ruined Wisu's Garden",
                12 => "Ruined Wisu's Garden",
                13 => "Ruined Wisu's Garden",

                14 => "Wisu's Area",

                _ => "Error"
            };
        }

        areaText[filePathIndex].text = areaName;
        #endregion

        #region SaveType
        if (PlatformSwitcher.instance.IsKorean)
        {
            saveTypeText[filePathIndex].text = "자동 저장";
        }
        else
        {
            saveTypeText[filePathIndex].text = "Auto Save";
        }

        #endregion

        #region PlayTime
        playTimeText[filePathIndex].text = "";
        #endregion

        #region Time
        dateText[filePathIndex].text = $"{time}";
        #endregion

        #region No Data
        noDataText[filePathIndex].text = "";
        #endregion

        //텍스트 작성

        //슬롯 정렬
        SortSlots();
    }
    public void ChangeSlotLanguage()
    {
        ResetSlot();
        for (int i = 0; i < SaveManager.instance.Filepath.Length; i++)
        {
            SaveManager.instance.WriteSlotDate(i);
        }
        SortSlots();


        //string areaName = null;
        //string saveType = null;

        //if (PlatformSwitcher.instance.IsKorean)
        //{
        //    for (int i = 0; i < areaText.Length; i++)
        //    {
        //        areaText[i].text = areaName switch
        //        {
        //            "Area00" => "덕굴 옆 샛길",
        //            "Area01" => "덕굴 입구 1",

        //            "Area10" => "덕굴 입구 2",
        //            "Area11" => "현무 중앙길",

        //            "Area20" => "서쪽으로 가는 길",
        //            "Area21" => "위수의 다리 1",
        //            "Area22" => "위수의 다리 2",
        //            "Area23" => "위수의 다리 3",
        //            "Area24" => "위수의 다리 4",

        //            "Area30" => "뱀의 정원 1",
        //            "Area31" => "뱀의 정원 2",
        //            "Area32" => "뱀의 정원 3",
        //            "Area33" => "뱀의 정원 4",
        //            "Area34" => "뱀의 정원 5",

        //            "Area40" => "위수의 방",
        //                _ => ""
        //        };

        //        saveTypeText[i].text = saveType switch
        //        {
        //            "Auto Save" => "자동 저장",
        //            _ => ""
        //        };
        //    }
        //}
        //else
        //{
        //    for (int i = 0; i < areaText.Length; i++)
        //    {
        //        areaText[i].text = areaName switch
        //        {
        //            "덕굴 옆 샛길" =>"Area00" , 
        //            "덕굴 입구 1" =>"Area01" , 

        //            "덕굴 입구 2" =>    "Area10" ,
        //            "현무 중앙길" => "Area11",

        //            "서쪽으로 가는 길" => "Area20",
        //            "위수의 다리 1" =>   "Area21",
        //            "위수의 다리 2" =>   "Area22",
        //            "위수의 다리 3" =>   "Area23",
        //            "위수의 다리 4" =>   "Area24",

        //            "뱀의 정원 1" =>"Area30",
        //            "뱀의 정원 2" =>"Area31",
        //            "뱀의 정원 3" =>"Area32",
        //            "뱀의 정원 4" =>"Area33",
        //            "뱀의 정원 5" => "Area34",

        //            "위수의 방" => "Area40",

        //            _ => ""
        //        };

        //        saveTypeText[i].text = saveType switch
        //        {
        //            "자동 저장" => "Auto Save",
        //            _ => ""
        //        };
        //    }
        //}
    }

    public void ResetSlot()
    {
        for (int i = 0; i < timeIndex.Length; i++)
        {
            areaText[i].text = "";
            saveTypeText[i].text = "";
            playTimeText[i].text = "";
            dateText[i].text = "";
            if (PlatformSwitcher.instance.IsKorean)
            {
                noDataText[i].text = "데이터 없음";
            }
            else
            {
                noDataText[i].text = "No Data";
            }

            timeIndex[i] = null;
        }
    }
    public void SortSlots()
    {
        //데이터가 있는 슬롯이라면 비교해서 정렬, 빈슬롯은 비활성화

        //timeIndex 순서대로 transform 위치 정렬
        bool[] isSelected = new bool[loadSlots.Length];

        for (int i = 0; i < loadSlots.Length; i++)
        {
            int index = 0;
            int biggestNumber = 0;

            for (int j = 0; j < loadSlots.Length; j++)
            {
                //슬롯 정보가 없다면 비활성화하고 다음 항으로
                if (!timeIndex[j].HasValue)
                {
                    //loadSlots[j].gameObject.SetActive(false);
                    continue;
                }


                //선택된 index 제외
                if (isSelected[j]) continue;

                if (biggestNumber < timeIndex[j].Value)
                {
                    biggestNumber = timeIndex[j].Value;
                    index = j;
                }
            }

            //남아있는 값 중에 제일 큰 값의 index

            isSelected[index] = true;
            loadSlots[index].transform.SetAsFirstSibling();
        }
    }

    public void CloseWindowUsingButton()
    {
        //if (inputKeyUI.isEditingKey)
        //{
        //    return;
        //}

        settingWindow.SetActive(false);
    }
    #endregion

}