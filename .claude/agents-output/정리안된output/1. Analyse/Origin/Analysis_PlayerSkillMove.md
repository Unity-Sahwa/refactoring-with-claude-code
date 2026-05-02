# PlayerSkillMove 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerSkillMove |
| 현재 역할 | 스킬 실행 중 플레이어 강제 이동 관리<br>- 스킬 기반 이동<br>- 방향별 이동 제어<br>- 충돌 감지 |
| 구현 디자인 패턴 | MonoBehaviour (이동 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기화 | 1. maskChange 참조 획득 <br>2. playerRigidbody 참조 획득 <br>3. playerSensor 참조 획득 <br>4. stopMoveCoroutine = false로 초기화 | MaskChange <br>PlayerSensor |

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 이동 상태 초기화 | 1. stopMoveCoroutine = true<br>2. 이동 플래그 초기화<br>3. 이동 속도 0 설정 | - |

### SkillMove(SkillMoveStruct moveStruct)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 이동 코루틴 | 1. stopMoveCoroutine = false<br>2. waitTime 후 MoveSwitch(direction, true)<br>3. 방향별 속도 설정<br>4. waitTime + duration 후 MoveSwitch(direction, false) | - |

### RayCastForward()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 전방 레이캐스트 충돌 감지 | 1. 캐릭터 위치 기준점 설정<br>2. 6개 레이캐스트 발사<br>3. 적 레이어 충돌 시 isMovingForward = false | MaskChange <br>CameraData |

### UpdateSkillMovement()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 이동 실제 적용 | 1. 이동 플래그 확인<br>2. 방향별 벡터 계산<br>3. PlayerSensor 센서 체크<br>4. Rigidbody.MovePosition으로 이동 | MaskChange <br>PlayerSensor |

### GetOriginHeight()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 원본 높이 값 반환 | 1. 플레이어의 초기 Y 위치 (originHeight) 값을 반환 <br>2. 스킬 수행 시 높이 기반 이동 제어에 사용 | 없음 |

### MoveSwitch(MoveDirection dir, bool moveSwitch)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 이동 방향 전환 | 1. MoveDirection 파라미터 판정<br>2. FRONT: isMovingForward = moveSwitch로 전방 이동 플래그 제어<br>3. BACK: isMovingBackward = moveSwitch로 후방 이동 플래그 제어<br>4. UP: isMovingUpward = moveSwitch로 상방 이동 플래그 제어<br>5. DOWN: isMovingDownward = moveSwitch로 하방 이동 플래그 제어<br>6. UpdateSkillMovement()에서 이 플래그들을 기반으로 실제 이동 계산<br>7. moveSwitch=true: 이동 시작, false: 이동 종료 | MoveDirection |
