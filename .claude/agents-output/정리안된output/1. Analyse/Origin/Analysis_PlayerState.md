# PlayerState 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerState |
| 현재 역할 | 플레이어 상태 관리<br>- 플레이어 현재 상태 및 서브상태 관리<br>- 스킬 수행 여부 판정<br>- 행동/이동/회전 제약 조건 관리<br>- 슈퍼아머, 무적 특수 상태 관리 |
| 구현 디자인 패턴 | MonoBehaviour (상태 머신 패턴) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 상태 초기화 | 1. isPerfomingSklill = false로 초기화<br>2. doNotAct = false (행동 제약 해제)<br>3. doNotMove = false (이동 제약 해제)<br>4. doNotRotate = false (회전 제약 해제)<br>5. stopRestrictCoroutine = true (제약 코루틴 중지 플래그 설정)<br>6. isInvincible = false (무적 상태 해제)<br>7. isSuperArmor = false (슈퍼아머 상태 해제) | - |

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 전체 상태 초기화 | 1. stopRestrictCoroutine = true로 실행 중인 제약 코루틴 중지<br>2. playerCurrentState = PlayerStateType.IDLE로 기본 상태 설정<br>3. playerCurrentSubState = PlayerSubStateType.NONE으로 서브상태 초기화<br>4. isPerfomingSklill = false로 스킬 수행 상태 해제<br>5. ToggleSuperArmorState(false) 호출로 슈퍼아머 해제<br>6. ToggleInvincibleState(false) 호출로 무적 해제<br>7. doNotAct, doNotMove, doNotRotate 모두 false로 설정 | ToggleSuperArmorState() <br>ToggleInvincibleState() |

### ChangePlayerState(PlayerStateType state)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 메인 상태 변경 | 1. playerCurrentState = state로 새로운 상태 할당<br>2. IsPerformingSKill() 호출로 현재 상태에 따른 스킬 수행 여부 자동 판정 | PlayerStateType <br>IsPerformingSKill() |

### ChangePlayerSubState(PlayerSubStateType subState)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 서브상태 변경 | 1. playerCurrentSubState = subState로 새로운 서브상태 할당<br>2. IsPerformingSKill() 호출로 스킬 수행 여부 자동 판정 | PlayerSubStateType <br>IsPerformingSKill() |

### IsPerformingSKill()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 수행 여부 판정 | 1. playerCurrentState에 따라 isPerfomingSklill 자동 판정:<br>   - NONE, IDLE, WALK, HIT, DEAD → false (스킬 미수행)<br>   - 그 외 모든 상태 → true (스킬 수행 중)<br>2. Switch 표현식 사용으로 깔끔한 조건 처리 | PlayerStateType |

### ToggleSuperArmorState(bool toggleSwitch)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 슈퍼아머 상태 토글 | 1. isSuperArmor = toggleSwitch로 슈퍼아머 상태 설정<br>2. true: 피격 시 넉백 경감 또는 저항<br>3. false: 정상 피격 반응 | - |

### ToggleInvincibleState(bool toggleSwitch)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 무적 상태 토글 | 1. isInvincible = toggleSwitch로 무적 상태 설정<br>2. true: 모든 피해로부터 완전 보호<br>3. false: 정상 피해 수용 | - |

### RestrictPlayer(RestrictStruct restrictStruct, float skillStartTime)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 시간 기반 플레이어 행동 제약 | **행동(Act) 제약:**<br>1. restrictStruct.actRestrictDuration != 0이면:<br>   - Time.time >= skillStartTime + actRestrictWaitTime + actRestrictDuration: doNotAct = false (제약 해제)<br>   - Time.time >= skillStartTime + actRestrictWaitTime: doNotAct = true (제약 적용)<br><br>**이동(Move) 제약:**<br>2. restrictStruct.moveRestrictDuration != 0이면:<br>   - Time.time >= skillStartTime + moveRestrictWaitTime + moveRestrictDuration: doNotMove = false (제약 해제)<br>   - Time.time >= skillStartTime + moveRestrictWaitTime: doNotMove = true (제약 적용)<br><br>**회전(Rotate) 제약:**<br>3. restrictStruct.rotateRestrictDuration != 0이면:<br>   - Time.time >= skillStartTime + rotateRestrictWaitTime + rotateRestrictDuration: doNotRotate = false (제약 해제)<br>   - Time.time >= skillStartTime + rotateRestrictWaitTime: doNotRotate = true (제약 적용)<br><br>**목적:** 스킬 시전 시 플레이어가 일정 시간 동안 행동하지 못하도록 제약 | RestrictStruct |

### RestrictPlayer(PlayerRestrictionType restrictionType, bool doRestrict)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 즉시 플레이어 행동 제약 | 1. Switch문으로 restrictionType 판정:<br>   - ACT: doNotAct = doRestrict<br>   - MOVE: doNotMove = doRestrict<br>   - ROTATE: doNotRotate = doRestrict<br>2. true=제약 적용, false=제약 해제<br>3. 즉시 적용되며 시간 조건 없음 | PlayerRestrictionType |
