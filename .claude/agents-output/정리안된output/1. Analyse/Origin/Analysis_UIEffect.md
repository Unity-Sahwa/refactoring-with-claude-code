# UIEffect 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | UIEffect |
| 현재 역할 | UI 효과 관리<br>- 화면 페이드 인/아웃 효과<br>- 플레이어 HUD 페이드 효과<br>- 사망 화면 연출<br>- 보스 격퇴 화면 및 씬 전환 연출 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 및 데이터 로드 | 1. instance가 null이면 현재 객체를 instance로 설정<br>2. instance가 이미 존재하면 현재 게임오브젝트 Destroy<br>3. UIEffectData.Instance로 효과 데이터 로드 | UIEffectData |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| UI 객체 초기 상태 설정 | 1. fadeImage.gameObject.SetActive(false) (페이드 이미지 숨김)<br>2. deadImage.gameObject.SetActive(false) (사망 화면 이미지 숨김)<br>3. battleCanvasGroup.gameObject.SetActive(false) (플레이어 HUD 숨김) | Image |

### FadeOutScreen(float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 화면을 검은색으로 페이드 아웃 | 1. StartCoroutine(ShowFadeInOutScreen(false, timeRate)) 호출<br>2. false는 페이드 아웃 방향 지정 | ShowFadeInOutScreen() |

### FadeInScreen(float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 검은 페이드에서 화면 복구 | 1. StartCoroutine(ShowFadeInOutScreen(true, timeRate)) 호출<br>2. true는 페이드 인 방향 지정 | ShowFadeInOutScreen() |

### ShowFadeScreen(bool isFadeIn, float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 실행 중인 페이드 효과 중지 후 새로운 페이드 실행 | 1. fadeEffectCoroutine이 null이 아니면 StopCoroutine(fadeEffectCoroutine) 호출<br>2. fadeEffectCoroutine = StartCoroutine(ShowFadeInOutScreen(isFadeIn, timeRate))<br>3. 이전 페이드 효과를 중단하고 새로운 페이드 시작 | ShowFadeInOutScreen() |

### ShowFadeInOutScreen(bool isFadeIn, float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 화면 페이드 효과 코루틴 | **1단계 초기화:**<br>1. fadeImage.rectTransform.anchoredPosition3D = Vector3.zero<br>2. fadeImage.gameObject.SetActive(true)<br>3. alphaOfCanvasGroup = fadeCanvasGroup.alpha (현재 알파값 저장)<br>4. timer = 0<br><br>**2단계 페이드 루프 (while timer <= 1f):**<br>5. yield return null<br>6. timer += Time.unscaledDeltaTime * timeRate<br>7. **페이드 인(isFadeIn=true):** fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer) (투명)<br>8. **페이드 아웃(isFadeIn=false):** fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer) (검은색)<br>9. playerSound.TogglePlayingAudioPause(true) (음성 일시정지)<br><br>**3단계 완료:**<br>10. isFadeIn이 true이면 fadeImage.gameObject.SetActive(false) | CanvasGroup <br>Image |

### ActivatePlayerHUD(bool isActivate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 HUD 활성화/비활성화 | 1. battleCanvasGroup.gameObject.SetActive(isActivate)<br>2. isActivate=true: 플레이어 HUD(배틀 UI) 표시<br>3. isActivate=false: 플레이어 HUD 숨김 | CanvasGroup |

### IsPlayerHUDFading(bool isFade)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 HUD 페이드 유지 상태 설정 | 1. isPlayerHUDHoldingFade = isFade<br>2. HUD 페이드 효과 진행 중 유지 여부 제어 | - |

### ShowPlayerHUDFadeEffect()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 HUD 페이드 효과 실행 (조건부) | **조건 확인:**<br>1. isPlayerHUDHoldingFade가 true이고 battleCanvasGroup이 활성화되면 return (중복 페이드 방지)<br>2. TimelineHelper.instance.IsTimelinePlaying()이 true이면:<br>   - isPlayerHUDHoldingFade = false 설정<br>   - return (타임라인 재생 중에는 페이드 효과 중지)<br><br>**코루틴 실행:**<br>3. playerHUDCoroutine이 null이 아니면 StopCoroutine(playerHUDCoroutine)<br>4. playerHUDCoroutine = StartCoroutine(ShowUIFadeEffect(battleCanvasGroup)) | ShowUIFadeEffect() <br>TimelineHelper |

### ShowUIFadeEffect(CanvasGroup canvasGroup)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 HUD 페이드 인 → 유지 → 페이드 아웃 | **1단계 초기화:**<br>1. alphaOfCanvasGroup = canvasGroup.alpha (현재 알파값 저장)<br>2. canvasGroup.gameObject.SetActive(true)<br>3. timer = 0<br><br>**2단계 페이드 인 (while timer <= 1f || isPlayerHUDHoldingFade):**<br>4. yield return null<br>5. timer += Time.unscaledDeltaTime * UIEffectData.fadeInTimeRate<br>6. canvasGroup.alpha = Mathf.Lerp(alphaOfCanvasGroup, 1, timer) (불투명으로 변함)<br><br>**3단계 유지:**<br>7. yield return new WaitForSeconds(UIEffectData.floatingTime) (설정된 시간만큼 유지)<br><br>**4단계 페이드 아웃 (while timer <= 1f || isPlayerHUDHoldingFade):**<br>8. yield return null<br>9. timer += Time.unscaledDeltaTime * UIEffectData.fadeOutTimeRate<br>10. canvasGroup.alpha = Mathf.Lerp(1, 0, timer) (투명으로 변함)<br><br>**5단계 완료:**<br>11. isPlayerHUDHoldingFade = false | CanvasGroup <br>UIEffectData |

### ShowDeathScreen()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 사망 화면 연출 | **1단계 초기화:**<br>1. deadImage.gameObject.SetActive(true)<br>2. deadImage.rectTransform.anchoredPosition3D = Vector3.zero<br>3. timer = 0<br><br>**2단계 사망 화면 페이드 인 (while timer <= 1f):**<br>4. yield return null<br>5. timer += Time.unscaledDeltaTime * 10 (시간율 10배속 - 빠른 페이드)<br>6. deadCanvasGroup.alpha = Mathf.Lerp(0, 1, timer) (알파값 증가 → 사망 이미지 표시) | Image |

### ShowBossDefeatedScreen()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 보스 격퇴 화면 연출 및 씬 전환 | **1단계 초기 대기:**<br>1. yield return new WaitForSecondsRealtime(5f) (5초 대기)<br><br>**2단계 보스 격퇴 이미지 활성화:**<br>2. bossDefeatedImage.gameObject.SetActive(true)<br>3. bossDefeatedImage.rectTransform.anchoredPosition3D = Vector3.zero<br>4. yield return new WaitForSecondsRealtime(1f) (1초 대기)<br><br>**3단계 화면 페이드 인 (while timer <= 1f):**<br>5. yield return null<br>6. timer += Time.unscaledDeltaTime * 2f (시간율 2배속)<br>7. bossDefeatedCanvasGroup.alpha = Mathf.Lerp(0, 1, timer) (알파값 증가 → 격퇴 이미지 표시)<br><br>**4단계 최종 대기 후 씬 전환:**<br>8. yield return new WaitForSecondsRealtime(3.5f) (3.5초 대기)<br>9. SceneManager.LoadScene(0) (메인 씬으로 전환)<br><br>**총 소요 시간:** 약 10초 (5 + 1 + 페이드인 + 3.5) | Image <br>SceneManager |
