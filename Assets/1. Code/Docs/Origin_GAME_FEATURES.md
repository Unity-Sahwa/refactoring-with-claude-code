# 게임 기능 목록

> 작성일: 2026-03-31 / 수정일: 2026-04-06
> 출처: Assets/Scripts/ 코드 분석 (Enemy/Gimmick/Timeline 제외)
> 목적: 리팩토링 재구현 시 기능 누락 방지용 참고 문서

---

## 1. 입력 시스템

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| PC 키 입력 | `Input.GetKey` / `GetKeyDown`으로 이동·공격·스킬·메뉴 처리 | `PlayerController` |
| 기본 키 바인딩 | W/S/A/D 이동, LeftShift 락온, Mouse0 공격, Q 스킬, F 처치기, Mouse1 대쉬, Esc 메뉴 | `PlayerController`, `SaveManager` |
| 모바일 입력 | 조이스틱 이동 + 각 버튼이 함수를 직접 호출 | `MobileInput`, `VariableJoystick` |
| 키 리바인딩 | 런타임에 키 변경, CSV 파일에 저장·로드 | `InputKeySettingUI`, `SaveManager` |
| 입력 스무딩 | 방향키 입력값을 부드럽게 보간, 반대 방향 누르면 즉시 리셋 | `PlayerMovement` |
| 입력 버퍼링 | 스킬 시전 중 다음 스킬 입력을 저장해두다가 타이밍에 맞춰 실행 | `PlayerSkillInput` |

---

## 2. 플레이어 이동

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 이동 | `Rigidbody.MovePosition`으로 이동, 카메라 방향 기준으로 벡터 회전 | `PlayerMovement`, `MaskChange`, `CameraController` |
| 회전 | 일반 이동 시 `RotateTowards`, 락온 시 `Slerp`로 타겟 방향 바라봄 | `PlayerMovement`, `CameraController` |
| 추가 중력 | 유니티 기본 중력 외에 `AddForce`로 중력 추가 적용 | `PlayerMovement` |
| 벽 감지 | 전방 레이캐스트로 장애물 감지 시 이동 속도 0으로 차단 | `PlayerSensor`, `PlayerMovement` |
| 마스크별 이동 속도 | Human / Animal 각각 ScriptableObject에서 `moveSpeed` 읽어옴 | `PlayerMovement`, `PlayerHumanMaskData`, `PlayerAnimalMaskData` |
| 모바일 리센터링 | 조이스틱 수평값 ±0.5 초과 시 카메라 자동 리센터링 | `PlayerMovement`, `CameraController` |

---

## 3. 마스크 시스템

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 캐릭터 전환 | Human ↔ Animal 오브젝트를 켜고 끄고, 위치·회전 복사 | `MaskChange` |
| 전환 시 스킬 초기화 | 전환할 때 상대방 마스크의 진행 중인 스킬 초기화 | `MaskChange`, `HumanMaskSkill`, `AnimalMaskSkill` |
| 전환 쿨다운 | 전환 직후 일정 시간 재전환 불가 | `MaskChange`, `PlayerCommonData` |
| 마스크 비주얼 전환 | 캐릭터 오브젝트와 별개로 마스크 장착 메시 전환 (Ghost는 처치기 중만 표시) | `MaskChange` |
| 파트너 오브젝트 | 마스크 전환 시 파트너가 플레이어와 충돌하는 연출 | `MaskChange`, `OrbitPartnerMask` |
| 전환 이펙트·사운드 | 방사형 이펙트 + 전환 사운드 재생 | `MaskChange`, `PlayerEffect`, `PlayerSound` |

---

## 4. 스킬 / 전투

### Human 마스크

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 기본 공격 3콤보 | 1타→2타→3타 순서, 이동 중 리셋, 2~3타 슈퍼아머 | `HumanMaskSkill`, `PlayerSkillInput`, `PlayerHitBox`, `PlayerHitBoxCollider` |
| 잉크 형상 (InkShape) | Q키, 쿨다운 있음, 별도 히트박스와 이펙트 | `HumanMaskSkill`, `PlayerEffect`, `PlayerSound`, `PlayerCameraEffect`, `GameTimeScale` |
| 잉크 바닥 (InkFloor) | 코드는 구현됨, **현재 입력 주석 처리로 실제 사용 불가** | `HumanMaskSkill` |
| 대쉬 | Mouse1, 쿨다운 있음 | `HumanMaskSkill`, `PlayerSkillMove` |

