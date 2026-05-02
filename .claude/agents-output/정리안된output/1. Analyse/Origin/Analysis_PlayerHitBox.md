# PlayerHitBox 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerHitBox |
| 현재 역할 | 플레이어 히트박스 제어<br>- 휴먼/동물 마스크 일반공격 히트박스 관리<br>- 히트박스 타이밍 제어(활성화/비활성화)<br>- 히트박스 시각화(디버그 표시) |
| 구현 디자인 패턴 | MonoBehaviour (히트박스 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 코루틴 중지 플래그 설정 | 1. stopHitboxCoroutine = true로 설정하여 실행 중인 코루틴 정지 신호 전달 | - |

### TogglePlayerHitBox(HitBoxStruct hitboxStruct)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 코루틴 활성화 여부 확인 | 1. hitboxStruct.useFunction이 false면 코루틴 종료 | HitBoxStruct |
| 코루틴 중지 플래그 초기화 | 1. stopHitboxCoroutine = false로 설정하여 새 코루틴 시작 준비 | - |
| 히트박스 선택 및 설정 | 1. SelectHitBox(hitboxStruct) 호출로 해당 히트박스 선택<br>2. hitBox.GetComponent<MeshRenderer>().enabled을 hitboxStruct.showHitbox 값으로 설정<br>   (디버그 표시 여부) | SelectHitBox() <br>MeshRenderer |
| 활성화/비활성화 타이밍 제어 | 1. while 루프에서 Time.time 기반 타이밍 판정<br>2. waitTime 후에 히트박스 활성화 (activeHitBoxOnce 플래그로 일회성 보장)<br>3. waitTime + duration 후에 히트박스 비활성화 | GameObject |
| 외부 중지 신호 처리 | 1. stopHitboxCoroutine이 true이면:<br>   - hitboxStruct.untilFinish가 false면 즉시 비활성화하고 코루틴 종료<br>   - true면 코루틴 계속 실행 | HitBoxStruct |
| 히트박스 활성 상태 모니터링 | 1. isHitBoxActive가 true이고 hitBox.activeSelf가 false면 코루틴 종료<br>   (스킬 중단 시 히트박스 강제 종료) | GameObject |
| 코루틴 대기 | 1. yield return null로 매프레임 대기 | - |

### SelectHitBox(HitBoxStruct hitboxStruct)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 히트박스 타입별 선택 | 1. switch문으로 hitboxStruct.hitBoxType 판정:<br>   - HUMAN_NORMALATTACK_FIRST: humanNormalAttackHitBox[0]<br>   - HUMAN_NORMALATTACK_SECOND: humanNormalAttackHitBox[1]<br>   - HUMAN_NORMALATTACK_THIRD: humanNormalAttackHitBox[2]<br>   - ANIMAL_NORMALATTACK_FIRST: animalNormalAttackHitBox[0]<br>   - ANIMAL_NORMALATTACK_SECOND: animalNormalAttackHitBox[1]<br>   - ANIMAL_NORMALATTACK_THIRD: animalNormalAttackHitBox[2]<br>2. 선택된 히트박스 반환 | HitBoxStruct <br>HitBoxType |
