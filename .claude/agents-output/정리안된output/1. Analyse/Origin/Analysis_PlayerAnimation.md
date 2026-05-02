# PlayerAnimation 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerAnimation |
| 현재 역할 | 플레이어 애니메이션 상태 및 속도 관리<br>- 애니메이션 상태 업데이트<br>- 구간별 애니메이션 속도 조정<br>- 이동 상태 감지 |
| 구현 디자인 패턴 | MonoBehaviour (애니메이션 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 애니메이션 초기화 | 1. 플레이어 데이터 로드<br>2. 애니메이션 속도 파라미터 초기화 | PlayerHumanMaskData <br>PlayerAnimalMaskData <br>PlayerGhostMaskData <br>PlayerCommonData |

### UpdateAnimationState()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 애니메이션 상태 매 프레임 업데이트 | 1. 현재 애니메이션 해시 판별<br>2. SetAnimationSpeed() 호출<br>3. moveAmount >= 0.5f: WALK 상태<br>4. moveAmount == 0: IDLE 상태<br>5. 사운드 처리 | MaskChange <br>PlayerMovement <br>PlayerState <br>PlayerSound <br>PlayerCommonData |

### SetAnimationSpeed(animationSpeedStruct[] animationStruct, float normalizedTime, string speedRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 애니메이션 구간별 재생 속도 조정 | 1. animationSpeedStruct 배열 순회<br>2. normalizedTime (0~1) 기반 현재 구간 판정<br>3. startTime <= normalizedTime < endTime 범위 확인<br>4. 해당 구간의 speed 값 추출<br>5. Animator.speed = speed로 현재 재생 속도 설정<br>6. 범위: 0 (정지) ~ 2+ (고속 재생)<br>7. 모든 애니메이션 계층에 일괄 적용<br>8. 슬로우 모션 이펙트와 함께 사용<br>9. 구간별 동적 속도 조절로 섬세한 연출 가능 | Animator <br>MaskChange <br>PlayerCommonData |
