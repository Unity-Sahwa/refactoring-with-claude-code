# PlayerController 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerController |
| 현재 역할 | 플레이어 입력 처리 및 스킬 관리 중추<br>- 플레이어 이동 및 회전 제어<br>- 스킬 입력 처리<br>- 메뉴 입력 처리<br>- 플레이어 상태 갱신 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 및 컴포넌트 획득 | 1. instance 설정 (중복 방지)<br>2. GetComponent<MaskChange>() - 마스크 전환 컴포넌트<br>3. GetComponent<PlayerMovement>() - 이동 제어 컴포넌트<br>4. GetComponentInChildren으로 스킬 클래스 획득:<br>   - HumanMaskSkill<br>   - AnimalMaskSkill<br>   - GhostMaskSkill<br>5. GetComponent<Player>() - 플레이어 체력 관리<br>6. PlayerCommonData.Instance - 공통 데이터 로드<br>7. MaskChange.InitialSetUp() 호출 | MaskChange <br>PlayerMovement <br>HumanMaskSkill <br>AnimalMaskSkill <br>GhostMaskSkill <br>Player <br>PlayerCommonData |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| SaveManager 참조 획득 | 1. SaveManager.instance를 캐싱하여 이후 접근 용이 | SaveManager |

### Update()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 매 프레임 입력 처리 및 상태 갱신 | **1단계 입력 처리:**<br>1. 메뉴 키 입력 확인 → MenuUI.MenuSwitch() 호출<br>2. TimelineHelper.IsTimelinePlaying() 확인 (타임라인 재생 중 반환)<br>3. MenuUI 활성화 확인 (메뉴 열려있으면 반환)<br>4. 플레이어 컨트롤 비활성화 확인<br>5. PlayerState == DEAD 확인 (죽음 상태면 반환)<br><br>**2단계 상태 갱신:**<br>6. PlayerAnimation.UpdateAnimationState() 호출<br>7. PlayerState.HitCooldown() 호출<br><br>**3단계 HUD 업데이트:**<br>8. 스킬 사용 가능 여부 확인 → UIEffect.ShowPlayerHUDFadeEffect()<br>9. 각 스킬별 SkillHUD.SkillCooldown() 호출<br><br>**4단계 플레이어 상태 확인:**<br>10. PlayerState.RestrictPlayer() - 플레이어 제약 갱신<br>11. GhostMaskSkill.DetectTargetToFinish() - 피니시 대상 감지<br><br>**5단계 이동/회전 입력:**<br>12. PlayerMovement.InputMovement() - 이동 입력 처리<br>13. PlayerMovement.CharacterRotate() - 회전 처리<br>14. 플레이어 제약 확인 후 실행<br><br>**6단계 스킬 입력 (플랫폼별):**<br>15. PC: 마우스/키보드 입력 처리<br>16. 모바일: MobileInput 스킬 호출 | SaveManager <br>MenuUI <br>TimelineHelper <br>PlayerAnimation <br>PlayerState <br>UIEffect <br>PlayerMovement <br>GhostMaskSkill <br>CameraController <br>CheatMode <br>PlatformSwitcher |

### FixedUpdate()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 물리 엔진 기반 이동 및 상태 갱신 | 1. Player.FollowCharacterObject() 호출로 플레이어 위치를 마스크 위치에 동기화<br>2. CheatMode.isGameSlowed 확인 (치트 비행 모드)<br>3. PlayerMovement.AddGravity() 호출로 중력 적용<br>4. PlayerMovement.CharacterMove() 호출로 실제 위치 이동<br>5. PlayerSkillMove.UpdateSkillMovement() 호출로 스킬 이동 적용<br>6. 스킬 이동과 일반 이동 블렌딩<br>7. PlayerAnimation.SetMovementBlend() 호출로 애니메이션 블렌드 업데이트<br>8. PlayerSensor.RayCastDown() 호출로 지면 판정<br>9. PlayerState == DEAD 확인 (사망시 조기 반환)<br>10. GhostMaskSkill.FinishSkillCooldown() 호출로 피니시 쿨다운 관리<br>11. 시간 기반 물리 엔진(Time.fixedDeltaTime) 기반 처리<br>12. doNotRotate/doNotMove 플래그로 이동 제약 적용 | Player <br>PlayerMovement <br>PlayerSkillMove <br>PlayerAnimation <br>PlayerSensor <br>CheatMode <br>PlayerState <br>GhostMaskSkill |
