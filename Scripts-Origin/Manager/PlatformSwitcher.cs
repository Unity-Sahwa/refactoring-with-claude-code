using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformSwitcher : MonoBehaviour
{
    public static PlatformSwitcher instance;
    
    private bool isPCPlatform;
    public bool IsPCPlatform
    {
        get { return isPCPlatform; }
        set { isPCPlatform = value; }
    }

    //언어교체
    private bool isKorean;
    public bool IsKorean
    {
        get { return isKorean; }
        set { isKorean = value; }
    }


    private void Awake()
    {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion

        DontDestroyOnLoad(this.gameObject);
        
        isKorean = true;
    }

    private void Start()
    {
        //실행 최초 한번 저장
        //씬이 로딩될 때마다 SetPlatform 실행 
        SceneManager.sceneLoaded += OnSceneLoaded;

#if UNITY_ANDROID
        SetPCPlatform(false);
#else
        SetPCPlatform(true);
#endif
    }


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(isPCPlatform) 
        { 
            SetPCPlatform(true);
        }
        else
        {
            SetPCPlatform(false);
        }
    }


    public void SetPCPlatform(bool isSet)
    {
        isPCPlatform = isSet;

        MenuUI.instance.SetPCPlatform(isSet);
        CameraController.instance.SetPCPlatform(isSet);
    }
}
