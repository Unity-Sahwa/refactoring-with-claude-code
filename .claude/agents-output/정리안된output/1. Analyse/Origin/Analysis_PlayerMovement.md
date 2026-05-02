# PlayerMovement 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerMovement |
| 현재 역할 | 플레이어 입력 기반 이동 및 회전 관리<br>- 플랫폼별 입력 처리 (PC/모바일)<br>- 캐릭터 회전 및 이동<br>- 이동 제약 처리<br>- 중력 적용 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance 설정 (중복 방지) | PlayerController |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 참조 획득 및 초기화 | 1. SaveManager.instance 캐싱<br>2. PlayerController.maskChange 획득<br>3. PlayerCommonData.Instance 획득 | SaveManager <br>PlayerController <br>PlayerCommonData |

### InputMovementValue(float moveValue, bool isHorizontal)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력값 가속화 처리 | **파라미터:** moveValue(현재값), isHorizontal(수평/수직)<br>**반환값:** 가속된 입력값(-1~1)<br><br>**처리 방식:**<br>1. SignType 배열로 동시 입력 처리 (Positive, Negative)<br>2. 해당 방향의 입력 상태 추적<br>3. 배열 순회로 가장 최근 입력값 선택<br>4. commonData.inputIncreaseSpeed로 가속화<br>5. 입력 없을 때 감속 처리<br>6. Clamp(-1, 1)로 범위 제한 | SaveManager <br>PlayerCommonData |

### InputMovement()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플랫폼별 입력 처리 및 애니메이터 설정 | **1단계 플랫폼별 입력:**<br>1. PC: InputMovementValue() 호출<br>2. 모바일: Joystick 입력 획득<br>3. 모바일 카메라 리센터링 설정<br><br>**2단계 이동량 계산:**<br>4. moveAmount = Vector3(moveHorizontal, moveVertical).magnitude로 정규화<br>5. movement 벡터 계산 (정규화된 이동값)<br>6. 카메라 방향 기반 월드 좌표 변환<br><br>**3단계 락온 타겟 처리:**<br>7. CameraController.currentTarget 존재 시 방향 조정<br><br>**4단계 이동 속도 설정:**<br>8. 휴먼/동물별 이동 속도 차이 적용<br><br>**5단계 애니메이터 업데이트:**<br>9. animator.SetFloat("moveAmount", moveAmount)<br>10. animator.SetFloat("moveHorizontal", moveHorizontal)<br>11. animator.SetFloat("moveVertical", moveVertical) | CameraController <br>MaskChange <br>PlayerState <br>CheatMode <br>PlayerCommonData |

### CharacterRotate()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 캐릭터 회전 제어 | **락온 타겟 있음:**<br>1. Vector3.Slerp로 부드러운 회전<br>2. commonData.rotationSpeed로 회전 속도 제어<br><br>**락온 타겟 없음:**<br>3. Vector3.RotateTowards로 정해진 속도로 회전<br>4. commonData.rotationSpeed 적용 | CameraController <br>MaskChange <br>PlayerCommonData |

### CharacterMove()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 캐릭터 위치 이동 | **1단계 이동 속도 설정:**<br>1. 마스크별 이동 속도 획득 (humanSpeed, animalSpeed)<br>2. CheatMode.speedMultiplier 적용<br><br>**2단계 이동 제약 확인:**<br>3. PlayerSensor로 전방 이동 가능 확인<br>4. 후방 이동 가능 확인<br><br>**3단계 치트 모드 비행:**<br>5. CheatMode.flyingSpeed로 비행 속도 계산<br><br>**4단계 위치 업데이트:**<br>6. Rigidbody.MovePosition(newPosition)으로 위치 이동 | MaskChange <br>PlayerSensor <br>CheatMode <br>PlayerCommonData |

### AddGravity()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 중력 물리 효과 적용 | 1. PlayerCommonData.additionalGravity에서 중력 가속도 값 획득<br>2. currentVelocity.y -= additionalGravity * Time.deltaTime으로 매프레임 감소<br>3. 최대 낙하 속도 제한 (음수 최대값)<br>4. 누적 낙하 거리 기록<br>5. 지면 접촉 시 currentVelocity.y = 0으로 초기화<br>6. PlayerSensor의 raycast 결과로 지면 접촉 판정<br>7. 실시간 중력 가속(Time.fixedDeltaTime 기반)<br>8. 자유낙하, 점프, 스킬 이동 중 모두 작용 | Physics <br>Time <br>PlayerCommonData <br>PlayerSensor <br>Rigidbody |
