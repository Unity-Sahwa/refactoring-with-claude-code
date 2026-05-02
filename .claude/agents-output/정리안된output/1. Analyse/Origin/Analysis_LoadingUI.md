# LoadingUI 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | LoadingUI |
| 현재 역할 | 로딩 UI 관리<br>- 로딩 바 및 진행률 표시<br>- 페이드 인/아웃 화면 효과<br>- 씬 전환 시 로딩 연출 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance가 null이면 현재 객체를 instance로 설정<br>2. instance가 이미 존재하면 현재 게임오브젝트 Destroy (중복 생성 방지)<br>3. 게임 전역에서 하나의 LoadingUI만 존재 보장 | - |

### OnEnable()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 게임 객체 활성화 시 UI 초기화 | 1. LoadBG.gameObject.SetActive(false) (로딩 배경 처음엔 숨김)<br>2. fadeCanvasGroup.gameObject.SetActive(false) (페이드 효과 처음엔 숨김) | GameObject |

### StartLoading()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 로딩 코루틴 시작 | 1. StartCoroutine(Loading()) 호출로 로딩 시작 | Loading() |

### Loading()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 로딩 진행도 표시 및 진행 처리 | **1단계 초기화:**<br>1. Time.timeScale = 1 (게임 시간 정상 진행)<br>2. loadBG RectTransform 위치 설정 (offsetMin, offsetMax = 0)<br>3. loadBG.gameObject.SetActive(true) (로딩 화면 표시)<br>4. progressBar.fillAmount = 0 (진행도 초기화)<br>5. progressPercent.text = "0 %" (진행률 텍스트 초기화)<br>6. startTime = Time.time (로딩 시작 시간 기록)<br><br>**2단계 로딩 진행도 표시 (while progressBar.fillAmount <= 0.95f):**<br>7. currentRate = (Time.time - startTime) / loadingTime (경과 시간 / 예상 시간)<br>8. progressBar.fillAmount = Mathf.Lerp(0, currentRate, rate01) (부드러운 진행도 증가)<br>9. progressPercent.text = (progressBar.fillAmount * 100).ToString("F1") + " %" (진행률 텍스트 업데이트)<br>10. yield return null (매프레임 대기)<br><br>**3단계 로딩 완료:**<br>11. progressBar.fillAmount = 1f (100% 완료)<br>12. progressPercent.text = "100 %" (완료 텍스트)<br>13. yield return new WaitForSecondsRealtime(0.5f) (0.5초 대기)<br>14. loadBG.gameObject.SetActive(false) (로딩 화면 숨김) | Image <br>Slider |

### FadeOutScreen(float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 화면을 검은색으로 페이드 아웃 | 1. StartCoroutine(CoFadeScreen(false, timeRate)) 호출<br>2. false는 페이드 아웃 방향을 지정<br>3. 화면이 점차 검은색으로 변함 | CoFadeScreen() |

### FadeInScreen(float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 검은 페이드에서 화면 복구 | 1. StartCoroutine(CoFadeScreen(true, timeRate)) 호출<br>2. true는 페이드 인 방향을 지정<br>3. 검은 화면에서 점차 게임 화면 노출 | CoFadeScreen() |

### CoFadeScreen(bool isFadeIn, float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 페이드 효과 코루틴 구현 | **1단계 초기화:**<br>1. RectTransform rectTransform = fadeCanvasGroup.gameObject.GetComponent<RectTransform>()<br>2. rectTransform.anchoredPosition3D = Vector3.zero (위치 정중앙)<br>3. fadeCanvasGroup.gameObject.SetActive(true) (페이드 효과 활성화)<br>4. alphaOfCanvasGroup = fadeCanvasGroup.alpha (현재 알파값 저장)<br>5. timer = 0<br><br>**2단계 페이드 루프 (while timer <= 1f):**<br>6. yield return null (매프레임 대기)<br>7. timer += Time.unscaledDeltaTime * timeRate (타임스케일 무시하고 진행)<br>8. **페이드 인(isFadeIn=true):** fadeCanvasGroup.alpha = Mathf.Lerp(alphaOfCanvasGroup, 0, timer) (알파값 감소 → 투명)<br>9. **페이드 아웃(isFadeIn=false):** fadeCanvasGroup.alpha = Mathf.Lerp(alphaOfCanvasGroup, 1, timer) (알파값 증가 → 검은색)<br><br>**3단계 완료 처리:**<br>10. isFadeIn이 true이면 fadeCanvasGroup.gameObject.SetActive(false) (페이드 효과 비활성화) | CanvasGroup |

### FadeOutInScreen(float startTimeRate, float endTimeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 페이드 아웃 → 씬 로드 → 페이드 인 | 1. StartCoroutine(CoFadeOutInScreen(startTimeRate, endTimeRate)) 호출<br>2. startTimeRate: 페이드 아웃 속도<br>3. endTimeRate: 페이드 인 속도 | CoFadeOutInScreen() |

### CoFadeOutInScreen(float startTimeRate, float endTimeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 씬 전환 전체 연출 코루틴 | **1단계 초기화:**<br>1. RectTransform 위치 설정 (Vector3.zero)<br>2. fadeCanvasGroup.alpha = 0<br>3. fadeCanvasGroup.gameObject.SetActive(true)<br>4. timer = 0, isFadeOut = false, currentSceneIndex = SceneManager.GetActiveScene().buildIndex<br>5. sceneLoaded = false<br>6. SceneManager.sceneLoaded += SceneSwitcher.instance.OnSceneLoaded (씬 로드 콜백 등록)<br><br>**2단계 페이드 아웃 (while fadeCanvasGroup.alpha < 0.99f):**<br>7. yield return null<br>8. timer += Time.unscaledDeltaTime * startTimeRate<br>9. fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer) (알파값 증가 → 검은색)<br><br>**3단계 씬 로드 대기:**<br>10. fadeCanvasGroup.alpha = 1<br>11. timer = 0<br>12. yield return new WaitUntil(() => SceneSwitcher.instance.IsSceneLoaded) (씬 로드 완료 대기)<br>13. yield return new WaitForSecondsRealtime(2) (2초 추가 대기)<br><br>**4단계 페이드 인 (while fadeCanvasGroup.alpha > 0.1f):**<br>14. yield return null<br>15. timer += Time.unscaledDeltaTime * endTimeRate<br>16. fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer) (알파값 감소 → 투명)<br><br>**5단계 완료:**<br>17. SceneManager.sceneLoaded -= SceneSwitcher.instance.OnSceneLoaded (콜백 제거)<br>18. SceneSwitcher.instance.IsNotSceneLoaded() (씬 로드 플래그 초기화)<br>19. fadeCanvasGroup.alpha = 0<br>20. fadeCanvasGroup.gameObject.SetActive(false) | SceneManager <br>SceneSwitcher |
