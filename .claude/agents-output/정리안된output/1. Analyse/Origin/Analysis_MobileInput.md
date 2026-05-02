# MobileInput 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | MobileInput |
| 현재 역할 | 모바일/UI 입력 처리<br>- 일반공격, 특수공격, 피니시 공격 등 스킬 입력<br>- 대시, 락온, 메뉴 열기 입력<br>- 상호작용(Interaction) 플래그 관리<br>- 마스크별 스킬 분기 처리 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance가 null이면 현재 객체를 instance에 할당<br>2. instance가 이미 존재하면 현재 게임오브젝트 제거 | - |

### NormalAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 기록 및 현재 마스크 판정 | 1. maskChange.CurrentMask == maskChange.HumanMask 여부 확인 | MaskChange |
| 휴먼 마스크 일반공격 | 1. playerSkillInput.StoreInput(PlayerStateType.HUMAN_NORMALATTACK) 호출<br>2. playerState.isPerfomingSklill이 false면 humanMaskSkill.NormalAttack() 실행 | PlayerSkillInput <br>PlayerState <br>HumanMaskSkill |
| 동물 마스크 일반공격 | 1. playerSkillInput.StoreInput(PlayerStateType.ANIMAL_NORMALATTACK) 호출<br>2. playerState.isPerfomingSklill이 false면 animalMaskSkill.NormalAttack() 실행 | PlayerSkillInput <br>PlayerState <br>AnimalMaskSkill |

### SpecialAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 휴먼 마스크 특수공격 | 1. playerSkillInput.StoreInput(PlayerStateType.HUMAN_INKSHAPE) 호출<br>2. playerState.isPerfomingSklill이 false면 humanMaskSkill.InkShape() 실행 | PlayerSkillInput <br>PlayerState <br>HumanMaskSkill |
| 동물 마스크 특수공격 | 1. playerSkillInput.StoreInput(PlayerStateType.ANIMAL_LEAPSTRIKE) 호출<br>2. playerState.isPerfomingSklill이 false면 animalMaskSkill.LeapStrike() 실행 | PlayerSkillInput <br>PlayerState <br>AnimalMaskSkill |

### FinishAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피니시 공격 입력 및 실행 | 1. playerSkillInput.StoreInput(PlayerStateType.GHOST_FINISHSKILL) 호출로 입력 기록<br>2. playerState.isPerfomingSklill이 false면 ghostMaskSkill.Finish() 실행 | PlayerSkillInput <br>PlayerState <br>GhostMaskSkill |

### Dash()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 기록 | 1. playerSkillInput.StoreInput(PlayerStateType.DASH) 호출 | PlayerSkillInput |
| 휴먼/동물 마스크 대시 | 1. maskChange.CurrentMask == maskChange.HumanMask 여부 확인<br>2. 휴먼이면 humanMaskSkill.Dash() 실행<br>3. 동물이면 animalMaskSkill.Dash() 실행<br>4. 둘 다 playerState.isPerfomingSklill이 false일 때만 실행 | MaskChange <br>PlayerState <br>HumanMaskSkill <br>AnimalMaskSkill |

### LockOnTarget()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 카메라 락온 토글 | 1. cameraController.LockOnTarget() 호출로 락온 기능 실행 | CameraController |

### OpenMenu()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 메뉴 토글 | 1. menuUI.MenuSwitch() 호출로 메뉴 열기/닫기 | MenuUI |

### EnableInteraction()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 상호작용 플래그 활성화 | 1. interact = true로 설정 | - |

### DisableInteraction()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 모바일 입력 일시 중단 | 1. interact 플래그를 false로 설정<br>2. 모든 UI 버튼 입력 감지 차단<br>3. 플레이어 스킬 실행 불가능 상태로 변경<br>4. 메뉴 열기/닫기 입력도 무효화<br>5. 게임 일시정지/장면 전환 중 호출됨 | - |
