# TimelineHelper 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | TimelineHelper |
| 현재 역할 | 타임라인 재생 관리 및 플레이어 컨트롤 활성화/비활성화<br>- 튜토리얼 타임라인 실행<br>- 페이드 인/아웃 효과 연출<br>- 플레이어 입력 제어<br>- 씬 전환 보조 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance가 null이면 현재 객체를 instance로 설정<br>2. instance가 이미 존재하면 현재 게임오브젝트 제거 | - |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 장면 초기화 및 플레이어 상태 설정 | 1. SceneManager.GetActiveScene().buildIndex로 현재 장면 판별<br>2. 튜토리얼 장면(buildIndex==0) 여부 확인<br>3. 플레이어 컨트롤 활성화 여부 결정<br>4. MenuUI에 메인 메뉴 활성화 상태 전달 | SceneManager <br>SceneSwitcher <br>MaskChange |

### StartTutorialTimeline()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 튜토리얼 타임라인 시작 (공개 메서드) | 1. CoStartTutorialTimeline() 코루틴 시작 | - |

### CoStartTutorialTimeline()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 튜토리얼 타임라인 시작 코루틴 | 1. FadeOutScreen(2) 호출로 페이드 아웃<br>2. 플레이어 위치/회전 설정<br>3. UI 요소 비활성화<br>4. 대기 처리<br>5. FadeInScreen(0.5f) 호출로 페이드 인<br>6. PlayableDirector 실행 시작 | FadeOutScreen() <br>FadeInScreen() <br>MenuUI <br>MaskChange |

### CoSkipTutorialStartScene()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 튜토리얼 시작 장면 스킵 코루틴 | 1. FadeOutScreen(2) 호출<br>2. 튜토리얼 요소 비활성화<br>3. Cursor.lockState 설정<br>4. 카메라, 타임라인, 버튼 비활성화<br>5. 플레이어 컨트롤 활성화<br>6. FadeInScreen(2) 호출 | PlatformSwitcher <br>Cursor <br>FadeOutScreen() <br>FadeInScreen() |

### SkipTutorialStartScene()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 튜토리얼 스킵 공개 메서드 | 1. CoSkipTutorialStartScene() 코루틴 시작 | - |

### IsTimelinePlaying()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 타임라인 재생 상태 확인 | 1. timelines 배열 전체 순회<br>2. 각 PlayableDirector의 playableGraph.IsValid() 확인<br>3. PlayState == PlayState.Playing 여부 확인<br>4. 하나라도 재생 중이면 true 반환, 모두 정지면 false 반환 | PlayableDirector |

### DisablePlayerControl(bool disableControl)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 컨트롤 활성화/비활성화 | **disableControl = true (비활성화):**<br>1. MenuUI에 컨트롤 상태 전달<br>2. PlayerSound.TogglePlayingAudioPause(true) - 음성 일시정지<br>3. PlayerState.ChangePlayerState(NONE) - 플레이어 상태 NONE<br>4. MouseSettingUI에서 마우스 입력 초기화<br><br>**disableControl = false (활성화):**<br>5. MenuUI에 컨트롤 활성화 전달<br>6. MouseSettingUI.LoadMouseData() - 저장된 마우스 설정 로드<br>7. Rigidbody의 isKinematic = false (물리 활성화)<br>8. PlatformSwitcher.SetCursor() - 커서 상태 설정 | MenuUI <br>PlayerSound <br>PlayerState <br>MouseSettingUI <br>Cursor <br>PlatformSwitcher |

### FadeInScreen(float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 화면 페이드인 | 1. timeRate 매개변수로 페이드 시간 조정<br>2. UIEffect.FadeInScreen(timeRate) 호출 | UIEffect |

### FadeOutScreen(float timeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 화면 페이드아웃 | 1. timeRate 매개변수로 페이드 시간 조정<br>2. UIEffect.FadeOutScreen(timeRate) 호출 | UIEffect |
