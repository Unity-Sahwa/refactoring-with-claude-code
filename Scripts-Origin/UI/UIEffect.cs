using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIEffect : MonoBehaviour
{
    public static UIEffect instance;

    [SerializeField] private PlayerSound playerSound;
    private UIEffectData UIEffectData;


    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private CanvasGroup deadCanvasGroup;
    [SerializeField] private CanvasGroup battleCanvasGroup;
    [SerializeField] private CanvasGroup bossDefeatedCanvasGroup;
    

    [SerializeField] Coroutine playerHUDCoroutine;
    public bool isPlayerHUDHoldingFade { get; private set; }
    public bool isPlayerHUDFade { get; private set; }

    [SerializeField] private Image fadeImage;
    [SerializeField] private Image deadImage;
    [SerializeField] private Image bossDefeatedImage;
    
    [SerializeField] Coroutine fadeEffectCoroutine;

    private void Awake()
    {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion
        UIEffectData = UIEffectData.Instance;
    }

    private void Start()
    {
        fadeImage.gameObject.SetActive(false);
        deadImage.gameObject.SetActive(false);
        battleCanvasGroup.gameObject.SetActive(false);
    }

    public void FadeOutScreen(float timeRate)
    {   //점점 검게 변하는
        StartCoroutine(ShowFadeInOutScreen(false, timeRate));
    }
    public void FadeInScreen(float timeRate)
    {
        //점점 원래 화면으로 나타나는
        StartCoroutine(ShowFadeInOutScreen(true, timeRate));
    }

    public void ShowFadeScreen(bool isFadeIn, float timeRate)
    {
        if (fadeEffectCoroutine != null)
        {
            StopCoroutine(fadeEffectCoroutine);
        }
        fadeEffectCoroutine = StartCoroutine(ShowFadeInOutScreen(isFadeIn, timeRate));
    }

    public IEnumerator ShowFadeInOutScreen(bool isFadeIn, float timeRate)
    {
        fadeImage.rectTransform.anchoredPosition3D = Vector3.zero;
        fadeImage.gameObject.SetActive(true);

        float alphaOfCanvasGroup = fadeCanvasGroup.alpha;
        float timer = 0f;
        
        while (timer <= 1f)
        {
            yield return null;

            //타임스케일 영향 X
            timer += Time.unscaledDeltaTime * timeRate;

            fadeCanvasGroup.alpha = isFadeIn ? Mathf.Lerp(1, 0f, timer) : Mathf.Lerp(0, 1f, timer);

            //임시로 계속 퍼즈
            playerSound.TogglePlayingAudioPause(true);
        }

        if (isFadeIn)
        {
            fadeImage.gameObject.SetActive(false);
        }

        yield break;
    }
    


    //PlayerHUD
    public void ActivatePlayerHUD(bool isActivate)
    {
        battleCanvasGroup.gameObject.SetActive(isActivate);
    }
    public void IsPlayerHUDFading(bool isFade)
    {
        isPlayerHUDHoldingFade = isFade;
    }
    public void ShowPlayerHUDFadeEffect()
    {
        if (isPlayerHUDHoldingFade && battleCanvasGroup.gameObject.activeSelf)
        {
            return;
        }

        if (TimelineHelper.instance.IsTimelinePlaying())
        {
            isPlayerHUDHoldingFade = false;
            return;
        }

        if (playerHUDCoroutine != null)
        {
            StopCoroutine(playerHUDCoroutine);
        }
        playerHUDCoroutine = StartCoroutine(ShowUIFadeEffect(battleCanvasGroup));
    }

    public IEnumerator ShowUIFadeEffect(CanvasGroup canvasGroup)
    {//fadeIn: alpha가 점점 1로 향함
        //canvasGroup의 alpha 값을 변수에 등록.
        float alphaOfCanvasGroup = canvasGroup.alpha;
        canvasGroup.gameObject.SetActive(true);

        //fadeIn
        float timer = 0f;
        while (timer <= 1f || isPlayerHUDHoldingFade)
        {
            yield return null;
            //타임스케일 영향 X
            timer += Time.unscaledDeltaTime * UIEffectData.fadeInTimeRate;
            canvasGroup.alpha = Mathf.Lerp(alphaOfCanvasGroup, 1f, timer);
        }
        //일정시간 떠있음
        yield return new WaitForSeconds(UIEffectData.floatingTime);
        //fadeOut
        timer = 0f;
        while (timer <= 1f || isPlayerHUDHoldingFade)
        {
            yield return null;
            //타임스케일 영향 X
            timer += Time.unscaledDeltaTime * UIEffectData.fadeOutTimeRate;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer);
        }
        //fadeImage.gameObject.SetActive(false);
        isPlayerHUDHoldingFade = false;
    }


    public IEnumerator ShowDeathScreen()
    {
        deadImage.gameObject.SetActive(true);
        
        deadImage.rectTransform.anchoredPosition3D = Vector3.zero;

        float timer = 0f;
        while (timer <= 1f)
        {
            yield return null;

            //시간 측정하는거 다른곳에도 써먹을수있을듯
            //타임스케일 영향 X
            timer += Time.unscaledDeltaTime * 10;

            deadCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
        }
    }

    public IEnumerator ShowBossDefeatedScreen()
    {
        yield return new WaitForSecondsRealtime(5f);

        bossDefeatedImage.gameObject.SetActive(true);

        bossDefeatedImage.rectTransform.anchoredPosition3D = Vector3.zero;

        yield return new WaitForSecondsRealtime(1f);

        float timer = 0f;
        while (timer <= 1f)
        {
            yield return null;

            //시간 측정하는거 다른곳에도 써먹을수있을듯
            //타임스케일 영향 X
            timer += Time.unscaledDeltaTime * 2f;

            bossDefeatedCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
        }

        yield return new WaitForSecondsRealtime(3.5f);

        SceneManager.LoadScene(0);
    }
}