### Animal 마스크

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 기본 공격 3콤보 | Human과 동일 구조, 타격 타이밍에 무기 메시 활성화 | `AnimalMaskSkill`, `PlayerSkillInput`, `PlayerHitBox`, `PlayerHitBoxCollider` |
| 도약 강타 (LeapStrike) | Q키, 전진 이동 + 트레일 이펙트, 쿨다운 | `AnimalMaskSkill`, `PlayerSkillMove`, `PlayerEffect`, `PlayerSound` |
| 포효 (Roar) | 코드는 구현됨, **현재 입력 주석 처리로 실제 사용 불가** | `AnimalMaskSkill` |
| 대쉬 | Human과 같은 입력, 별도 애니메이션 | `AnimalMaskSkill`, `PlayerSkillMove` |

### Ghost (처치기)

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 처치기 조건 감지 | 범위 내 적 중 먹물 최대 스택 이상 + 시야각 내 타겟 탐색 | `GhostMaskSkill`, `CalliSystem`, `CameraController` |
| 처치기 실행 | Ghost 비주얼 → 전용 카메라 → 슬로우모션 → 타겟 즉사 → HP +2 → 복귀 | `GhostMaskSkill`, `MaskChange`, `CameraController`, `GameTimeScale`, `PlayerSkillMove`, `PlayerEffect`, `PlayerSound`, `PlayerCameraEffect` |
| 처치기 쿨다운 | `ghostData.cooldown` 기반, SkillHUD에 쿨다운 표시 | `GhostMaskSkill`, `SkillHUD` |

### 공통

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 칼리그래피 시스템 | 적에게 색상별 먹물 스택 누적, 처치기 발동 조건 연동 | `CalliSystem`, `PlayerHitBoxCollider` |
| 히트박스 On/Off | waitTime·duration 기반 코루틴으로 히트박스 활성화·비활성화 | `PlayerHitBox`, `PlayerHitBoxCollider` |
| 피격 반응 | 스킬 초기화 + 히트 애니메이션 + 피격 쿨다운 | `Player`, `PlayerState`, `PlayerAnimation`, `PlayerSound` |
| 슈퍼아머 | 2~3콤보 중 피격 시 히트 모션 생략 | `PlayerState`, `HumanMaskSkill`, `AnimalMaskSkill` |
| 무적 | 처치기 실행 중 데미지 무시 | `PlayerState`, `GhostMaskSkill` |
| 스킬 중 강제 이동 | waitTime·duration·speed 구조체 기반으로 스킬 도중 캐릭터 강제 이동 | `PlayerSkillMove` |
| 카메라 흔들기 | 스킬 타격 시 CinemachineImpulse + Noise로 흔들림 | `PlayerCameraEffect`, `CameraController` |
| 애니메이션 속도 제어 | 스킬별 normalizedTime 구간마다 다른 속도 적용 | `PlayerAnimation` |

---

## 5. 플레이어 상태

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 상태 관리 | IDLE·WALK·스킬·HIT·DEAD 등 현재 상태 추적, 스킬 수행 중 여부 판단 | `PlayerState` |
| 행동 제한 | `doNotAct` / `doNotMove` / `doNotRotate` — 스킬별 시간 기반으로 입력 차단 | `PlayerState`, `HumanMaskSkill`, `AnimalMaskSkill`, `GhostMaskSkill` |
| HP 관리 | 최대 20, 정수 단위, 피격 시 감소 / 처치기 성공 시 +2 회복 | `Player`, `HpHUD` |
| 낙사 처리 | HP 3 이상이면 이전 세이브 지점 부활 + HP -3, 미만이면 HP 0 처리 | `PlayerDamageReaction`, `Player`, `SaveManager`, `UIEffect` |
| HP 0 사망 | 사망 애니메이션 → 페이드 → 사망 화면 → 씬 0(메인메뉴) 이동 | `Player`, `PlayerAnimation`, `PlayerSound`, `UIEffect`, `MenuUI` |

---

## 6. 저장 시스템

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 저장·로드 | CSV 텍스트 파일로 4개 슬롯 관리 (`Application.persistentDataPath`) | `SaveManager` |
| 저장 항목 | 씬번호, 구역번호, 마스크 종류, 캐릭터 위치, HP, 조명 오브젝트 상태, PostProcess 상태 | `SaveManager` |
| 자동 저장 | 씬 전환 시 / 낙사 후 부활 시 자동 저장 | `SaveManager`, `SceneSwitcher`, `Player` |
| 슬롯 정렬 | 씬번호×100 + 구역번호 내림차순 (진행도 높은 순) | `SaveManager` |
| ScriptableObject 초기화 | 로드 후 SO 데이터를 저장값으로 덮어씀 | `SetSOData`, `SetPlayerData` |
| 설정 파일 별도 저장 | 사운드 볼륨 / 키 바인딩 / 마우스 감도 각각 별도 파일 | `SaveManager`, `SoundSettingUI`, `InputKeySettingUI`, `MouseSettingUI` |

