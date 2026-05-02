# HumanMaskSkill 분석 문서

## 클래스 개요

**클래스명:** HumanMaskSkill

**현재 역할:** 인간 마스크 상태에서 사용 가능한 모든 스킬(Normal Attack, InkShape, InkFloor, Dash)을 관리하고 실행하는 클래스

**구현 디자인 패턴:** Coroutine 패턴, State Machine 패턴

**분석날짜:** 2026-04-13

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기화 | 1. StartSet() 호출 <br>2. HumanMaskStartSet() 호출 | PlayerSkill(부모) <br>HumanMaskStartSet() |

### InitializeSkill()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 전체 스킬 시스템 초기화 | 1. InitializeCoroutine() 호출 <br>2. InitializeState() 호출 <br>3. playerSkillMove.Initialize() 호출 <br>4. playerEffect.Initialize() 호출 <br>5. playerState.Initialize() 호출 <br>6. playerSound.Initialize() 호출 <br>7. playerHitBox.Initialize() 호출 <br>8. playerSkillInput.Initialize() 호출 <br>9. gameTimeScale.Initialize() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerSkillInput <br>GameTimeScale |

### InitializeCoroutine()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 실행 중인 모든 스킬 코루틴 중지 | 1. coFirstAttack != null이면 StopCoroutine(coFirstAttack) <br>2. coSecondAttack != null이면 StopCoroutine(coSecondAttack) <br>3. coThirdAttack != null이면 StopCoroutine(coThirdAttack) <br>4. coInkShape != null이면 StopCoroutine(coInkShape) <br>5. coInkFloor != null이면 StopCoroutine(coInkFloor) <br>6. coDash != null이면 StopCoroutine(coDash) | Coroutine |

### InitializeState()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 내부 상태 플래그 초기화 | 1. 기본 공격 플래그: isPerformingFirstAttack, isPerformingSecondAttack, isPerformingThirdAttack = false <br>2. 애니메이션 플래그: isPerformingFirstAttackAnim, isPerformingSecondAttackAnim, isPerformingThirdAttackAnim = false <br>3. 공격 사용 가능: canUseFirstAttack=true, canUseSecondAttack=false, canUseThirdAttack=false <br>4. 스페셜 스킬 플래그: isPerformingInkShape, isPerformingInkFloor, isPerformingDash = false <br>5. 스페셜 스킬 애니메이션 플래그 초기화 | 없음 |

### HumanMaskStartSet()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 인간 마스크 전용 초기화 | 1. 스킬 사용 가능 상태 설정: canUseFirstAttack=true, canUseSecondAttack=false, canUseThirdAttack=false, canUseInkShape=true, canUseInkFloor=true, canUseDash=true <br>2. 모든 수행 상태 플래그 false로 초기화 <br>3. 모든 애니메이션 플래그 false로 초기화 <br>4. 모든 코루틴 참조 null로 초기화 <br>5. 히트 카운트 초기화: inkShapeHitCount=0, inkFloorHitCount[0]=0, inkFloorHitCount[1]=0 <br>6. 게임오브젝트 비활성화: normalAttackHitBox, inkShapeHitBox, inkShapeSplashEffect[], inkShapeTrail[], inkFloorProjectileEffect[], inkFloorHitBox[] | GameObject.SetActive() |

### UseSkill()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 사용 공통 처리 | 1. canUse가 true 확인 <br>2. InitializeSkill() 호출 <br>3. 기존 coroutine 실행 중이면 StopCoroutine() 호출 <br>4. UIEffect.ShowPlayerHUDFadeEffect() 호출 <br>5. coroutine = StartCoroutine(coroutineMethod()) 호출 <br>6. canUse = false로 설정 | UIEffect <br>CoroutineDelegate |

### NormalAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 첫 번째 공격 시작 | 1. UseSkill(canUseFirstAttack, coFirstAttack, CoFirstAttack) 호출 | UseSkill() <br>CoFirstAttack() |

### CoFirstAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 첫 번째 공격 수행 | 1. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_FirstNormalAttack, 0.1f) <br>2. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.HUMAN_NORMALATTACK) <br>3. playerState.ChangePlayerSubState(PlayerSubStateType.HUMAN_FIRSTNORMALATTACK) <br>4. 수행 상태 플래그: isPerformingFirstAttack=true, canUseFirstAttack=false <br>5. firstAttackStartTime = Time.time <br>6. playerSkillInput.ProcessInput(humanData.firstNormalAttackInput, firstAttackStartTime) 호출 <br>7. while 루프 - 애니메이션 상태 확인: Human_FirstNormalAttack 유지 여부 <br>8. 이동 처리: humanData.firstNormalAttackMove[] 배열의 각 이동을 playerSkillMove.SkillMove()로 실행 <br>9. 제약 설정: playerState.RestrictPlayer()로 플레이어 제약 <br>10. actRestrictWaitTime 이후 canUseSecondAttack=true 설정 <br>11. 이펙트 실행: playerEffect.TogglePlayerEffect()로 firstAttackEffect 활성화 <br>12. 히트박스 활성화: playerHitBox.TogglePlayerHitBox() 호출 <br>13. 사운드 재생: playerSound.SetPlayerSound()로 firstNormalAttackSound 재생 <br>14. 카메라 흔들림: playerCameraEffect.ShakeCamera() 호출 <br>15. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerCameraEffect <br>PlayerAnimation <br>PlayerSkillInput <br>HumanData |

### CoSecondAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 두 번째 공격 수행 | 1. playerState.ToggleSuperArmorState(true) 호출 <br>2. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_SecondNormalAttack, 0.1f) <br>3. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.HUMAN_NORMALATTACK) <br>4. playerState.ChangePlayerSubState(PlayerSubStateType.HUMAN_SECONDNORMALATTACK) <br>5. 수행 상태 플래그: isPerformingSecondAttack=true, canUseSecondAttack=false <br>6. secondAttackStartTime = Time.time <br>7. playerSkillInput.ProcessInput(humanData.secondNormalAttackInput, secondAttackStartTime) 호출 <br>8. while 루프 - 애니메이션 상태 확인 <br>9. 이동 처리: humanData.secondNormalAttackMove[] 배열 실행 <br>10. 제약 설정: actRestrictWaitTime 이후 canUseThirdAttack=true 설정 <br>11. 이펙트, 히트박스, 사운드, 카메라 흔들림 처리 (CoFirstAttack과 동일 구조) <br>12. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerCameraEffect <br>PlayerAnimation <br>PlayerSkillInput <br>HumanData |

### CoThirdAttack()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 세 번째 공격 수행 | 1. playerState.ToggleSuperArmorState(true) 호출 <br>2. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_ThirdNormalAttack, 0.1f) <br>3. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.HUMAN_NORMALATTACK) <br>4. playerState.ChangePlayerSubState(PlayerSubStateType.HUMAN_THIRDNORMALATTACK) <br>5. 수행 상태 플래그: isPerformingThirdAttack=true <br>6. thirdAttackStartTime = Time.time <br>7. playerSkillInput.ProcessInput(humanData.thirdNormalAttackInput, thirdAttackStartTime) 호출 <br>8. while 루프 - 애니메이션 상태 확인 <br>9. 이동, 제약, 이펙트, 히트박스, 사운드 처리 <br>10. 카메라 흔들림 <br>11. **GameTimeScale 조정**: gameTimeScale.CoSetTimeScale()로 시간 속도 조정 (세 번째 공격의 추가 기능) <br>12. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerCameraEffect <br>GameTimeScale <br>PlayerAnimation <br>PlayerSkillInput <br>HumanData |

### InkShape()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkShape 스킬 시작 | 1. UseSkill(canUseInkShape, coInkShape, CoInkShape) 호출 | UseSkill() <br>CoInkShape() |

