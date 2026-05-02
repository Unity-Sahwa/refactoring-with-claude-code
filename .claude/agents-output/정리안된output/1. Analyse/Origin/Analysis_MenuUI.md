# MenuUI 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | MenuUI |
| 현재 역할 | 게임 메뉴 및 설정 UI 총괄 관리<br>- 메인 메뉴, 일시정지 메뉴, 설정 창<br>- 로드 슬롯 관리<br>- 플레이어 컨트롤 제어 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance 설정<br>2. timeIndex 배열 초기화 | - |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| UI 설정 및 초기화 | 1. 참조 획득<br>2. SetPosition() 호출<br>3. SetUIElements() 호출<br>4. SetButtonFunction() 호출<br>5. 장면별 초기 상태 설정 | TimelineHelper <br>LoadingUI <br>SceneManager |

### SetPosition()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| UI 요소 위치 설정 | 1. RectTransform 조정<br>2. 화면 전체 채우도록 설정 | - |

### SetRectTransform(RectTransform rectTransform)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| RectTransform 설정 | 1. anchorMin = (0, 0) 설정 <br>2. anchorMax = (1, 1) 설정 <br>3. offsetMin = (0, 0) 설정 <br>4. offsetMax = (0, 0) 설정 <br>5. 화면 전체를 채우는 레이아웃 적용 | RectTransform |

### SetButtonFunction()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 버튼 리스너 등록 | 1. 새 게임: ShowStory()<br>2. 로드: SaveManager 데이터 로드<br>3. 메인 메뉴: SceneManager.LoadScene(0)<br>4. 종료: Application.Quit()<br>5. 설정: 탭 제어<br>6. 언어 변경: LanguageManager 호출 | SaveManager <br>SceneManager <br>LanguageManager <br>PlatformSwitcher |

### MenuSwitch()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 메뉴 열기/닫기 토글 | **메뉴 열기:** 1. 설정 저장 처리<br>2. 일시정지 메뉴 활성화<br>3. Time.timeScale = 0 (게임 일시정지)<br><br>**메뉴 닫기:** 4. 설정 창 닫기<br>5. Time.timeScale 복구<br>6. 커서 상태 설정 | GameTimeScale <br>PlayerSound <br>TimelineHelper <br>SaveManager <br>Cursor |

### DisablePlayerControl(bool disableControl)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 컨트롤 활성화/비활성화 | 1. isPlayerControlDisabled = disableControl | - |

### RecordSlot(int filePathIndex, int sceneIndex, int areaIndex, string time)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 세이브 슬롯 기록 | 1. 슬롯 활성화<br>2. timeIndex 저장<br>3. 지역명, 플레이 타임, 날짜 표시 | PlatformSwitcher <br>SaveManager |

### SortSlots()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 슬롯 정렬 | 1. timeIndex 배열 정렬<br>2. 최근 저장 데이터 우선 | - |

### Restart()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 게임 재시작 | 1. Time.timeScale = 1<br>2. 현재 장면 로드 | SceneManager |

### ShowStory()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스토리 이미지 윈도우 표시 | 1. storyImageWindow.SetActive(true) <br>2. 모든 StoryImage 버튼 활성화 | GameObject |

### CanShowPauseMenu(bool canShow)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 일시정지 메뉴 표시 여부 제어 | 1. canShowPauseMenu = canShow 설정 <br>2. isPauseMenuShowing 플래그 업데이트 | 없음 |

### ActivateLetterBox(bool activate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 레터박스 활성화/비활성화 | 1. letterBox.SetActive(activate) <br>2. 화면 상단/하단 검은 바 표시 제어 | GameObject |

### SetPCPlatform(bool activate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| PC 플랫폼 설정 | 1. PlatformSwitcher 통해 PC 플랫폼으로 전환 <br>2. 버튼 UI 활성화 업데이트 | PlatformSwitcher |

### ChangeSlotLanguage()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 세이브 슬롯 언어 변경 | 1. 현재 언어 확인 (LanguageManager) <br>2. 모든 슬롯 텍스트를 현재 언어로 업데이트 <br>3. 지역명, 플레이 타임 표시 언어 변경 | LanguageManager <br>SaveManager |

### ResetSlot()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 세이브 슬롯 초기화 | 1. 선택된 슬롯 비활성화 <br>2. 슬롯 텍스트 초기화 <br>3. timeIndex 배열에서 해당 슬롯 제거 | SaveManager |

### CloseWindowUsingButton()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 버튼으로 창 닫기 | 1. 현재 활성화된 설정 창 비활성화 <br>2. 상위 메뉴로 돌아가기 | GameObject |

### Quit()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 게임 완전 종료 | 1. SaveManager에서 플레이어 현재 데이터 저장<br>2. 모든 설정(마우스, 키 입력, 음량) 최종 저장<br>3. Time.timeScale = 1로 게임 시간 복구<br>4. 모든 코루틴 정리<br>5. Application.Quit() 호출로 게임 프로세스 종료<br>6. Unity 에디터에서 실행 중이면 에디터도 종료<br>7. 빌드된 게임이면 애플리케이션 완전 종료<br>8. OS 제어권 복귀 | Application <br>SaveManager <br>Time |
