# PlayerEffect 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerEffect |
| 현재 역할 | 플레이어 스킬 이펙트 관리<br>- 히트 이펙트 표시<br>- 위치 기반 이펙트<br>- 이펙트 타이밍 제어 |
| 구현 디자인 패턴 | MonoBehaviour (이펙트 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기화 | 1. playerCommonData 참조 획득 <br>2. stopEffectCoroutine = false로 초기화 | PlayerCommonData |

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 이펙트 중지 | 1. stopEffectCoroutine = true | - |

### TogglePlayerHitEffect(EffectStruct effectStruct, GameObject[] effects, Vector3 hitPosition)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 히트 이펙트 표시 | 1. useFunction 확인<br>2. 비활성 이펙트 찾기<br>3. waitTime 후 활성화<br>4. duration 후 비활성화<br>5. 위치 추적 (followPosition) | Player <br>PlayerCommonData |

### TogglePlayerEffect (3개 오버로드)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 이펙트 위치별 표시 | 1. **위치 오브젝트 기준:** positionObject 기준으로 위치 추적<br>2. **고정 위치 기준:** effectPosition에 이펙트 배치<br>3. **충돌점 기준:** 충돌점 위치 기반 이펙트<br>4. 모두 waitTime, duration, followPosition 처리 | Player <br>PlayerCommonData |

### ShowHitVignette(float duration)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피격 비네트 효과 표시 | 1. hitVignette 이펙트 활성화 <br>2. duration 시간 동안 표시 <br>3. duration 후 비활성화 <br>4. 피격 반응 시각적 피드백 제공 | GameObject |
