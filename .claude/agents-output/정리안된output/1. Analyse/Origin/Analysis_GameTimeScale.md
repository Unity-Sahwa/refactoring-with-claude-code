# GameTimeScale 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | GameTimeScale |
| 현재 역할 | 게임 시간 스케일 관리<br>- 시간 스케일 설정 및 복원<br>- 타이밍에 따른 시간 스케일 변경(코루틴)<br>- 치트 모드와의 상호작용 |
| 구현 디자인 패턴 | MonoBehaviour (게임 관리) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기값 설정 | 1. savedTimeScaleValue = 1로 기본값 설정<br>2. CameraController.instance에서 cameraController 획득 | CameraController |

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 게임 시간 스케일 초기화 | 1. cheatMode.isGameSlowed 여부 확인<br>2. 게임이 느려져있으면 조기 반환<br>3. 정상이면 Time.timeScale = 1로 원래 속도로 복원 | CheatMode |

### CoSetTimeScale(TimeScaleStruct timeScaleStruct)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 코루틴 활성화 여부 확인 | 1. timeScaleStruct.useFunction이 false면 코루틴 종료<br>2. cheatMode.isGameSlowed가 true면 코루틴 종료 | TimeScaleStruct <br>CheatMode |
| 시간 단위 판정 | 1. timeScaleStruct.useFrame이 true면:<br>   - waitTime = waitTimeFrames * Time.deltaTime<br>   - duration = durationFrames * Time.deltaTime<br>2. false면:<br>   - waitTime = waitTimeSeconds<br>   - duration = durationSeconds | TimeScaleStruct |
| 이전 타임스케일 저장 | 1. savedTimeScaleValue = Time.timeScale으로 현재 값 저장 | - |
| 대기 시간 처리 | 1. WaitForSecondsRealtime(waitTime)으로 대기<br>   (실시간 기반이므로 timeScale 영향 없음) | - |
| 타임스케일 변경 | 1. Time.timeScale = timeScaleStruct.timeScale으로 설정된 값으로 변경 | - |
| 지속 시간 처리 | 1. WaitForSecondsRealtime(duration)으로 대기<br>   (변경된 timeScale로 재생되는 동안의 대기) | - |
| 타임스케일 복원 | 1. Time.timeScale = 1로 원래 속도로 복원 | - |

### SetTimeScale(float timeScaleValue)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 게임 속도 즉시 제어 | 1. 입력받은 timeScaleValue 직접 할당<br>2. Time.timeScale에 즉시 반영<br>3. 코루틴 없이 순간적 적용<br>4. 범위: 0 (정지) ~ 1 (정상 속도)<br>5. 슬로우 모션/빠른 재생 실시간 제어<br>6. UI 및 효과음 함께 영향받음 | Time |
