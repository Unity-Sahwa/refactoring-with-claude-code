using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static SceneSwitcher instance;

    private SaveManager saveManager;
    private MaskChange maskChange;

    private bool isNotPlayingMainMenu;
    public bool IsNotPlayingMainMenu
    {
        set
        {
            isNotPlayingMainMenu = value ;
        }
        get
        {
            return isNotPlayingMainMenu;
        }
    }
    
    //세이브 지점으로 불러올 때 한번더 세이브 되는 기능 일부를 스킵하기 위함
    private bool skipRespawnSave;
    public bool SkipRespawnSave
    {
        set
        {
            skipRespawnSave = value;
        }
        get
        {
            return skipRespawnSave;
        }
    }

    private bool isSceneLoaded = false;
    public bool IsSceneLoaded
    {
        get { return isSceneLoaded; }
        set { isSceneLoaded = value; }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        isNotPlayingMainMenu = false;
    }
    private void Start()
    {
        saveManager = SaveManager.instance;
        maskChange = MaskChange.instance;

        DontDestroyOnLoad(this);


        //TODO:UI 켜질때만 작동을 해야한다
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            MenuUI.instance.ActivateLetterBox(true);
        }
        else
        {
            MenuUI.instance.ActivateLetterBox(false);
        }
    }

    //비파괴 오브젝트라서 OnEnable은 게임 실행 후 1번만 실행
    //private void OnEnable()
    //{
    //    LoadingUI.instance.StartLoading();
    //}

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneLoaded = true;
    }

    public void IsNotSceneLoaded()
    {
        isSceneLoaded = false;
    }
    public void SwitchScene(int index) //트리거를 통해 다음 씬으로 넘어감
    {
        StartCoroutine(CoSwitchScene(index));
    }

    public IEnumerator CoSwitchScene(int index)
    {
        //TODO: 플레이어가 일정시간동안 정지할 필요가 있다. -> bool 값으로
        //씬을 넘나들어야하니까 비파괴 오브젝트에 붙어있어야함

        //현재 씬의 최신데이터 임시저장, currentIndex도 전달을 위해 저장
        float currentHP = Player.instance.currentHP;
        bool isHumanMask = MaskChange.instance.HumanMask.activeSelf;
        int currentIndex = SaveManager.instance.CurrentIndex;
        
        SceneManager.LoadScene(index);
        
        yield return new WaitForSeconds(0.5f);

        CameraController.instance.TerrainLoadCamera.gameObject.SetActive(true);
        CameraController.instance.DefaultCamera.gameObject.SetActive(false);
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_RecenteringTime = 0;
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_enabled = true;

        

        //재할당
        saveManager = SaveManager.instance;
        maskChange = MaskChange.instance;

        if (TimelineHelper.instance.startNewGameCamera != null)
        {
            TimelineHelper.instance.startNewGameCamera.SetActive(false);
        }
        if (MenuUI.instance.MainMenu != null)
        {
            MenuUI.instance.MainMenu.SetActive(false);
        }

        //인게임에서 커서 설정
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

        //이전 씬의 데이터 로드, 포지션 재배치
        Player.instance.currentHP = currentHP;
        HpHUD.instance.ChangeHPStack((int)currentHP);
        if (isHumanMask)
        {
            if (!maskChange.HumanMask.activeSelf)
            {
                maskChange.ChangeCharacter();
            }
        }
        else
        {
            if (maskChange.HumanMask.activeSelf)
            {
                maskChange.ChangeCharacter();
            }
        }

        saveManager.CurrentIndex = currentIndex;

        saveManager.ResetPlayerPosition();

        yield return new WaitForSeconds(0.5f);
        CameraController.instance.TerrainLoadCamera.gameObject.SetActive(false);
        CameraController.instance.DefaultCamera.gameObject.SetActive(true);

        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_RecenteringTime = 1;
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_enabled = false;
        CameraController.instance.ChangeCamera(CameraType.DEFAULT);
        yield return new WaitForSeconds(1f);
    }

    public void LoadScene() //불러오기를 통한 씬전환(데이터 셋팅)
    {
        StartCoroutine(CoLoadScene());
    }
    public IEnumerator CoLoadScene()
    {
        //경로[currentIndex]에 해당하는 데이터 얻어오기
        saveManager.GetLoadData();
        
        //LoadingUI.instance.FadeOutInScreen(1, 1);
        LoadingUI.instance.FadeOutScreen(4);

        int currentIndex = saveManager.CurrentIndex;

        if (saveManager.CurrentSceneIndex == 0)
        {
            //메인메뉴 불러오는 게 아니라 메인메뉴 기능 비활성화 시킴, 튜토리얼 지역을 불러옴. 
            isNotPlayingMainMenu = true;
        }

        //TODO: WaitForSecond를 할 경우 시간에 영향을 받음. 너무 조잡하게 코드작성함
        yield return new WaitForSecondsRealtime (0.5f);

        SceneManager.LoadScene(saveManager.CurrentSceneIndex);

        yield return null;

        saveManager = SaveManager.instance;
        saveManager.CurrentIndex = currentIndex;
        saveManager.LoadSlotData();
        CameraController.instance.TerrainLoadCamera.gameObject.SetActive(true);
        CameraController.instance.DefaultCamera.gameObject.SetActive(false);
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_RecenteringTime = 0;
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_enabled = true;

        Time.timeScale = 1;

        //인게임에서 커서 설정
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

        yield return new WaitForSecondsRealtime(0.5f);
        CameraController.instance.TerrainLoadCamera.gameObject.SetActive(false);
        CameraController.instance.DefaultCamera.gameObject.SetActive(true);
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_RecenteringTime = 1;
        CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_enabled = false;
        CameraController.instance.ChangeCamera(CameraType.DEFAULT);
        
        yield return new WaitForSecondsRealtime(1f);

        MenuUI.instance.ActivateLetterBox(false);
        LoadingUI.instance.FadeInScreen(2);

        yield return new WaitForSecondsRealtime(1f);
    }
}
