# UI 시스템 분석

> 대상 원본: `Scripts-Origin/UI/` 11개 파일
> 형식: analysis-origin (수집 → 분석) + 기능 흐름 + 예전 MVP/Popup 평가

---

## 기존 코드 위치 및 역할

### HpHUD
| 위치 | 역할 |
| --- | --- |
| [HpHUD.Awake()](../../../../Scripts-Origin/UI/HpHUD.cs) | 자기 자신을 전역 인스턴스(싱글톤)로 등록한다. |
| [HpHUD.Start()](../../../../Scripts-Origin/UI/HpHUD.cs) | Player 전역 인스턴스를 받아 보관한다. |
| [HpHUD.ChangeHPStack()](../../../../Scripts-Origin/UI/HpHUD.cs) | 현재 체력값을 받아, 그 수만큼 하트 칸을 켜고 나머지는 끄며 "현재/최대" 텍스트를 갱신한다. |

### PlayerHUD
| 위치 | 역할 |
| --- | --- |
| [PlayerHUD.UpdateMask()](../../../../Scripts-Origin/UI/PlayerHUD.cs) | 마스크 번호를 받아, 범위가 맞으면 해당 스프라이트로 마스크 아이콘 이미지를 바꾸고 켠다. |

### SkillHUD
| 위치 | 역할 |
| --- | --- |
| [SkillHUD.ActivateFinishHUD()](../../../../Scripts-Origin/UI/SkillHUD.cs) | on/off 값을 받아 마무리 공격 아이콘을 켜거나 끈다. |
| [SkillHUD.SkillCooldown()](../../../../Scripts-Origin/UI/SkillHUD.cs) | 플레이어 상태와 진행률(0~1)을 받아, 그 상태에 맞는 스킬 아이콘의 채움량을 진행률만큼 채운다(쿨다운 시각화). |
| [SkillHUD.ChangeGuideHUDColor()](../../../../Scripts-Origin/UI/SkillHUD.cs) | 플레이어 상태와 색을 받아, 그 상태에 맞는 스킬 아이콘 색을 바꾼다. |
| [SkillHUD.SetImage()](../../../../Scripts-Origin/UI/SkillHUD.cs) | 플레이어 상태를 받아, 그에 대응하는 스킬 아이콘 이미지를 골라 돌려준다. |
| [SkillHUD.ChangeIcon()](../../../../Scripts-Origin/UI/SkillHUD.cs) | 마스크 종류를 받아, 인간/동물에 맞는 특수공격 아이콘만 켜고 버튼의 대상 그래픽을 바꾼다. |

### MinimapHUD
| 위치 | 역할 |
| --- | --- |
| [MinimapHUD.FixedUpdate()](../../../../Scripts-Origin/UI/MinimapHUD.cs) | 매 물리 프레임마다 미니맵 갱신 메서드를 호출한다. |
| [MinimapHUD.ShowMinimap()](../../../../Scripts-Origin/UI/MinimapHUD.cs) | 현재 마스크의 위치를 읽어, 미니맵 카메라의 x·z를 그 위치로 옮긴다(높이는 고정). |

### TargetIndicator
| 위치 | 역할 |
| --- | --- |
| [TargetIndicator.Update()](../../../../Scripts-Origin/UI/TargetIndicator.cs) | 매 프레임 타겟의 월드 위치를 화면 좌표로 바꿔, 화면 안이면 그 자리에, 밖이면 화면 가장자리에 화살표를 두고 알파를 깜빡이게 한다. |