---

## 7. 씬 / 스테이지

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 씬 구성 | Area_0_Tutorial ~ Area_4_FirstBoss (빌드 인덱스 0~4), 구역 15개 | `SceneSwitcher`, `SaveManager` |
| 씬 전환 | 저장 후 페이드 없이 씬 로드, HP·마스크 상태 유지 | `SceneSwitcher`, `SaveManager`, `LoadingUI` |
| 슬롯 로드 | 페이드 아웃 → 씬 로드 → 데이터 복원 → 페이드 인 | `SaveManager`, `SetSOData`, `SetPlayerData`, `LoadingUI`, `UIEffect` |
| 보스방 카메라 | buildIndex == 4일 때 카메라 궤도 설정값 자동 변경 | `CameraController`, `SetBossRoomCamera` |
| 튜토리얼 타임라인 연출 | 씬 0에서 새 게임 시작 시 타임라인 재생, 플레이어 제어 비활성화·페이드 처리, 스킵 가능 | `TimelineHelper`, `MenuUI`, `MaskChange`, `UIEffect`, `PlatformSwitcher` |
| 에셋 프리로드 | 씬 시작 시 오디오·이펙트·환경·적 오브젝트를 잠깐 활성화해 메모리에 미리 로드 | `Preloader` |

---

## 8. 카메라

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 기본 카메라 | CinemachineFreeLook, PC 마우스 / 모바일 조이스틱 수평값으로 X축 조작 | `CameraController` |
| 락온 카메라 | OverlapSphere로 적 탐지, 가장 가까운 적 자동 선택, VirtualCamera + GroupComposer | `CameraController` |
| 타겟 자동 전환 | 락온 타겟 사망 시 주변 적으로 자동 전환, 없으면 해제 | `CameraController` |
| 처치기 카메라 | 처치기 시 전용 VirtualCamera + Animator로 Human/Animal 버전 구분 | `CameraController`, `GhostMaskSkill` |
| 타겟 마커 | 감지·락온 적 위에 WorldToScreenPoint로 UI 마커 표시, 색상 구분 | `CameraController`, `CameraData` |
| 아웃라인 | 감지 적에 outline 머티리얼 추가, 적 변경 시 이전 제거 | `CameraController` |
| 해상도 자동 조정 | Android 720p / PC 1080p 기준으로 화면비 유지하며 자동 계산 | `CameraController` |
| 카메라 값 고정 | 전환 직후 2초간 카메라 offset·Y축 값 강제 유지 코루틴 | `CameraController` |

---

## 9. UI

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| HP HUD | HP 스택 이미지 배열 On/Off + "현재/최대" 텍스트 | `HpHUD`, `Player` |
| 스킬 HUD | Human/Animal 아이콘 전환, fillAmount로 쿨다운 시각화 | `SkillHUD`, `HumanMaskSkill`, `AnimalMaskSkill`, `GhostMaskSkill` |
| 플레이어 HUD | 현재 마스크 아이콘 표시 | `PlayerHUD` |
| 타겟 방향 표시 | 타겟이 화면 밖이면 가장자리에 깜빡이는 방향 표시 | `TargetIndicator` |
| HUD 페이드 | 스킬 사용 시 HUD 페이드인 연출 | `UIEffect` |
| 메인메뉴 | 새 게임·불러오기·설정·플랫폼 전환·언어 전환·종료 | `MenuUI`, `SaveManager`, `LanguageManager` |
| 일시정지 | TimeScale 0, 오디오 정지, 설정창 접근 가능 | `MenuUI`, `GameTimeScale`, `PlayerSound` |
| 로딩 UI | 프로그레스바 + 퍼센트 + 페이드 인/아웃 | `LoadingUI` |
| 레터박스 | 메뉴 열릴 때 상하 마스크 활성화 | `MenuUI` |
| 사망 화면 | HP 0 사망 시 페이드 후 사망 UI 표시 | `UIEffect`, `Player` |

---

