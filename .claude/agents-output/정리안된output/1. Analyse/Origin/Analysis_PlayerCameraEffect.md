# PlayerCameraEffect 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerCameraEffect |
| 현재 역할 | 플레이어 스킬 카메라 효과 관리<br>- 카메라 흔들림(shake) 효과<br>- 카메라 리컴포저(zoom, follow, lookat) 조정<br>- Cinemachine Impulse 기반 효과 구현 |
| 구현 디자인 패턴 | MonoBehaviour (카메라 이펙트 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 카메라 오브젝트 초기화 | 1. cameraObject 배열의 모든 카메라에 대해:<br>   - GetComponent<CinemachineRecomposer>()로 Recomposer 컴포넌트 획득<br>   - m_ZoomScale = 1<br>   - m_FollowAttachment = 1<br>   - m_LookAtAttachment = 1 (기본값으로 초기화) | CinemachineRecomposer |

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 사망 상태 확인 | 1. playerState.playerCurrentState == PlayerStateType.DEAD이면 조기 반환<br>2. 현재 구현에서는 실질적 작업 없음 | PlayerState |

### ShakeCamera(CameraShakeStruct cameraShake)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 함수 활성화 여부 확인 | 1. cameraShake.useFunction이 false면 조기 반환 | CameraShakeStruct |
| 활성 카메라 탐색 | 1. cameraObject 배열을 순회하며 activeSelf == true인 카메라 찾기<br>2. 찾은 카메라를 currentCamera에 할당 | GameObject |
| Secondary Noise 설정 | 1. currentCamera에서 CinemachineImpulseListener 획득<br>2. listener.m_ReactionSettings.m_SecondaryNoise 설정:<br>   - cameraShake.reactionType에 따라 secondaryNoise 배열에서 선택<br>3. m_AmplitudeGain = cameraShake.amplitudeGain<br>4. m_FrequencyGain = cameraShake.frequencyGain<br>5. m_Duration = cameraShake.reactionDuration | CinemachineImpulseListener <br>CameraShakeStruct |
| Impulse Shape 설정 | 1. cameraShake.shakeType에 따라 switch문으로 분기:<br>   - IMPULSE_RECOIL: Recoil<br>   - IMPULSE_BUMP: Bump<br>   - IMPULSE_EXPOLOSION: Explosion<br>   - IMPULSE_RUMBLE: Rumble<br>2. impulseSource.m_ImpulseDefinition.m_ImpulseShape 설정 | CinemachineImpulseDefinition |
| Impulse 생성 | 1. impulseSource.m_ImpulseDefinition.m_ImpulseDuration = cameraShake.impulseDuration<br>2. impulseSource.GenerateImpulseWithVelocity(cameraShake.impulseVelocty) 호출로 카메라 흔들림 실행 | CinemachineImpulseSource |

### ToggleCameraRecomposer(CameraRecomposerStruct cameraRecomposer)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 카메라 리컴포저 일시적 조정 | 1. cameraRecomposer.useFunction이 false면 코루틴 즉시 종료<br>2. cameraObject 배열 순회로 활성화된 카메라 탐색<br>3. 해당 카메라의 CinemachineRecomposer 컴포넌트 획득<br>4. waitTime만큼 대기(코루틴)<br>5. m_ZoomScale, m_FollowAttachment, m_LookAtAttachment 값 적용<br>6. duration만큼 설정값 유지<br>7. 시간 경과 후 모든 값을 기본값(1.0f, 1.0f, 1.0f)으로 복원<br>8. 스킬 실행 중 카메라 연출 효과 적용<br>9. 시간 기반 코루틴 처리로 부드러운 연출 | CinemachineRecomposer <br>CameraRecomposerStruct <br>Time |