### SoundSettingUI
| 위치 | 역할 |
| --- | --- |
| [SoundSettingUI.Awake()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 볼륨 슬라이더 4개에 값 변경 시 호출될 함수를 연결한다. |
| [SoundSettingUI.Start()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | SaveManager를 받고 저장된 볼륨을 불러온다. |
| [SoundSettingUI.SetMasterVolume()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 마스터 볼륨값을 받아 적·플레이어·환경 믹서의 Master에 그대로 적용한다. |
| [SoundSettingUI.SetBGM()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 값을 받아 환경 믹서의 BGM과 SFX에 적용한다. |
| [SoundSettingUI.SetEnemySFX()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 값을 받아 적 믹서의 SFX에 적용한다. |
| [SoundSettingUI.SetPlayerSFX()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 값을 받아 플레이어 믹서의 SFX에 적용한다. |
| [SoundSettingUI.MuteAudio()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 마스터 볼륨을 0으로 만들어 음소거한다. |
| [SoundSettingUI.SaveVolumeData()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 슬라이더 4개의 현재 값을 SaveManager에 넘겨 저장한다. |
| [SoundSettingUI.LoadVolumeData()](../../../../Scripts-Origin/UI/SoundSettingUI.cs) | 저장된 볼륨을 불러와 믹서에 적용하고 슬라이더 위치도 맞춘다. |

### InputKeySettingUI
| 위치 | 역할 |
| --- | --- |
| [InputKeySettingUI.Awake()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | 키 버튼마다 "그 키 편집 시작" 함수를 연결하고, 버튼 안 텍스트를 미리 모아둔다. |
| [InputKeySettingUI.Start()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | SaveManager를 받고, 키 종류마다 현재 키값을 버튼 텍스트에 표시한다. |
| [InputKeySettingUI.Update()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | 편집 중이면 눌린 키를 감지해, 그 키로 설정을 바꾸고 저장한 뒤 버튼 텍스트를 갱신한다(메뉴키는 좌클릭 무시). |
| [InputKeySettingUI.CompleteEditingKey()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | 편집 완료 여부 플래그를 설정한다. |
| [InputKeySettingUI.DetectPressedKeyCode()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | 모든 키코드를 훑어 지금 눌린 키를 돌려준다(없으면 None). |
| [InputKeySettingUI.EditInputKey()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | 편집 대상 인덱스를 받아 편집 모드로 들어간다. |
| [InputKeySettingUI.ChangeInputKeyButtonText()](../../../../Scripts-Origin/UI/InputKeySettingUI.cs) | 인덱스를 받아, 저장된 그 키값을 해당 버튼 텍스트에 적는다. |

### MouseSettingUI
| 위치 | 역할 |
| --- | --- |
| [MouseSettingUI.Awake()](../../../../Scripts-Origin/UI/MouseSettingUI.cs) | 마우스 감도 슬라이더 2개(X·Y)에 값 변경 함수를 연결한다. |
| [MouseSettingUI.Start()](../../../../Scripts-Origin/UI/MouseSettingUI.cs) | 저장된 마우스 감도를 불러온다. |
| [MouseSettingUI.SetXAxisValue()](../../../../Scripts-Origin/UI/MouseSettingUI.cs) | X 감도값을 받아 텍스트로 표시하고 카메라에 적용한다. |
| [MouseSettingUI.SetYAxisValue()](../../../../Scripts-Origin/UI/MouseSettingUI.cs) | Y 감도값을 받아 텍스트로 표시하고 카메라에 적용한다. |
| [MouseSettingUI.SaveMouseData()](../../../../Scripts-Origin/UI/MouseSettingUI.cs) | 카메라의 현재 X·Y 감도를 읽어 SaveManager에 저장한다. |
| [MouseSettingUI.LoadMouseData()](../../../../Scripts-Origin/UI/MouseSettingUI.cs) | 저장된 감도를 불러와 카메라·슬라이더·텍스트에 모두 반영한다. |

### LoadingUI
| 위치 | 역할 |
| --- | --- |
| [LoadingUI.Awake()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 자기 자신을 전역 인스턴스로 등록한다. |
| [LoadingUI.OnEnable()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 로딩 배경과 페이드 화면을 꺼둔다. |
| [LoadingUI.StartLoading()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 로딩 진행 코루틴을 시작한다. |
| [LoadingUI.Loading()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 정해진 시간 동안 진행 바를 채우고 퍼센트를 갱신하다, 다 차면 잠시 뒤 로딩 배경을 끈다(코루틴). |
| [LoadingUI.FadeOutScreen()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 속도값을 받아 화면을 어둡게 하는 페이드 코루틴을 시작한다. |
| [LoadingUI.FadeInScreen()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 속도값을 받아 어두운 화면을 밝히는 페이드 코루틴을 시작한다. |
| [LoadingUI.CoFadeScreen()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 방향과 속도를 받아 캔버스 그룹의 투명도를 0↔1로 서서히 바꾼다(코루틴). |
| [LoadingUI.FadeOutInScreen()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 시작·끝 속도를 받아 "어둡게→대기→밝게" 코루틴을 시작한다. |
| [LoadingUI.CoFadeOutInScreen()](../../../../Scripts-Origin/UI/LoadingUI.cs) | 화면을 어둡게 한 뒤 씬 로드가 끝날 때까지 기다렸다가 다시 밝힌다(씬 전환 연출 코루틴). |

### UIEffect
| 위치 | 역할 |
| --- | --- |
| [UIEffect.Awake()](../../../../Scripts-Origin/UI/UIEffect.cs) | 자기 자신을 전역 인스턴스로 등록하고 연출 설정 데이터를 받는다. |
| [UIEffect.Start()](../../../../Scripts-Origin/UI/UIEffect.cs) | 페이드·사망·전투 화면을 꺼둔다. |
| [UIEffect.FadeOutScreen()](../../../../Scripts-Origin/UI/UIEffect.cs) | 속도값을 받아 화면 페이드 아웃 코루틴을 시작한다. |
| [UIEffect.FadeInScreen()](../../../../Scripts-Origin/UI/UIEffect.cs) | 속도값을 받아 화면 페이드 인 코루틴을 시작한다. |
| [UIEffect.ShowFadeScreen()](../../../../Scripts-Origin/UI/UIEffect.cs) | 방향·속도를 받아, 진행 중이던 페이드를 멈추고 새 페이드를 시작한다. |
| [UIEffect.ShowFadeInOutScreen()](../../../../Scripts-Origin/UI/UIEffect.cs) | 방향·속도에 맞춰 화면 투명도를 바꾸며, 진행 중 소리를 잠시 멈춘다(코루틴). |
| [UIEffect.ActivatePlayerHUD()](../../../../Scripts-Origin/UI/UIEffect.cs) | on/off 값을 받아 전투 HUD 묶음을 켜거나 끈다. |
| [UIEffect.IsPlayerHUDFading()](../../../../Scripts-Origin/UI/UIEffect.cs) | HUD 페이드를 유지할지 여부 플래그를 설정한다. |
| [UIEffect.ShowPlayerHUDFadeEffect()](../../../../Scripts-Origin/UI/UIEffect.cs) | 조건(유지 중·타임라인 재생 중)을 확인한 뒤 전투 HUD를 잠깐 보였다 사라지게 하는 코루틴을 시작한다. |
| [UIEffect.ShowUIFadeEffect()](../../../../Scripts-Origin/UI/UIEffect.cs) | 캔버스 그룹을 받아 "서서히 나타남→잠시 머묾→사라짐"을 수행한다(코루틴). |
| [UIEffect.ShowDeathScreen()](../../../../Scripts-Origin/UI/UIEffect.cs) | 사망 화면을 서서히 띄운다(코루틴). |
| [UIEffect.ShowBossDefeatedScreen()](../../../../Scripts-Origin/UI/UIEffect.cs) | 일정 시간 뒤 보스 처치 화면을 띄우고, 잠시 후 첫 씬으로 이동한다(코루틴). |

### MenuUI
| 위치 | 역할 |
| --- | --- |
| [MenuUI.Awake()](../../../../Scripts-Origin/UI/MenuUI.cs) | 자기 자신을 전역 인스턴스로 등록하고, 슬롯별 정렬용 시간 인덱스 배열을 비워 둔다. |
| [MenuUI.Start()](../../../../Scripts-Origin/UI/MenuUI.cs) | 위치·UI·버튼을 한 번에 세팅하고, 지금이 메인메뉴인지에 따라 커서와 플레이어 조작 상태를 정한다. |
| [MenuUI.SetPosition()](../../../../Scripts-Origin/UI/MenuUI.cs) | 메인메뉴·일시정지·설정창의 위치를 화면 전체에 맞춘다. |
| [MenuUI.SetRectTransform()](../../../../Scripts-Origin/UI/MenuUI.cs) | 사각 영역을 받아 여백과 위치를 0으로 맞춘다. |
| [MenuUI.SetUIElements()](../../../../Scripts-Origin/UI/MenuUI.cs) | 새 게임·일시정지·설정·로드·종료 등 여러 창을 처음에 모두 꺼둔다. |
| [MenuUI.SetButtonFunction()](../../../../Scripts-Origin/UI/MenuUI.cs) | 새 게임·스토리·로드 슬롯·로드·메인 복귀·종료·설정·플랫폼·언어·설정 탭 등 모든 버튼의 클릭 동작을 코드로 연결한다. |
| [MenuUI.ShowStory()](../../../../Scripts-Origin/UI/MenuUI.cs) | 스토리 이미지 창과 그 안의 이미지들을 모두 켠다. |
| [MenuUI.DisablePlayerControl()](../../../../Scripts-Origin/UI/MenuUI.cs) | on/off 값을 받아 플레이어 조작 불가 상태를 설정한다. |
| [MenuUI.MenuSwitch()](../../../../Scripts-Origin/UI/MenuUI.cs) | Esc 등으로 호출되어, 지금 열려 있는 창이 무엇인지에 따라 닫기·설정 저장·일시정지 토글을 분기 처리한다. |
| [MenuUI.CanShowPauseMenu()](../../../../Scripts-Origin/UI/MenuUI.cs) | 일시정지 메뉴를 띄울 수 있는지 여부 플래그를 설정한다. |
| [MenuUI.ActivateLetterBox()](../../../../Scripts-Origin/UI/MenuUI.cs) | on/off 값을 받아 위아래 검은 띠(레터박스) 이미지와 마스크를 켜거나 끈다. |
| [MenuUI.SetPCPlatform()](../../../../Scripts-Origin/UI/MenuUI.cs) | PC 여부를 받아 입력 HUD와 전투 가이드 HUD를 플랫폼에 맞게 전환한다. |
| [MenuUI.Restart()](../../../../Scripts-Origin/UI/MenuUI.cs) | 시간 흐름을 정상으로 돌리고 현재 씬을 다시 불러온다. |
| [MenuUI.Quit()](../../../../Scripts-Origin/UI/MenuUI.cs) | 게임을 종료한다(에디터에서는 플레이 중지). |
| [MenuUI.RecordSlot()](../../../../Scripts-Origin/UI/MenuUI.cs) | 슬롯 번호·씬·지역·시간을 받아, 그 슬롯에 지역명·저장 종류·날짜를 적고 로드 이벤트를 달고 정렬한다. |
| [MenuUI.ChangeSlotLanguage()](../../../../Scripts-Origin/UI/MenuUI.cs) | 슬롯을 비운 뒤 현재 언어에 맞춰 모든 슬롯 정보를 다시 적고 정렬한다. |
| [MenuUI.ResetSlot()](../../../../Scripts-Origin/UI/MenuUI.cs) | 모든 슬롯 텍스트를 비우고 "데이터 없음"으로 표시하며 정렬값을 초기화한다. |
| [MenuUI.SortSlots()](../../../../Scripts-Origin/UI/MenuUI.cs) | 저장 시간 인덱스를 기준으로 슬롯을 최신순으로 위로 올려 정렬한다. |
| [MenuUI.CloseWindowUsingButton()](../../../../Scripts-Origin/UI/MenuUI.cs) | 버튼으로 설정창을 닫는다. |

<br>
<br>
<br>

# 분석

### 기능 필요성

플레이어는 자기 상태(체력·마스크·스킬 쿨다운)와 게임 상황(로딩·사망·보스 처치)을 화면으로 알아야 하고, 게임을 멈추고 설정을 바꾸거나 세이브를 골라 이어 할 수단이 필요하다. UI 시스템은 이 "상태를 보여주는 일"과 "플레이어의 선택을 받아 게임에 전달하는 일"을 맡는다. 지금은 이 두 일이 게임플레이 코드 곳곳에 직접 박혀 있어, UI를 손대면 전투·세이브·씬 전환 코드가 함께 흔들린다. 보여주기와 입력받기를 한 곳으로 모아 게임플레이와 분리하는 것이 이 시스템의 목적이다.

### 필요한 기능

1. **플레이어 상태 표시 (HUD)**
   - 체력(하트 칸 + 텍스트), 현재 마스크 아이콘, 스킬 쿨다운 채움·색, 마무리 공격 아이콘.
   - 방향: 게임플레이가 값을 직접 넣어주는 지금 방식 대신, 상태가 바뀔 때 알림(event)을 보내고 HUD가 받아 갱신.

2. **미니맵·타겟 표시**
   - 미니맵 카메라가 플레이어를 따라가기, 화면 밖 타겟을 가장자리 화살표로 표시.
   - 방향: 따라갈 대상(Transform)만 밖에서 받아 두고 내부에서 갱신.

3. **메뉴 / 일시정지**
   - 메인메뉴, 일시정지, 그 위에 뜨는 확인 창(종료·메인으로·로드), Esc로 맨 위 창부터 닫기.
   - 방향: 창마다 자기 열고 닫기를 책임지고, Esc 처리는 "열린 창 목록의 맨 위를 닫기"로 단순화.

4. **세이브/로드 슬롯**
   - 슬롯별 지역명·저장 종류·날짜 표시, 최신순 정렬, 언어 전환 시 다시 표시.
   - 방향: 슬롯 정보(지역·시간 등)는 SaveManager에서 받아 표시만. 지역명 표는 데이터로 분리.

5. **설정 (사운드·키·마우스)**
   - 슬라이더/버튼으로 값 바꾸고 SaveManager에 저장, 다시 열면 불러와 반영.
   - 방향: 입력이 곧 화면 일이므로 UI가 직접 처리하되, 저장은 SaveManager로 단방향.

6. **연출 (페이드·로딩·사망·보스 처치)**
   - 화면 어둡게/밝게, 로딩 바, 사망·보스 처치 화면, 전투 HUD 잠깐 표시.
   - 방향: 게임플레이/씬 전환이 "연출 재생"만 요청하고 실행은 연출 담당이 맡음. 페이드 코루틴은 한 곳으로 합침.

### 적용 범위

`Scripts-Origin` 안에서 각 UI를 누가 부르는지 추적한 결과(텍스트 검색 기반, 같은 이름·이벤트·상속 호출은 놓칠 수 있어 일부 `(확인 필요)`):

| UI | 접근 방식 | 부르는 곳 |
| --- | --- | --- |
| MenuUI | 싱글톤 + SerializeField | PlatformSwitcher(SetPCPlatform), SceneSwitcher·TimelineHelper(ActivateLetterBox, MainMenu), Player(DisablePlayerControl), MobileInput(MenuSwitch). 참조 보관: CameraController·SaveManager·PlayerController |
| HpHUD | 싱글톤 + public 필드 | SceneSwitcher·SaveManager·GhostMaskSkill(ChangeHPStack), Player(필드 보관) |
| LoadingUI | 싱글톤 + SerializeField | SceneSwitcher(FadeOut/In), FadeForSwitchSceneTrigger(FadeOutIn), Player·MenuUI(참조 보관) |
| UIEffect | 싱글톤 + SerializeField | Player(ShowFadeScreen·ShowDeathScreen·ShowPlayerHUDFadeEffect), CameraController·PlayerController·Human/AnimalMaskSkill(HUD 페이드), WisuMainRe(보스 처치 화면), TimelineHelper(Fade) |
| SkillHUD | SerializeField 주입 | Human/Animal/GhostMaskSkill, PlayerMaskChange |
| SoundSettingUI / InputKeySettingUI / MouseSettingUI | SerializeField 주입 | MenuUI(저장·편집완료 호출), TimelineHelper(MouseSettingUI 참조) |
| PlayerHUD | 직접 참조 | `UpdateMask` 호출처 미발견 `(확인 필요)` |
| MinimapHUD | 외부 호출 없음 | 내부 `FixedUpdate`에서 `PlayerController.instance` 폴링 |
| TargetIndicator | 외부 호출 없음 | 내부 `Update`, target은 인스펙터 주입 |

- **공용으로 묶을 부분**: 페이드(LoadingUI·UIEffect 중복), 메뉴 위 확인 창 스택 처리.
- **대상별로 갈라야 할 부분**: HUD(상태 표시)·설정(입력 처리)·연출(코루틴)은 성격이 달라 한 클래스로 합치지 않는다. 특히 MenuUI는 메뉴·세이브·설정·레터박스·플랫폼이 한 곳에 섞여 있어 분리 대상.
- **적용 범위 한정**: 부르는 곳이 많은 MenuUI·UIEffect·HpHUD부터. 외부 호출이 없는 MinimapHUD·TargetIndicator는 결합이 약해 후순위.

### 예상 문제

1. **상태 표시를 이벤트로 뒤집을 때, 알림 누락**
   - HUD가 값을 받아 그리도록 바꾸면, 게임플레이가 상태 변화 때 알림 보내는 걸 빠뜨리면 화면이 안 바뀐다.
   - 방향: 체력·마스크·스킬처럼 "바뀌면 알린다"가 명확한 값만 우선 이벤트화하고, 한 번에 다 바꾸지 않는다.

2. **싱글톤 제거 시 호출처 광범위**
   - HpHUD·UIEffect·MenuUI는 전투·세이브·씬 전환·보스 등 여러 곳에서 직접 부른다. 한꺼번에 없애면 그 모든 곳이 깨진다.
   - 방향: 싱글톤 제거는 마지막 단계. 먼저 책임 분리·이벤트화부터 하고, 호출처가 줄어든 뒤 정리.

3. **모바일 게임플레이 입력의 소속 혼동**
   - 모바일 공격·대시·메뉴 버튼은 UI 위젯이지만 게임플레이 입력이다. UI에 그 실행을 넣으면 PC 입력과 경로가 갈라진다.
   - 방향: 게임플레이성 입력은 InputSystem으로 보내고(키보드와 같은 경로), UI는 버튼과 눌림 피드백만. `MobileInput`이 게임 객체를 직접 조작하던 코드는 InputSystem으로 흡수.
   - (검토 후 제외: UI가 직접 스킬을 호출하는 현행 방식 유지 — 입력 경로가 둘로 갈려 유지보수가 어려워 제외.)

4. **페이드 통합 시 미묘한 차이**
   - LoadingUI와 UIEffect의 페이드는 비슷하지만, UIEffect는 진행 중 소리를 멈추고 씬 로드 대기 같은 부가 동작이 섞여 있다.
   - 방향: 순수 페이드(투명도 0↔1)만 공통으로 빼고, 소리·씬 대기 같은 부가 동작은 부르는 쪽에 남긴다.

5. **세이브 슬롯 텍스트가 코드에 박힘**
   - 지역명·저장 종류가 한국어/영어 switch로 코드에 직접 들어가 있어, 지역이 늘면 코드를 고쳐야 한다.
   - 방향: 지역명 같은 표시 문자열은 데이터(예: ScriptableObject)로 분리해 코드에서 떼어낸다.

<br>
<br>
<br>

## 기능 흐름 (이렇게 흐르게 하면 된다)

- **상태 → 화면 (출력)**: 게임플레이(Player·스킬)가 상태가 바뀔 때 **알림(event)** 을 보낸다 → HUD가 그걸 **구독**해 자기 화면만 갱신한다. HUD는 게임플레이를 모른다. (지금은 반대로 게임플레이가 `HpHUD.instance.ChangeHPStack()`을 직접 부름 → 방향을 뒤집음)
- **버튼 → 동작 (입력)**: 입력 성격에 따라 세 갈래로 흐른다.
  - 게임플레이 입력(모바일 공격·대시·메뉴 열기) → **InputSystem** 으로 보냄(PC 키보드와 합류). UI는 버튼·피드백만.
  - 컷씬 스킵 → **Timeline** 에 "스킵" 신호만 전달.
  - 설정·세이브 슬롯 → **UI가 직접 처리**(입력이 곧 화면 일). 결과 저장은 **SaveManager** 로 단방향.
- **연출 (페이드·로딩·사망·보스)**: 씬 전환·전투·보스가 "이 연출 재생해줘"만 **요청** → 연출 담당이 코루틴 실행. 공통 페이드는 한 곳에서.
- **메뉴 내비게이션**: 창을 열면 **열린 창 목록**에 쌓고, Esc는 "맨 위 창 닫기"만 한다. 닫을 때 할 일(설정 저장 등)은 각 창이 스스로 한다.

> 설계(구체적 클래스 분해·인터페이스 정의)는 이 문서 범위 밖. 위 흐름 방향만 확정한다.

<br>

## 예전 MVP / Popup 방식 평가 (냉정·중립)

### MVP_Old (`Scripts-Refactoring-Old/UI/MVP`)
사운드 슬라이더 1개를 검증하려 만든 프로토타입. `BaseDataSO`(리플렉션으로 프로퍼티명↔enum 매핑) + `MVP_Model`(SO) + `View`/`Presenter` + `IViewUpdatable`/`IModelUpdatable` + `object` 박싱 Dictionary 구조.

| 항목 | 평가 |
| --- | --- |
| 의도 | 데이터-뷰 자동 양방향 바인딩(범용 틀) |
| 실제 | 슬라이더 1개 연결에 클래스 6개 + 리플렉션 + enum + object 박싱 |
| 상태 | 미완성. `Debug.Log(1)`·`Debug.Log(2)` 디버그 코드 잔존, Update에서 매 프레임 텍스트 갱신 |
| 비용 | 새 항목마다 enum 추가 + 양쪽 Dictionary 등록 + 박싱/캐스팅. 컴파일 단계 타입 검사 없음(`(float)data` 런타임 캐스팅) |

**결론: 폐기 권장.** 과설계다. 이 규모(화면 11개, 각 화면 로직이 제각각)에서는 슬라이더 `onValueChanged` + C# `event` 직접 구독으로 충분하다. 범용 바인딩 틀은 같은 패턴 화면이 수십 개로 반복될 때나 값을 한다. "강화"는 비용만 키운다.

### Popup_Old (`Scripts-Refactoring-Old/UI/UI_Popup`)
| 파일 | 상태 |
| --- | --- |
| `IPopup` | `Open()`/`Close()`/컨트롤러 주입 — 인터페이스 선언만 |
| `PopupController` | 빈 `Start`/`Update` + 정체불명 `GameObject ww`. 사실상 스텁 |
| `R_UIManager` | 완전히 빈 `Start`/`Update`. 내용 없음 |

**결론: 코드는 버리고 개념만 채택.** 살릴 실제 코드가 없다. 다만 "여러 확인 창을 쌓아 두고 Esc로 맨 위부터 닫는다"는 개념은 유효하며, 이는 위 **기능 흐름의 "메뉴 내비게이션"** 으로 흡수한다. `IPopup` 같은 거창한 인터페이스 없이 "열린 창 목록 + 맨 위 닫기"면 충분하다.
