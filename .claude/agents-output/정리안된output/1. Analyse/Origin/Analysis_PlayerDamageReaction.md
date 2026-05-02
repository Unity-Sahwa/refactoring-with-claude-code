# PlayerDamageReaction 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerDamageReaction |
| 현재 역할 | 플레이어 손상 이벤트 처리<br>- 플레이어 사망 상태 전환<br>- 사망 애니메이션 실행 |
| 구현 디자인 패턴 | EventData 상속 패턴 (이벤트 시스템) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 인스턴스 캐싱 | 1. Player.instance에서 player 객체 획득 | Player |

### Execute()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 사망 상태 전환 | 1. playerState.ChangePlayerState(PlayerStateType.DEAD) 호출<br>2. playerState.ChangePlayerSubState(PlayerSubStateType.DEAD_FALL) 호출 | PlayerState <br>PlayerStateType |
| 사망 애니메이션 실행 | 1. player.StartCoroutine(player.CoDieAction()) 호출로 사망 애니메이션 재생 | Player |
