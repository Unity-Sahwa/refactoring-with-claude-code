# AnimalMaskSkill 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | AnimalMaskSkill |
| 현재 역할 | 동물 마스크 스킬 관리<br>- 일반공격 (1단계, 2단계, 3단계)<br>- 특수공격 (도약 공격, 포효)<br>- 대시 스킬 |
| 구현 디자인 패턴 | PlayerSkill 상속 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기화 및 컴포넌트 설정 | 1. StartSet() 호출로 기본 데이터 로드<br>2. AnimalMaskStartSet() 호출로 동물 마스크 전용 설정<br>3. 게임 오브젝트 비활성화 | - |

### InitializeSkill()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 상태 완전 초기화 | 1. InitializeCoroutine() 호출 <br>2. InitializeWeapon() 호출 <br>3. InitializeHitBox() 호출 <br>4. InitializeState() 호출 <br>5. playerSkillMove.Initialize() 호출 <br>6. playerEffect.Initialize() 호출 <br>7. playerState.Initialize() 호출 <br>8. playerSound.Initialize() 호출 <br>9. playerSkillInput.Initialize() 호출 <br>10. gameTimeScale.Initialize() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerSkillInput <br>GameTimeScale |

### InitializeCoroutine()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 실행 중인 모든 스킬 코루틴 중지 | 1. coFirstAttack != null이면 StopCoroutine(coFirstAttack) <br>2. coSecondAttack != null이면 StopCoroutine(coSecondAttack) <br>3. coThirdAttack != null이면 StopCoroutine(coThirdAttack) <br>4. coLeapStrike != null이면 StopCoroutine(coLeapStrike) <br>5. coRoar != null이면 StopCoroutine(coRoar) <br>6. coDash != null이면 StopCoroutine(coDash) | Coroutine |

### InitializeWeapon()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 무기 메시 비활성화 | 1. rightHandWeaponMesh.enabled = false <br>2. leftHandWeaponMesh.enabled = false | MeshRenderer |

### InitializeHitBox()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 히트박스 비활성화 | 1. leapStrikeHitBox.SetActive(false) <br>2. roarHitBox.SetActive(false) | GameObject |

### InitializeState()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 내부 상태 플래그 초기화 | 1. 기본 공격 플래그: isPerformingFirstAttackAnim, isPerformingSecondAttackAnim, isPerformingThirdAttackAnim = false <br>2. 공격 사용 가능: canUseFirstAttack=true, canUseSecondAttack=false, canUseThirdAttack=false <br>3. 스페셜 스킬 플래그: isPerformingLeapStrike, isPerformingRoar, isPerformingDash = false <br>4. 스페셜 스킬 애니메이션 플래그: isPerformingLeapStrikeAnim, isPerformingRoarAnim, isPerformingDashAnim = false | 없음 |

### AnimalMaskStartSet()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 동물 마스크 전용 초기화 | 1. 스킬 사용 가능 상태 설정: canUseFirstAttack=true, canUseSecondAttack=false, canUseThirdAttack=false, canUseLeapStrike=true, canUseRoar=true, canUseDash=true <br>2. 모든 수행 상태 플래그 false로 초기화 <br>3. 모든 애니메이션 플래그 false로 초기화 <br>4. 모든 코루틴 참조 null로 초기화 <br>5. 게임오브젝트 비활성화: leapStrikeHitBox 및 각 효과 배열 <br>6. 무기 메시 초기화: rightHandWeaponMesh, leftHandWeaponMesh 비활성화 <br>7. 히트 카운트 초기화: leapStrikeHitCount=0, roarHitCount=0 | GameObject.SetActive() <br>MeshRenderer |

### NormalAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 첫 번째 공격 시작 | 1. UseSkill() 메서드 호출로 스킬 실행 | - |

### CoFirstAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 첫 번째 일반공격 코루틴 | 1. 애니메이션 재생 (Animal_FirstNormalAttack)<br>2. PlayerState 변경<br>3. 입력 처리 시작<br>4. 이동, 이펙트, 무기, 히트박스, 사운드 처리<br>5. 상태 변화 감지 시 종료 | MaskChange <br>PlayerAnimation <br>PlayerState <br>PlayerSkillInput <br>PlayerSkillMove <br>PlayerEffect <br>PlayerHitBox <br>PlayerSound |

### CoSecondAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 두 번째 일반공격 코루틴 | 1. Super Armor 활성화<br>2. 애니메이션 재생 (Animal_SecondNormalAttack)<br>3. 세 번째 공격 조건 설정<br>4. 이동, 이펙트, 무기, 히트박스, 사운드 처리 | MaskChange <br>PlayerAnimation <br>PlayerState <br>PlayerSkillMove <br>PlayerEffect <br>PlayerHitBox <br>PlayerSound |

### CoThirdAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 세 번째 일반공격 코루틴 | 1. Super Armor 활성화<br>2. 마지막 공격 처리<br>3. 스킬 초기화 | - |

### LeapStrike()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 도약 공격 시작 | 1. UseSkill() 메서드 호출 | - |

### CoLeapStrike()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 도약 공격 코루틴 | 1. playerState.ToggleSuperArmorState(true) 호출 <br>2. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.ANIMAL_LEAPSTRIKE) <br>3. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_LeapStrike, 0.1f) <br>4. 수행 상태 플래그: isPerformingLeapStrike=true, canUseLeapStrike=false <br>5. leapStrikeStartTime = Time.time <br>6. playerSkillInput.ProcessInput(animalData.leapStrikeInput, leapStrikeStartTime) 호출 <br>7. while 루프 - 애니메이션 상태 확인 <br>8. 이동 처리: animalData.leapStrikeMove[] 배열 실행 <br>9. 제약 설정: playerState.RestrictPlayer() 호출 <br>10. 이펙트 실행: leapStrikeTrailEffect와 leapStrikeSplashEffect 두 가지 <br>11. 무기 활성화: rightHandWeaponMesh.enabled = true <br>12. 히트박스 활성화: leapStrikeHitBoxWaitTime 이후 InvokeRepeating() <br>13. 사운드: leapStrikeSpinSound와 leapStrikeSplashSound <br>14. 카메라 흔들림: playerCameraEffect.ShakeCamera() <br>15. 시간 속도 조정: gameTimeScale.CoSetTimeScale() <br>16. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerCameraEffect <br>PlayerAnimation <br>PlayerSkillInput <br>AnimalData <br>InvokeRepeating |

### LeapStrikeCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 도약 공격 쿨다운 관리 | 1. canUseLeapStrike가 true이면 함수 종료 <br>2. flowTimeRate 계산: (Time.time - leapStrikeStartTime) / animalData.leapStrikeStat.cooldown <br>3. skillHUD.SkillCooldown(PlayerStateType.ANIMAL_LEAPSTRIKE, flowTimeRate) 호출 <br>4. 현재 시간이 leapStrikeStartTime + cooldown 초과 시 canUseLeapStrike = true 설정 | SkillHUD <br>AnimalData |

### LeapStrikeHitBoxOff()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 도약 공격 히트박스 비활성화 | 1. leapStrikeHitBox.SetActive(false) 비활성화 | GameObject |

### Roar()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 포효 스킬 시작 | 1. UseSkill(canUseRoar, coRoar, CoRoar) 호출 | UseSkill() <br>CoRoar() |

### CoRoar()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 포효 스킬 수행 | 1. playerState.ToggleSuperArmorState(true) 호출 <br>2. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.ANIMAL_ROAR) <br>3. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_Roar, 0.1f) <br>4. 수행 상태 플래그: isPerformingRoar=true, canUseRoar=false <br>5. roarStartTime = Time.time <br>6. playerSkillMove.GetOriginHeight() 호출 <br>7. playerSkillInput.ProcessInput(animalData.roarInput, roarStartTime) 호출 <br>8. while 루프 - 애니메이션 상태 확인 <br>9. 이동 처리: animalData.roarMove[] 배열 실행 <br>10. 제약 설정: playerState.RestrictPlayer() 호출 <br>11. 이펙트 실행: roarChargeEffect와 roarDisChargeEffect 및 roarTrailEffect <br>12. 히트박스 활성화: roarHitBoxWaitTime 이후 InvokeRepeating() <br>13. 사운드: roarChargeSound와 roarDisChargeSound <br>14. 카메라 흔들림: playerCameraEffect.ShakeCamera() <br>15. 시간 속도 조정: gameTimeScale.CoSetTimeScale() <br>16. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerCameraEffect <br>PlayerAnimation <br>PlayerSkillInput <br>AnimalData <br>InvokeRepeating |

### roarCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 포효 쿨다운 관리 | 1. canUseRoar가 true이면 함수 종료 <br>2. flowTimeRate 계산: (Time.time - roarStartTime) / animalData.roarStat.cooldown <br>3. skillHUD.SkillCooldown(PlayerStateType.ANIMAL_ROAR, flowTimeRate) 호출 <br>4. 현재 시간이 roarStartTime + cooldown 초과 시 canUseRoar = true 설정 | SkillHUD <br>AnimalData |

### RoarHitBoxOn()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 포효 히트박스 반복 활성화 | 1. roarHitCount++ 증가 <br>2. roarHitBox.SetActive(true) 활성화 <br>3. Invoke("RoarHitBoxOff", animalData.roarHitInterval - 0.1f) 호출 <br>4. roarHitCount >= animalData.roarHitCount 시 CancelInvoke("RoarHitBoxOn") 호출 <br>5. roarHitCount 초기화 | GameObject <br>Invoke <br>AnimalData |

### RoarHitBoxOff()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 포효 히트박스 비활성화 | 1. roarHitBox.SetActive(false) 비활성화 | GameObject |

### Dash()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 대시 스킬 시작 | 1. UseSkill(canUseDash, coDash, CoDash) 호출 | UseSkill() <br>CoDash() |

### CoDash()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 대시 스킬 수행 | 1. playerMovement.Movement 확인 후 마스크 방향 설정 <br>2. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.DASH) <br>3. 수행 상태 플래그: isPerformingDash=true, dashStartTime = Time.time, canUseDash=false <br>4. isFrontDash 변수로 전방/후방 대시 구분 <br>5. cameraController.CurrentTarget이 있으면 후방 대시 (BackDash): <br>&nbsp;&nbsp;&nbsp;&nbsp;- maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_BackDash, 0.1f) <br>&nbsp;&nbsp;&nbsp;&nbsp;- playerSkillInput.ProcessInput(commonData.backDashInput, dashStartTime) <br>6. cameraController.CurrentTarget이 없으면 전방 대시 (FrontDash): <br>&nbsp;&nbsp;&nbsp;&nbsp;- maskChange.CurrentAnimator.CrossFade(playerAnimation.Animal_FrontDash, 0.1f) <br>&nbsp;&nbsp;&nbsp;&nbsp;- playerSkillInput.ProcessInput(commonData.dashInput, dashStartTime) <br>7. while 루프 - 애니메이션 상태 확인 <br>8. 이동 처리: isFrontDash 여부에 따라 commonData.dashMove[] 또는 commonData.backDashMove[] 실행 <br>9. 제약 설정: isFrontDash 여부에 따라 다른 제약 적용 <br>10. 사운드: isFrontDash 여부에 따라 dashSound 또는 backDashSound 재생 <br>11. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerState <br>PlayerSound <br>CameraController <br>PlayerAnimation <br>PlayerSkillInput <br>CommonData |

### DashCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 대시 쿨다운 관리 | 1. canUseDash가 true이면 함수 종료 <br>2. flowTimeRate 계산: (Time.time - dashStartTime) / commonData.dashCooldown <br>3. skillHUD.SkillCooldown(PlayerStateType.DASH, flowTimeRate) 호출 <br>4. 현재 시간이 dashStartTime + commonData.dashCooldown 초과 시 canUseDash = true 설정 | SkillHUD <br>CommonData |

### UseSkill(bool canUse, Coroutine coroutine, CoroutineDelegate coroutineMethod)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 사용 공통 처리 | 1. canUse가 true 확인 <br>2. InitializeSkill() 호출 <br>3. 기존 coroutine 실행 중이면 StopCoroutine() 호출 <br>4. UIEffect.ShowPlayerHUDFadeEffect() 호출 <br>5. coroutine = StartCoroutine(coroutineMethod()) 호출 <br>6. canUse = false로 설정 | UIEffect <br>CoroutineDelegate |