## 10. 오디오

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| AudioSource 풀 | 10개 AudioSource 미리 생성, 비어있는 슬롯 찾아 재생 | `PlayerSound` |
| 사운드 구조체 | waitTime·pitch·volume·loop·spatialBlend 등 한 번에 설정 (`SoundStruct`) | `PlayerSound` |
| 루프 중복 방지 | 같은 상태에서 같은 루프 사운드 중복 재생 차단 | `PlayerSound` |
| AudioMixer 3채널 | 적 SFX / 플레이어 SFX / 환경(BGM+환경SFX) | `PlayerSound`, `SoundSettingUI` |
| 볼륨 4채널 조절 | 마스터·BGM·적SFX·플레이어SFX 슬라이더 독립 조절 | `SoundSettingUI`, `SaveManager` |
| 일시정지 연동 | 포즈 시 전체 AudioSource Pause / 복귀 시 UnPause | `PlayerSound`, `MenuUI` |

---

## 11. 플랫폼

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 빌드 분기 | `#if UNITY_ANDROID`로 기본값 결정 | `CameraController`, `PlayerMovement` |
| 런타임 플랫폼 전환 | 메인메뉴에서 PC ↔ 모바일 전환, DontDestroyOnLoad로 씬 전환 후에도 유지 | `PlatformSwitcher`, `MenuUI` |
| PC 모드 전환 항목 | 카메라 마우스 축 활성화 / PC 키 가이드 표시 / 커서 잠금·숨김 | `PlatformSwitcher`, `CameraController`, `MobileInput` |

---

## 12. 기타

| 기능 | 설명 | 관련 클래스 |
|------|------|------------|
| 타임스케일 | 처치기 슬로우모션 (프레임/초 단위 선택), 포즈 시 0 | `GameTimeScale` |
| 카메라 진동 | CinemachineImpulse 충격 + Noise 흔들림 + Recomposer 줌 보간 | `PlayerCameraEffect` |
| 다국어 | Unity Localization 패키지, 한국어/영어 전환 | `LanguageManager`, `TextManager` |
| 이벤트 시스템 | `IEvent` 인터페이스 + `EventData` 기반, 현재 낙사 처리에만 사용 | `EventData`, `IEvent`, `PlayerDamageReaction` |
| 코루틴 유지 | 씬 전환 간 코루틴 끊기지 않도록 전역 매니저 사용 | `CoroutineManager` |
| 튜토리얼 | 처치기 조건 달성 시 씬 0에서 튜토리얼 UI 표시 | `CameraController`, `PlayGuide` |
| 지도 텔레포트 | 지도 UI에서 버튼 누르면 지정된 포인터 위치로 순간이동 | `PlayerTeleporter`, `MaskChange` |
| 메시 합치기 | 여러 오브젝트의 메시를 머티리얼별로 합쳐 드로우콜 감소 (씬 시작 시 자동 실행) | `MeshCombiner` |

---

## ⚠️ 파악이 덜 된 기능

| 기능 / 클래스 | 상태 | 이유 |
|------|------|------|
| InkFloor (잉크 바닥) | 코드 있음, **실제 작동 안 함** | `PlayerController`에서 입력 코드가 주석 처리됨 |
| Roar (포효) | 코드 있음, **실제 작동 안 함** | `PlayerController`에서 입력 코드가 주석 처리됨 |
| `EffectTimer` | **전체 주석 처리, 작동 안 함** | InkFloor/InkShape 히트박스 타이머 구버전 코드, 현재 미사용 |
| `Visualize` | **게임에서 사용 안 함** | 에디터 전용 벡터 연산 시각화 디버그 도구 |
| `ObjectLinker` | **빈 껍데기, 작동 안 함** | 메서드 선언만 있고 내부 구현이 전혀 없음 |
| `PlayerWaypoints` | **PlayerTeleporter와 동일한 코드** | savePointer 필드만 추가된 중복 클래스, 어느 쪽이 실제 사용되는지 불명확 |
| `OrbitPartnerMask` | 파악 부족 | 파트너 오브젝트 충돌 연출 방식 미분석 |
| `CalliSystem` | 파악 부족 | `Painting()`, `IsPaintOverMax()` 메서드 존재만 확인, 내부 색상별 스택 누적 방식 미분석 |
| `PlatformSwitcher` | 파악 부족 | 전환 시 변경 항목 목록만 파악, 내부 구현 미분석 |
| `PlayGuide` | 파악 부족 | 튜토리얼 UI 표시 트리거만 확인, 전체 흐름 미분석 |
| `TargetIndicator` | 파악 부족 | 화면 밖 타겟 방향 표시 기능만 파악, 내부 구현 미분석 |
| Enemy 시스템 전체 | 분석 제외 | 리팩토링 범위 외 |
| Gimmick 시스템 전체 | 분석 제외 | 리팩토링 범위 외 |