### CoInkShape()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkShape 스킬 수행 (회전 공격) | 1. playerState.ToggleSuperArmorState(true) 호출 <br>2. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.HUMAN_INKSHAPE) <br>3. playerMovement.Movement 확인 후 마스크 방향 설정 <br>4. 수행 상태 플래그: isPerformingInkShape=true, canUseInkShape=false <br>5. inkShapeStartTime = Time.time <br>6. playerSkillMove.GetOriginHeight() 호출 <br>7. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_InkShape, 0.1f) <br>8. playerSkillInput.ProcessInput(humanData.inkShapeInput, inkShapeStartTime) 호출 <br>9. while 루프 - 애니메이션 상태 확인 <br>10. 이동 처리: humanData.inkShapeMove[] 배열 실행 <br>11. 제약 설정: playerState.RestrictPlayer() 호출 <br>12. 이펙트 실행: inkShapeSpinTrailEffect와 inkShapeSplashEffect 두 가지 이펙트 <br>13. 히트박스 활성화: inkShapeHitBoxWaitTime 이후 inkShapeHitBox.SetActive(true) <br>14. InvokeRepeating("InkShapeHitBoxOn", 0, humanData.inkShapeHitInterval) 호출 <br>15. 사운드: inkShapeSpinSound와 inkShapeSplashSound 두 가지 사운드 재생 <br>16. 카메라 흔들림 <br>17. 시간 속도 조정: gameTimeScale.CoSetTimeScale() 호출 <br>18. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerHitBox <br>PlayerCameraEffect <br>GameTimeScale <br>PlayerAnimation <br>PlayerSkillInput <br>HumanData <br>Invoke <br>InvokeRepeating |

### InkShapeCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkShape 쿨다운 관리 | 1. canUseInkShape가 true이면 함수 종료 <br>2. flowTimeRate 계산: (Time.time - inkShapeStartTime) / humanData.inkShapeStat.cooldown <br>3. skillHUD.SkillCooldown(PlayerStateType.HUMAN_INKSHAPE, flowTimeRate) 호출 <br>4. 현재 시간이 inkShapeStartTime + cooldown 초과 시 canUseInkShape = true 설정 | SkillHUD <br>HumanData |

### InkShapeHitBoxOn()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkShape 히트박스 반복 활성화 | 1. inkShapeHitCount++ 증가 <br>2. inkShapeHitBox.SetActive(true) 활성화 <br>3. Invoke("InkShapeHitBoxOff", humanData.inkShapeHitInterval - 0.1f) 호출 <br>4. inkShapeHitCount >= humanData.inkShapeHitCount 시 CancelInvoke("InkShapeHitBoxOn") 호출 <br>5. inkShapeHitCount 초기화 | GameObject <br>Invoke <br>HumanData |

### InkShapeHitBoxOff()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkShape 히트박스 비활성화 | 1. inkShapeHitBox.SetActive(false) 비활성화 | GameObject |

### InkFloor()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 스킬 시작 (타겟 확인) | 1. cameraController.CurrentTarget가 null이면 함수 종료 <br>2. UseSkill(canUseInkFloor, coInkShape, CoInkFloor) 호출 | CameraController <br>UseSkill() <br>CoInkFloor() |

### CoInkFloor()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 스킬 수행 (투사체 스킬) | 1. target 변수에 cameraController.CurrentTarget 저장 <br>2. playerState.ToggleSuperArmorState(true) 호출 <br>3. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.HUMAN_INKFLOOR) <br>4. playerMovement.Movement 확인 후 마스크 방향 설정 <br>5. 수행 상태 플래그: isPerformingInkFloor=true, canUseInkFloor=false <br>6. inkFloorStartTime = Time.time <br>7. 애니메이션 재생: maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_InkFloor, 0.1f) <br>8. playerSkillInput.ProcessInput(humanData.inkFloorInput, inkFloorStartTime) 호출 <br>9. while 루프 - 애니메이션 상태 확인 <br>10. 이동 처리: humanData.inkFloorMove[] 배열 실행 <br>11. 제약 설정: playerState.RestrictPlayer() 호출 <br>12. 투사체 이펙트: inkFloorProjectileEffectPosition을 target 위치로 설정 <br>13. 히트박스 활성화: inkFloorHitBoxWaitTime 이후 target 유효성 확인 (CurrentTarget, Enemy 컴포넌트, isDead) <br>14. inkFloorHitBox[0] 또는 [1] 중 미사용 상태의 것을 선택 <br>15. InvokeRepeating()으로 반복 활성화 <br>16. 사운드: inkFloorSwingSound와 inkFloorProjectileSound 두 가지 사운드 <br>17. 카메라 흔들림 <br>18. 시간 속도 조정 <br>19. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>PlayerCameraEffect <br>GameTimeScale <br>PlayerAnimation <br>PlayerSkillInput <br>HumanData <br>CameraController <br>Enemy <br>InvokeRepeating |

### InkFloorCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 쿨다운 관리 | 1. canUseInkFloor가 true이면 함수 종료 <br>2. 현재 시간이 inkFloorStartTime + cooldown 초과 시 canUseInkFloor = true 설정 | HumanData |

### InkFloorHitBoxOn1()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 첫 번째 히트박스 반복 활성화 | 1. inkFloorHitCount[0]++ 증가 <br>2. inkFloorHitBox[0].SetActive(true) 활성화 <br>3. Invoke("InkFloorHitBoxOff1", humanData.inkFloorHitInterval - 0.1f) 호출 <br>4. inkFloorHitCount[0] >= humanData.inkFloorHitCount 시 CancelInvoke("InkFloorHitBoxOn1") 호출 <br>5. inkFloorHitCount[0] 초기화 | GameObject <br>Invoke <br>HumanData |

### InkFloorHitBoxOff1()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 첫 번째 히트박스 비활성화 | 1. inkFloorHitBox[0].SetActive(false) 비활성화 | GameObject |

### InkFloorHitBoxOn2()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 두 번째 히트박스 반복 활성화 | 1. inkFloorHitCount[1]++ 증가 <br>2. inkFloorHitBox[1].SetActive(true) 활성화 <br>3. Invoke("InkFloorHitBoxOff2", humanData.inkFloorHitInterval - 0.1f) 호출 <br>4. inkFloorHitCount[1] >= humanData.inkFloorHitCount 시 CancelInvoke("InkFloorHitBoxOn2") 호출 <br>5. inkFloorHitCount[1] 초기화 | GameObject <br>Invoke <br>HumanData |

### InkFloorHitBoxOff2()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| InkFloor 두 번째 히트박스 비활성화 | 1. inkFloorHitBox[0].SetActive(false) 비활성화 - **주의: 버그로 보임 ([1]이어야 함)** | GameObject |

### Dash()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| Dash 스킬 시작 | 1. UseSkill(canUseDash, coDash, CoDash) 호출 | UseSkill() <br>CoDash() |

### CoDash()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| Dash 스킬 수행 (이동 회피) | 1. playerMovement.Movement 확인 후 마스크 방향 설정 <br>2. 플레이어 상태 변경: playerState.ChangePlayerState(PlayerStateType.DASH) <br>3. 수행 상태 플래그: isPerformingDash=true, dashStartTime = Time.time, canUseDash=false <br>4. isFrontDash 변수로 전방/후방 대시 구분 <br>5. cameraController.CurrentTarget이 있으면 후방 대시 (BackDash): <br>&nbsp;&nbsp;&nbsp;&nbsp;- maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_BackDash, 0.1f) <br>&nbsp;&nbsp;&nbsp;&nbsp;- playerSkillInput.ProcessInput(commonData.backDashInput, dashStartTime) <br>6. cameraController.CurrentTarget이 없으면 전방 대시 (FrontDash): <br>&nbsp;&nbsp;&nbsp;&nbsp;- maskChange.CurrentAnimator.CrossFade(playerAnimation.Human_FrontDash, 0.1f) <br>&nbsp;&nbsp;&nbsp;&nbsp;- playerSkillInput.ProcessInput(commonData.dashInput, dashStartTime) <br>7. while 루프 - 애니메이션 상태 확인 <br>8. 이동 처리: isFrontDash 여부에 따라 commonData.dashMove[] 또는 commonData.backDashMove[] 실행 <br>9. 제약 설정: isFrontDash 여부에 따라 다른 제약 적용 <br>10. 사운드: isFrontDash 여부에 따라 dashSound 또는 backDashSound 재생 <br>11. 애니메이션 종료 시 InitializeSkill() 호출 | PlayerSkillMove <br>PlayerState <br>PlayerSound <br>CameraController <br>PlayerAnimation <br>PlayerSkillInput <br>CommonData |

### DashCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| Dash 쿨다운 관리 | 1. canUseDash가 true이면 함수 종료 <br>2. flowTimeRate 계산: (Time.time - dashStartTime) / commonData.dashCooldown <br>3. skillHUD.SkillCooldown(PlayerStateType.DASH, flowTimeRate) 호출 <br>4. 현재 시간이 dashStartTime + commonData.dashCooldown 초과 시 canUseDash = true 설정 | SkillHUD <br>CommonData |
