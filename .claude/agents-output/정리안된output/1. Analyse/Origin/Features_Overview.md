# 전체 기능 목록 (Origin 코드 기반)

> 분석날짜: 2026-04-13  
> 대상: `Assets/1. Code/Docs/1. Analyse/Origin/` 내 34개 분석 파일에서 추출

---

## 목차

1. [플레이어 코어](#1-플레이어-코어)
2. [이동 및 물리](#2-이동-및-물리)
3. [상태 관리](#3-상태-관리)
4. [애니메이션 및 사운드](#4-애니메이션-및-사운드)
5. [스킬 시스템](#5-스킬-시스템)
6. [마스크 시스템](#6-마스크-시스템)
7. [히트박스 및 충돌](#7-히트박스-및-충돌)
8. [카메라 시스템](#8-카메라-시스템)
9. [입력 시스템](#9-입력-시스템)
10. [HUD 및 UI](#10-hud-및-ui)
11. [씬 전환 및 게임 흐름](#11-씬-전환-및-게임-흐름)
12. [설정 UI](#12-설정-ui)

---

## 1. 플레이어 코어

### Player
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 플레이어 초기 설정 (SetUp) | maxHP 로드, isDead/isInvincible 초기화, HpHUD 업데이트 |
| 데이터 로드 (Loading) | SaveManager에서 체력 데이터 로드 후 HpHUD 업데이트 |
| 손상 적용 (ApplyDamage) | 무적/사망 확인 → HP 감소 → HP==0 시 DEAD 상태로 DieAction() 호출 |
| 피격 반응 코루틴 (HitAction) | 스킬 초기화, 리지드바디 속도 0, 피격 애니메이션/사운드 재생 |
| 죽음 처리 코루틴 (CoDieAction) | DEAD_FALL/DEAD_HPZERO 분기 처리, 죽음 애니메이션, 사망 화면, 씬 로드 |
| 체력 회복 (RestoreHealth) | HP를 maxHP로 복구, isDead=false, HpHUD 업데이트 |
| 사망 여부 확인 (CheckDie) | isDead 플래그 반환 |
| 피격 쿨타임 관리 (HitCooldown) | 마지막 피격 시간 비교, 쿨타임 만료 시 피격 가능 상태로 변경 |
| 마스크 오브젝트 위치 동기화 (FollowCharacterObject) | 매 FixedUpdate에서 CurrentMask의 position/rotation을 자신에게 적용 |

### PlayerDamageReaction
| 기능 | 설명 |
|------|------|
| 사망 상태 전환 (Execute) | PlayerState를 DEAD/DEAD_FALL로 변경 |
| 사망 애니메이션 실행 | Player.CoDieAction() 코루틴 시작 |

---

## 2. 이동 및 물리

### PlayerMovement
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 입력값 가속화 처리 (InputMovementValue) | SignType 배열로 방향 추적, inputIncreaseSpeed로 가속/감속, Clamp(-1,1) |
| 플랫폼별 이동 입력 처리 (InputMovement) | PC/모바일 입력 획득, moveAmount 계산, 카메라 방향 기반 월드좌표 변환, 애니메이터 업데이트 |
| 캐릭터 회전 제어 (CharacterRotate) | 락온 유무에 따라 Slerp/RotateTowards로 부드러운 회전 |
| 캐릭터 위치 이동 (CharacterMove) | 마스크별 이동속도, 치트 모드 속도 배율, PlayerSensor 기반 이동 가능 여부 확인, MovePosition |
| 중력 물리 효과 (AddGravity) | currentVelocity.y 감소, 최대 낙하 속도 제한, 지면 접촉 시 초기화 |

### PlayerSkillMove
| 기능 | 설명 |
|------|------|
| 스킬 이동 상태 초기화 (Initialize) | 코루틴 중지 플래그 설정, 이동 플래그/속도 초기화 |
| 스킬 이동 코루틴 (SkillMove) | waitTime 후 방향별 이동 플래그 ON, duration 후 OFF |
| 전방 레이캐스트 충돌 감지 (RayCastForward) | 6개 레이캐스트, 적 레이어 충돌 시 전방 이동 차단 |
| 스킬 이동 실제 적용 (UpdateSkillMovement) | 방향 플래그 기반 벡터 계산, 센서 체크, MovePosition |
| 스킬 이동 방향 전환 (MoveSwitch) | FRONT/BACK/UP/DOWN 방향 플래그 on/off |
| 원본 높이 값 반환 (GetOriginHeight) | 스킬 높이 기반 이동 제어에 사용 |

### PlayerSensor
| 기능 | 설명 |
|------|------|
| 전방 장애물 감지 (DetectFrontCollider) | Physics.Raycast로 전방 감지, 충돌 시 canNotMoveforward=true |
| 후방 장애물 감지 (DetectBackCollider) | Physics.Raycast로 후방 감지, 충돌 시 canNotMoveBackward=true |

---

## 3. 상태 관리

### PlayerState
| 기능 | 설명 |
|------|------|
| 플레이어 상태 초기화 (Start/Initialize) | 모든 플래그 및 서브상태 초기화, 코루틴 중지 신호 설정 |
| 메인 상태 변경 (ChangePlayerState) | playerCurrentState 갱신 후 IsPerformingSKill() 자동 판정 |
| 서브 상태 변경 (ChangePlayerSubState) | playerCurrentSubState 갱신 후 IsPerformingSKill() 자동 판정 |
| 스킬 수행 여부 판정 (IsPerformingSKill) | NONE/IDLE/WALK/HIT/DEAD → false, 그 외 → true |
| 슈퍼아머 토글 (ToggleSuperArmorState) | isSuperArmor 설정으로 피격 넉백 저항 제어 |
| 무적 토글 (ToggleInvincibleState) | isInvincible 설정으로 모든 피해 차단 제어 |
| 시간 기반 행동 제약 (RestrictPlayer - 구조체) | Act/Move/Rotate 각각 waitTime + duration 기반 자동 on/off |
| 즉시 행동 제약 (RestrictPlayer - 즉시) | ACT/MOVE/ROTATE 타입별 즉시 제약 on/off |

---

## 4. 애니메이션 및 사운드

### PlayerAnimation
| 기능 | 설명 |
|------|------|
| 애니메이션 초기화 | 마스크별 데이터 로드, 속도 파라미터 초기화 |
| 애니메이션 상태 업데이트 (UpdateAnimationState) | 현재 해시 판별, moveAmount로 WALK/IDLE 전환, 사운드 처리 |
| 애니메이션 구간별 속도 조정 (SetAnimationSpeed) | animationSpeedStruct 배열 기반 normalizedTime 구간 판정, Animator.speed 동적 설정 |

### PlayerSound
| 기능 | 설명 |
|------|------|
| 오디오 시스템 초기화 (StartSet) | AudioSource 배열 생성, AudioMixerGroup 설정, 10개 사운드 오브젝트 생성 |
| 사운드 큐 추가 (SetPlayerSound) | useFunction 확인, 루프 중복 방지, 빈 AudioSource 선택, 코루틴 재생 |
| 사운드 재생 시간 제어 코루틴 (TogglePlayerSound) | waitTime 도달 시 Play(), 재생 완료 시 종료 |
| 루프 사운드 중지 (StopLoopingAudio) | loop==true인 AudioSource 모두 중지 |
| 사운드 일시정지/재개 (TogglePlayingAudioPause) | 재생 중인 모든 AudioSource Pause/Play, audioPause 플래그 관리 |
| 사운드 상태 초기화 (Initialize) | 코루틴 중지 플래그 설정, StopLoopingAudio() 호출 |

---

## 5. 스킬 시스템

### PlayerSkill (추상 베이스 클래스)
| 기능 | 설명 |
|------|------|
| 일회성 초기화 확인 (StartSet) | activeStartOnce 플래그로 중복 초기화 방지 |
| 플레이어 관련 객체 초기화 | PlayerController, PlayerMovement, MaskChange, Player 참조 획득 |
| 카메라 및 UI 초기화 | CameraController, UIEffect 참조 획득 |
| 데이터 객체 초기화 | CommonData, HumanData, AnimalData, CameraData 참조 획득 |
| 게임오브젝트 활성화 타이밍 제어 (ControlObject) | waitTime 후 활성화, waitTime+duration 후 비활성화, 일회성 보장 |
| 이펙트 위치/회전 동기화 (SetEffectPosition) | effectObject를 지정된 Transform에 동기화 |

### PlayerSkillInput
| 기능 | 설명 |
|------|------|
| 입력 버퍼 초기화 (Initialize) | canStoreInputValue=false, 스택 초기화, 코루틴 중지 |
| 입력 저장 (StoreInput) | canStoreInputValue 확인 후 스택에 Push |
| 직접 입력 처리 (ProcessInputDirectly) | 버퍼 없이 CoProcessInputDirectly() 코루틴 시작 |
| 버퍼 입력 처리 (ProcessInput) | CoProcessInput() 코루틴 시작 |
| 입력 저장/실행 시간 윈도우 (CoProcessInput) | storeWaitTime~storeDuration 동안 저장 허용, executeWaitTime~executeDuration 동안 실행 |
| 입력 실행 (ExecuteStoredInput) | HUMAN_NORMALATTACK/HUMAN_INKSHAPE/DASH 등 타입별 스킬 실행 |
| 직접 입력 실행 (ExecuteStoredInputDirectly) | Pop() 후 즉시 해당 스킬 메서드 실행 |

### HumanMaskSkill
| 기능 | 설명 |
|------|------|
| 전체 스킬 초기화 (InitializeSkill) | 코루틴/상태/이동/이펙트/상태/사운드/히트박스/입력/타임스케일 모두 초기화 |
| 스킬 사용 공통 처리 (UseSkill) | canUse 확인, InitializeSkill(), 코루틴 시작, UIEffect 페이드 |
| 1단계 공격 (CoFirstAttack) | 애니메이션, 상태 변경, 이동, 이펙트, 히트박스, 사운드, 카메라 쉐이크, 입력 버퍼 |
| 2단계 공격 (CoSecondAttack) | SuperArmor 활성화 포함, 1단계와 동일 구조 |
| 3단계 공격 (CoThirdAttack) | SuperArmor 활성화, GameTimeScale 조정 추가 |
| InkShape 스킬 (CoInkShape) | 회전 공격, InvokeRepeating으로 히트박스 반복 활성화, 슬로우 모션 |
| InkShape 히트박스 반복 (InkShapeHitBoxOn/Off) | 카운트 기반 히트박스 on/off 반복 |
| InkFloor 스킬 (CoInkFloor) | 투사체 스킬, 타겟 유효성 확인, 투사체 위치 동기화, 히트박스 반복 |
| InkFloor 히트박스 (InkFloorHitBoxOn1/2/Off1/2) | 2개 히트박스 독립 관리 |
| Dash 스킬 (CoDash) | 락온 유무에 따라 전방/후방 대시 분기, 이동/사운드 처리 |
| 각 스킬 쿨다운 관리 (InkShapeCooldown / InkFloorCooldown / DashCooldown) | 경과 시간 기반 flowTimeRate 계산, SkillHUD 업데이트, 쿨다운 완료 시 재사용 허가 |

### AnimalMaskSkill
| 기능 | 설명 |
|------|------|
| 전체 스킬 초기화 (InitializeSkill) | 코루틴/무기/히트박스/상태/이동/이펙트/사운드/입력/타임스케일 초기화 |
| 무기 메시 비활성화 (InitializeWeapon) | rightHandWeaponMesh, leftHandWeaponMesh enabled=false |
| 히트박스 비활성화 (InitializeHitBox) | leapStrikeHitBox, roarHitBox 비활성화 |
| 1~3단계 일반 공격 (CoFirstAttack ~ CoThirdAttack) | 순차 콤보 공격, SuperArmor 활성화 포함 |
| 도약 공격 (CoLeapStrike) | SuperArmor, 무기 활성화, 두 가지 이펙트/사운드, InvokeRepeating 히트박스, 슬로우 모션 |
| 도약 공격 쿨다운 (LeapStrikeCooldown) | flowTimeRate 계산, SkillHUD 업데이트 |
| 포효 스킬 (CoRoar) | SuperArmor, 3가지 이펙트(차지/방출/트레일), InvokeRepeating 히트박스, 슬로우 모션 |
| 포효 쿨다운 (roarCooldown) | flowTimeRate 계산, SkillHUD 업데이트 |
| 포효 히트박스 반복 (RoarHitBoxOn/Off) | 카운트 기반 히트박스 반복 |
| Dash 스킬 (CoDash) | 락온 유무에 따라 전방/후방 대시, HumanMaskSkill과 동일 구조 |
| 대시 쿨다운 (DashCooldown) | flowTimeRate 계산, SkillHUD 업데이트 |

### GhostMaskSkill
| 기능 | 설명 |
|------|------|
| 타겟 감지 및 HUD 활성화 (DetectTargetToFinish) | Physics.OverlapSphere로 범위 내 적 검색, 조건 필터링, SkillHUD.ActivateFinishHUD() 호출 |
| 피니시 가능 여부 확인 (CheckEnableFinish) | canUseFinishSkill 및 finishTargetList.Count 확인 |
| 피니시 스킬 입력 처리 (Finish) | CheckEnableFinish → InitializeSkill → CoFinish 시작 |
| 피니시 스킬 메인 코루틴 (CoFinish) | 무적 활성화, 카메라 전환, 적 NavMesh 정지, 마스크별 애니메이션, 무기 전환, 적 제거, 타임스케일 조정 |
| 스킬 카메라 애니메이션 (SkillCameraAnimation) | Animator.SetTrigger로 카메라 연출 시작 |
| 피니시 쿨다운 (FinishSkillCooldown) | 경과 시간 계산, SkillHUD 업데이트, 쿨다운 완료 시 재사용 허가 |
| 스킬 상태 초기화 (InitializeSkill) | 코루틴 중지, 상태 초기화, 카메라 DEFAULT로 변경 |

---

## 6. 마스크 시스템

### PlayerMaskChange
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 캐릭터 오브젝트 초기화 (InitialSetUp) | 자식에서 휴먼/동물 GameObject, Animator, Rigidbody 캐싱, 기본 마스크(HUMAN) 적용 |
| 캐릭터 오브젝트 변경 (ChangeCharacter) | 위치/회전 동기화, CurrentMask 갱신, 스킬 Initialize(), 이전/새 오브젝트 활성화 교체 |
| 마스크 오브젝트 전환 및 효과 (ChangeMask) | SkillHUD 아이콘 변경, 해당 마스크만 활성화, 이펙트/사운드 재생 |
| 마스크 변경 쿨다운 관리 (ChangeMaskCooldown) | 경과 시간 확인, 쿨다운 완료 시 canUseChangeMask=true |

---

## 7. 히트박스 및 충돌

### PlayerHitBox
| 기능 | 설명 |
|------|------|
| 코루틴 중지 플래그 설정 (Initialize) | stopHitboxCoroutine=true |
| 히트박스 타이밍 제어 (TogglePlayerHitBox) | waitTime 후 활성화, waitTime+duration 후 비활성화, 외부 중지 신호 처리, 디버그 메시 표시 |
| 히트박스 타입별 선택 (SelectHitBox) | HitBoxType enum으로 human/animal 공격별 히트박스 배열 인덱스 매핑 |

### PlayerHitBoxCollider
| 기능 | 설명 |
|------|------|
| 컴포넌트 활성화 시 초기화 (OnEnable) | 데이터 로드, 일회성 플래그 초기화 |
| 피해 메시지 설정 (SetMessage) | 태그별 피해량/색상/스택 정보로 DamageMessage 구성 |
| 공격 충돌 처리 (OnCollisionEnter) | Enemy 확인, SetMessage, 타임스케일/카메라쉐이크/사운드/이펙트 실행, Enemy.ApplyDamage(), CalliSystem 연동 |

---

## 8. 카메라 시스템

### CameraController
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 해상도 조정 (AdjustResolution) | 플랫폼별(Android/PC) 목표 해상도 계산 후 Screen.SetResolution 적용 |
| 카메라 시스템 초기화 (CameraInitialSet) | 외부 참조 획득, Cinemachine 컴포넌트 캐싱, 타겟 마커 초기화, 마우스 속도 로드 |
| PC 플랫폼 입력 설정 (SetPCPlatform) | 카메라 X/Y 입력축명 설정 또는 비워서 모바일 전환 |
| 카메라 전환 (ChangeCamera) | DEFAULT/LOCKON/FINISHSKILL 카메라 활성화 전환, 코루틴으로 초기값 유지 |
| 기본 카메라 초기값 유지 (HoldDefaultCameraValue) | 보스씬 여부로 Orbit/Offset 분기, 2초 루프 모니터링 |
| 락온 카메라 초기값 유지 (HoldLockOnCameraValue) | 씬별 Follow/Tracked Offset 설정, 2초 루프 모니터링 |
| 마우스 속도 설정/조회 (SetMouseSpeed / GetMouseSpeed) | X/Y축 m_MaxSpeed 설정 및 반환 |
| 보스룸 카메라 설정 (SetBossRoomCamera) | 3개 Rig 궤도값 및 TrackedObjectOffset 직접 설정 |
| 타겟 감지 (DetectTargetAlways) | OverlapSphere 탐색, 각도/거리 기반 최적 타겟 선택 |
| 아웃라인 렌더링 (CheckOutlineTarget / DrawOutlineOnTarget / ClearOutline) | OutlineTarget 태그 자식 탐색, 아웃라인 머티리얼 추가/제거 |
| 현재 타겟 상태 확인 (CheckCurrentTargetState) | 유효성 검증, 타겟 자동 전환, 범위 이탈 시 DeactivateLockOn() |
| 락온 해제 (DeactivateLockOn) | targetGroup 제거, 애니메이션 플래그 해제, 카메라 DEFAULT 전환 |
| 타겟 마커 위치 제어 (ControlTargetMarker) | WorldToScreenPoint 변환, 색상 구분(락온/감지), 타겟 없을 시 숨김 |
| 락온 토글 (LockOnTarget) | 활성화 시 DeactivateLockOn, 비활성화 시 LOCKON 카메라 전환 |
| 조이스틱 수평 카메라 회전 (CameraHorizontalSwipe) | joystick.Horizontal 기반 m_XAxis.Value 조정 |
| 튜토리얼 UI 표시 (InvokeFinishGuide) | PlayGuide.ShowTutorialUI(FINISHATTACK) 호출 |

### PlayerCameraEffect
| 기능 | 설명 |
|------|------|
| 카메라 오브젝트 초기화 | 모든 카메라의 CinemachineRecomposer 기본값 설정 |
| 카메라 흔들림 (ShakeCamera) | CinemachineImpulse 기반, ShakeType/ReactionType별 분기, 진폭/주파수/지속시간 설정 |
| 카메라 리컴포저 일시 조정 (ToggleCameraRecomposer) | waitTime 후 ZoomScale/FollowAttachment/LookAtAttachment 조정, duration 후 복원 |

### TargetIndicator
| 기능 | 설명 |
|------|------|
| 타겟 화면좌표 변환 및 화면 내/밖 판정 | WorldToScreenPoint, z < 0 처리 |
| 화면 내 타겟 표시 | indicator.position = screenPosition |
| 화면 밖 타겟 가장자리 표시 | edgeOffset 기반 Clamp로 가장자리에 고정 |
| 타겟 깜빡임 효과 (blink) | Mathf.PingPong으로 알파값 반복 변환 |

---

## 9. 입력 시스템

### PlayerController
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 및 컴포넌트 획득 (Awake) | MaskChange/PlayerMovement/스킬 3종/Player/CommonData 참조 획득, MaskChange.InitialSetUp() 호출 |
| 매 프레임 입력 처리 (Update) | 메뉴/타임라인/상태 확인 → 애니메이션/쿨타임 갱신 → HUD 업데이트 → 이동/회전 입력 → 스킬 입력(PC/모바일 분기) |
| 물리 프레임 이동 처리 (FixedUpdate) | FollowCharacterObject, 중력, CharacterMove, 스킬 이동, 애니메이션 블렌드, 지면 판정, 피니시 쿨다운 |

### MobileInput
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 일반 공격 입력 (NormalAttack) | 마스크별 분기, StoreInput → 스킬 수행 중 아닐 때 실행 |
| 특수 공격 입력 (SpecialAttack) | 마스크별 분기(InkShape/LeapStrike), StoreInput → 실행 |
| 피니시 공격 입력 (FinishAttack) | StoreInput → GhostMaskSkill.Finish() |
| 대시 입력 (Dash) | StoreInput → 마스크별 Dash() |
| 락온 입력 (LockOnTarget) | CameraController.LockOnTarget() 호출 |
| 메뉴 열기 입력 (OpenMenu) | MenuUI.MenuSwitch() 호출 |
| 상호작용 플래그 관리 (EnableInteraction / DisableInteraction) | interact 플래그로 모든 UI 입력 차단/허용 |

### TimelineHelper
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 장면별 플레이어 상태 초기화 (Start) | 튜토리얼 씬 여부 판단, 플레이어 컨트롤 활성화 결정 |
| 튜토리얼 타임라인 시작 (CoStartTutorialTimeline) | 페이드 아웃/인, 플레이어 위치 설정, PlayableDirector 실행 |
| 튜토리얼 스킵 (CoSkipTutorialStartScene) | 페이드 효과, 타임라인 비활성화, 플레이어 컨트롤 활성화 |
| 타임라인 재생 상태 확인 (IsTimelinePlaying) | 모든 PlayableDirector 재생 상태 확인 |
| 플레이어 컨트롤 활성화/비활성화 (DisablePlayerControl) | MenuUI, PlayerSound, PlayerState, MouseSettingUI, Rigidbody, Cursor 통합 제어 |
| 화면 페이드인/아웃 (FadeInScreen / FadeOutScreen) | UIEffect 위임 |

### GameTimeScale
| 기능 | 설명 |
|------|------|
| 게임 시간 스케일 초기화 (Initialize) | 치트 모드 확인 후 Time.timeScale = 1 복원 |
| 타임스케일 코루틴 (CoSetTimeScale) | 치트/useFunction 확인, 프레임/초 단위 선택, 이전 값 저장, waitTime 대기, 변경, duration 대기, 복원 |
| 즉시 타임스케일 제어 (SetTimeScale) | Time.timeScale 즉시 설정 |

---

## 10. HUD 및 UI

### PlayerHUD
| 기능 | 설명 |
|------|------|
| 마스크 아이콘 업데이트 (UpdateMask) | maskIndex 유효성 확인, MaskHUD.sprite 교체 |

### HpHUD
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| HP 스택 표시 (ChangeHPStack) | currentHP 기준으로 hpStack 배열 활성/비활성화, hpText 텍스트 업데이트 |

### SkillHUD
| 기능 | 설명 |
|------|------|
| 피니시 HUD 활성/비활성화 (ActivateFinishHUD) | finishAttackImage SetActive |
| 스킬 쿨다운 진행률 표시 (SkillCooldown) | SetImage로 대상 이미지 선택, fillAmount로 진행률 표시 |
| 스킬 가이드 색상 변경 (ChangeGuideHUDColor) | SetImage 후 color 설정 |
| 상태별 이미지 매핑 (SetImage) | HUMAN_INKSHAPE/ANIMAL_LEAPSTRIKE/DASH/GHOST_FINISHSKILL → 이미지 반환 |
| 마스크별 특수공격 아이콘 전환 (ChangeIcon) | HUMAN/ANIMAL에 따라 이미지 부모 오브젝트 활성화 교체, 버튼 targetGraphic 갱신 |

### MinimapHUD
| 기능 | 설명 |
|------|------|
| 미니맵 카메라 추적 (ShowMinimap) | CurrentMask 위치로 minimapCamera X,Z 동기화, Y는 고정 높이 유지 |

### TargetIndicator
| 기능 | 설명 |
|------|------|
| 화면 밖 타겟 가장자리 인디케이터 | WorldToScreenPoint, 가장자리 Clamp, 깜빡임 효과 |

---

## 11. 씬 전환 및 게임 흐름

### UIEffect
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 및 데이터 로드 | Awake에서 UIEffectData 로드 |
| 화면 페이드 아웃 (FadeOutScreen) | ShowFadeInOutScreen(false) 코루틴 시작 |
| 화면 페이드 인 (FadeInScreen) | ShowFadeInOutScreen(true) 코루틴 시작 |
| 진행 중 페이드 교체 (ShowFadeScreen) | 기존 코루틴 중지 후 새 페이드 시작 |
| 페이드 효과 코루틴 (ShowFadeInOutScreen) | Lerp 기반 alpha 변화, unscaledDeltaTime 사용 |
| 플레이어 HUD 활성/비활성화 (ActivatePlayerHUD) | battleCanvasGroup SetActive |
| 플레이어 HUD 페이드 상태 설정 (IsPlayerHUDFading) | 유지 플래그 설정 |
| 플레이어 HUD 페이드 효과 (ShowPlayerHUDFadeEffect) | 중복 방지, 타임라인 재생 중 차단, ShowUIFadeEffect 코루틴 |
| UI 페이드 인→유지→아웃 (ShowUIFadeEffect) | 3단계 alpha 변화 코루틴 |
| 사망 화면 연출 (ShowDeathScreen) | deadImage 페이드 인, 10배속 알파값 증가 |
| 보스 격퇴 화면 및 씬 전환 (ShowBossDefeatedScreen) | 5초 대기 → 이미지 표시 → 페이드 → 3.5초 후 메인씬 전환 |

### LoadingUI
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| 로딩 코루틴 시작 (StartLoading) | Loading() 코루틴 시작 |
| 로딩 진행률 표시 (Loading) | progressBar.fillAmount Lerp, 텍스트 업데이트, 완료 시 화면 숨김 |
| 페이드 아웃/인 (FadeOutScreen / FadeInScreen) | CoFadeScreen 코루틴 위임 |
| 페이드 효과 코루틴 (CoFadeScreen) | unscaledDeltaTime 기반 alpha Lerp |
| 씬 전환 통합 페이드 (FadeOutInScreen / CoFadeOutInScreen) | 페이드아웃 → 씬 로드 대기 → 페이드인, SceneSwitcher 콜백 등록/해제 |

### MenuUI
| 기능 | 설명 |
|------|------|
| 싱글톤 초기화 | Awake에서 instance 설정 |
| UI 위치 설정 (SetPosition / SetRectTransform) | 화면 전체 채우도록 anchorMin/Max, offsetMin/Max 설정 |
| 버튼 리스너 등록 (SetButtonFunction) | 새 게임/로드/메인메뉴/종료/설정/언어 변경 버튼 콜백 등록 |
| 메뉴 열기/닫기 토글 (MenuSwitch) | 열기: timeScale=0, 일시정지 메뉴 활성화 / 닫기: timeScale 복구, 커서 상태 설정 |
| 플레이어 컨트롤 제어 (DisablePlayerControl) | isPlayerControlDisabled 플래그 설정 |
| 세이브 슬롯 기록 (RecordSlot) | 지역명/플레이타임/날짜 표시, timeIndex 저장 |
| 슬롯 정렬 (SortSlots) | 최근 저장 데이터 우선 정렬 |
| 게임 재시작 (Restart) | timeScale=1, 현재 씬 로드 |
| 스토리 이미지 창 표시 (ShowStory) | storyImageWindow 활성화 |
| 일시정지 메뉴 표시 제어 (CanShowPauseMenu) | canShowPauseMenu 플래그 설정 |
| 레터박스 제어 (ActivateLetterBox) | 화면 상하 검은 바 활성화/비활성화 |
| 세이브 슬롯 언어 변경 (ChangeSlotLanguage) | LanguageManager 기반 슬롯 텍스트 갱신 |
| 세이브 슬롯 초기화 (ResetSlot) | 슬롯 비활성화, timeIndex 제거 |
| 게임 종료 (Quit) | 설정 저장, timeScale=1, Application.Quit() |

---

## 12. 설정 UI

### MouseSettingUI
| 기능 | 설명 |
|------|------|
| 슬라이더 리스너 등록 (Awake) | X/Y축 슬라이더에 SetXAxisValue/SetYAxisValue 등록 |
| X/Y축 마우스 속도 설정 (SetXAxisValue / SetYAxisValue) | 정수 변환 → 텍스트 업데이트 → CameraController 속도 설정 |
| 마우스 설정 저장 (SaveMouseData) | CameraController에서 값 조회 → SaveManager에 저장 |
| 마우스 설정 로드 (LoadMouseData) | SaveManager에서 값 획득 → 슬라이더/텍스트/카메라 동기화 |

### SoundSettingUI
| 기능 | 설명 |
|------|------|
| 슬라이더 이벤트 등록 (Awake) | 마스터/BGM/적SFX/플레이어SFX 슬라이더에 리스너 등록 |
| 마스터 볼륨 설정 (SetMasterVolume) | 3개 AudioMixer의 "Master" 파라미터 동시 설정 |
| BGM/환경음 설정 (SetBGM) | envAudioMixer의 "BGM", "SFX" 파라미터 설정 |
| 적 SFX 설정 (SetEnemySFX) | enemyAudioMixer의 "SFX" 파라미터 설정 |
| 플레이어 SFX 설정 (SetPlayerSFX) | playerAudioMixer의 "SFX" 파라미터 설정 |
| 전체 음소거 (MuteAudio) | SetMasterVolume(0) 호출 |
| 음성 설정 저장 (SaveVolumeData) | 슬라이더 값 수집 → SaveManager에 저장 |
| 음성 설정 로드 (LoadVolumeData) | SaveManager에서 값 획득 → AudioMixer 및 슬라이더 동기화 |

### InputKeySettingUI
| 기능 | 설명 |
|------|------|
| 버튼 배열 초기화 및 이벤트 등록 (Awake) | KeyAction.KEYCOUNT 기반 배열 생성, 버튼별 EditInputKey(index) 등록 |
| 저장된 키 설정 로드 (Start) | 모든 KeyAction에 대해 ChangeInputKeyButtonText() 호출 |
| 키 편집 모드 진행 (Update) | isEditingKey일 때 DetectPressedKeyCode() → 키 저장 → 텍스트 갱신 → 편집 완료 |
| 편집 완료 플래그 설정 (CompleteEditingKey) | completeEditingKey 외부 제어 |
| 입력 키 감지 (DetectPressedKeyCode) | 모든 KeyCode 순회, GetKeyDown으로 첫 눌린 키 반환 |
| 키 편집 모드 활성화 (EditInputKey) | isEditingKey=true, currentKeyIndex 저장 |
| 버튼 텍스트 갱신 (ChangeInputKeyButtonText) | SaveManager.InputKeys 딕셔너리에서 KeyCode 조회 → ToString → 버튼 텍스트 설정 |
