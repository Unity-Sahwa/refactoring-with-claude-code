# Player 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | Player |
| 현재 역할 | 플레이어 체력 및 손상 처리<br>- 체력 관리<br>- 손상 처리<br>- 죽음 처리<br>- 피격 반응 |
| 구현 디자인 패턴 | 싱글톤, IDamageable 구현 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance 설정 (중복 방지) | - |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기 설정 | 1. 데이터 로드<br>2. SetUp() 호출 | PlayerCommonData <br>SaveManager |

### ApplyDamage(DamageMessage damageMessage)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 손상 적용 | 1. 무적/죽음 상태 확인<br>2. currentHP 감소<br>3. HpHUD 업데이트<br>4. HP <= 0: PlayerState DEAD, DieAction() 호출<br>5. Super Armor 없음: PlayerState HIT, HitAction() 코루틴 시작 | HpHUD <br>PlayerState |

### HitAction()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피격 반응 코루틴 | 1. 스킬 초기화<br>2. 리지드바디 속도 0<br>3. 피격 애니메이션 재생<br>4. 애니메이션 완료 대기 | MaskChange <br>PlayerAnimation <br>HumanMaskSkill <br>AnimalMaskSkill <br>PlayerSound |

### DieAction()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 죽음 처리 시작 | 1. CoDieAction() 코루틴 시작 | - |

### CoDieAction()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 죽음 처리 코루틴 | **DEAD_FALL:** 1. HP > 3: 데이터 로드, HP 감소, 저장<br>**DEAD_HPZERO:** 2. 플레이어 컨트롤 비활성화<br>3. 죽음 애니메이션<br>4. 음향 및 카메라 효과<br>5. Death Screen 표시<br>6. 씬 로드 | PlayerState <br>SaveManager <br>UIEffect <br>LoadingUI <br>PlayerAnimation <br>PlayerSound <br>SceneManager |

### SetUp()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 초기 설정 | 1. playerCommonData 로드 <br>2. currentHP = playerCommonData.maxHP로 설정 <br>3. isDead = false <br>4. isInvincible = false <br>5. HpHUD 업데이트 | PlayerCommonData <br>HpHUD |

### RestoreHealth()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 체력 회복 | 1. currentHP = playerCommonData.maxHP로 설정 <br>2. isDead = false <br>3. HpHUD 업데이트 | HpHUD |

### CheckDie()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 사망 여부 확인 | 1. isDead 플래그 반환 | 없음 |

### HitCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피격 쿨타임 관리 | 1. 현재 시간과 마지막 피격 시간 비교 <br>2. 쿨타임 만료 시 피격 가능 상태로 변경 <br>3. HpHUD 무적 표시 업데이트 | HpHUD |

### Loading()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 저장된 데이터 로드 | 1. SaveManager에서 저장된 체력 데이터 로드 <br>2. currentHP 업데이트 <br>3. isDead 플래그 복원 <br>4. HpHUD 업데이트 | SaveManager <br>HpHUD |

### FollowCharacterObject()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 캐릭터 오브젝트 위치/회전 동기화 | 1. maskChange.CurrentMask의 Transform 참조<br>2. 자신의 transform.position = CurrentMask의 position으로 설정<br>3. 자신의 transform.rotation = CurrentMask의 rotation으로 설정<br>4. 마스크 변경(휴먼 ↔ 동물) 후 위치 즉시 동기화<br>5. FixedUpdate()에서 매 물리 프레임 호출<br>6. 플레이어 게임오브젝트 = 카메라 타겟/콜라이더 기준점<br>7. 실제 메시(Mesh)는 CurrentMask(휴먼/동물)에 부착<br>8. 플레이어 위치 = 마스크 위치로 유지 | MaskChange <br>Transform |
