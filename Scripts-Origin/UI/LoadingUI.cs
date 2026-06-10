using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//SceneSwitcher

public class LoadingUI : MonoBehaviour
{
    public static LoadingUI instance;

    #region Loading
    [SerializeField] private Image loadBG;
    public GameObject LoadBG 
    {
        get
        { 
            return loadBG.gameObject; 
        }
    }

    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI progressPercent;
    
    [SerializeField] private float loadingTime;
    [SerializeField] private float rate01;
    #endregion

    #region Fade
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    #endregion

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
    }

    //비파괴 오브젝트라서 처음 한 번만 실행
    private void OnEnable()
    {
        LoadBG.gameObject.SetActive(false); 
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    public void StartLoading()
    {
        StartCoroutine(Loading());
    }

    public IEnumerator Loading()
    {
        Time.timeScale = 1;

        //위치 재정렬
        loadBG.rectTransform.offsetMin = Vector2.zero;
        loadBG.rectTransform.offsetMax = Vector2.zero;
        loadBG.rectTransform.anchoredPosition3D = Vector3.zero;

        loadBG.gameObject.SetActive(true);
        progressBar.fillAmount = 0;
        progressPercent.text = "0 %";

        float startTime = Time.time;

        while (progressBar.fillAmount <= 0.95f)
        {
            //loadingTime만큼 진행
            float currentRate = (Time.time - startTime ) / loadingTime;
            progressBar.fillAmount = Mathf.Lerp(0f, currentRate,rate01);
            //progressBar.fillAmount = currentRate;
            progressPercent.text = (progressBar.fillAmount*100).ToString("F1")+ " %"; 

            yield return null;
        }

        progressBar.fillAmount = 1f;
        progressPercent.text = "100 %";

        yield return new WaitForSecondsRealtime(0.5f);
        
        loadBG.gameObject.SetActive(false);
    }

    public void FadeOutScreen(float timeRate)
    {   //점점 어두워짐
        StartCoroutine(CoFadeScreen(false, timeRate));
    }
    public void FadeInScreen(float timeRate)
    {
        //점점 원래 화면으로
        StartCoroutine(CoFadeScreen(true, timeRate));
    }
    public IEnumerator CoFadeScreen(bool isFadeIn, float timeRate)
    {
        //페이드 이미지를 화면에 배치, 활성화
        RectTransform rectTransform = fadeCanvasGroup.gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = Vector3.zero;
        fadeCanvasGroup.gameObject.SetActive(true);

        //캔버스그룹을 통해 투명도제어
        float alphaOfCanvasGroup = fadeCanvasGroup.alpha;
        float timer = 0f;

        while (timer <= 1f)
        {
            yield return null;

            //타임스케일 영향 X
            timer += Time.unscaledDeltaTime * timeRate;

            fadeCanvasGroup.alpha = isFadeIn ? Mathf.Lerp(alphaOfCanvasGroup, 0f, timer) : Mathf.Lerp(alphaOfCanvasGroup, 1f, timer);
        }

        if (isFadeIn)
        {
            fadeCanvasGroup.gameObject.SetActive(false);
        }

        yield break;
    }


    //FadeOut -> Duration -> FadeIn
    public void FadeOutInScreen(float startTimeRate, float endTimeRate)
    {
        StartCoroutine(CoFadeOutInScreen(startTimeRate, endTimeRate));
    }

    public IEnumerator CoFadeOutInScreen(float startTimeRate, float endTimeRate)
    {
        RectTransform rectTransform = fadeCanvasGroup.gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition3D = Vector3.zero;

        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.gameObject.SetActive(true);

        float timer = 0f;
        bool isFadeOut = false;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        bool sceneLoaded = false;

        //씬이 로드될 때 IsSceneLoaded가 실행되도록 추가
        SceneManager.sceneLoaded += SceneSwitcher.instance.OnSceneLoaded;

        //fadeIn
        while (fadeCanvasGroup.alpha < .99f)
        {
            //timeFlow
            yield return null;

            timer += Time.unscaledDeltaTime * startTimeRate;

            //t는 시작지점과 끝지점의 사이에 위치한 비율이라고 이해하면 될 것 같다. t=0.5f 면 중간값을 반환시켜줌
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1f, timer);
        }

        fadeCanvasGroup.alpha = 1;
        timer = 0;


        //씬로드 될때까지 대기
        //TODO: 같은 씬에서는 해당이 안되는 듯하다. 큰문제임... 
        yield return new WaitUntil(()=> SceneSwitcher.instance.IsSceneLoaded);
        yield return new WaitForSecondsRealtime(2);

        while (fadeCanvasGroup.alpha > .1f)
        {
            //timeFlow
            yield return null;

            timer += Time.unscaledDeltaTime * endTimeRate;

            //t는 시작지점과 끝지점의 사이에 위치한 비율이라고 이해하면 될 것 같다. t=0.5f 면 중간값을 반환시켜줌
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0f, timer);
        }

        //로드될 때 실행될 함수를 제거하고 내부 bool 변수도 초기화
        SceneManager.sceneLoaded -= SceneSwitcher.instance.OnSceneLoaded;
        SceneSwitcher.instance.IsNotSceneLoaded();
        
        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.gameObject.SetActive(false);
    }
}
