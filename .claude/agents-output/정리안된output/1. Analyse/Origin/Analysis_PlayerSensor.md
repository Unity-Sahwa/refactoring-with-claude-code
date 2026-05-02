# PlayerSensor 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerSensor |
| 현재 역할 | 플레이어 충돌 감지<br>- 전방 방향 장애물 감지<br>- 후방 방향 장애물 감지<br>- 이동 가능 여부 판정 |
| 구현 디자인 패턴 | MonoBehaviour (센서 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 및 데이터 초기화 | 1. Player.instance에서 player 객체 획득<br>2. PlayerCommonData.Instance에서 commonData 획득 | Player <br>PlayerCommonData |

### Update()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 매 프레임 센서 업데이트 | 1. DetectFrontCollider() 호출로 전방 감지<br>2. DetectBackCollider() 호출로 후방 감지 | DetectFrontCollider() <br>DetectBackCollider() |

### DetectFrontCollider()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 전방 센서 레이캐스트 설정 | 1. playerPosition = player.transform.position + player.transform.up 으로 플레이어 위 기준점 설정<br>2. Debug.DrawRay()로 디버그 라인 표시(빨간색) | Transform <br>Physics |
| 전방 장애물 감지 | 1. Physics.Raycast()로 전방 감지:<br>   - 방향: player.transform.forward<br>   - 거리: commonData.playerForwardSensorRange<br>   - 레이어: commonData.collisionLayer<br>   - 트리거 무시<br>2. 충돌 있으면 canNotMoveforward = true<br>3. 충돌 없으면 canNotMoveforward = false | Physics <br>PlayerCommonData |

### DetectBackCollider()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 후방 센서 레이캐스트 설정 | 1. playerPosition = player.transform.position + player.transform.up 으로 플레이어 위 기준점 설정<br>2. Debug.DrawRay()로 디버그 라인 표시(초록색) | Transform <br>Physics |
| 후방 장애물 감지 | 1. Physics.Raycast()로 후방 감지:<br>   - 방향: -player.transform.forward<br>   - 거리: commonData.playerBackwardSensorRange<br>   - 레이어: commonData.collisionLayer<br>   - 트리거 무시<br>2. 충돌 있으면 canNotMoveBackward = true<br>3. 충돌 없으면 canNotMoveBackward = false | Physics <br>PlayerCommonData |
