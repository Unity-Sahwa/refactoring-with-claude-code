# CameraController 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | CameraController |
| 현재 역할 | 게임의 카메라 시스템 총괄 관리<br>- Cinemachine 기반 카메라 전환<br>- 타겟 감지 및 락온(Lock-On) 기능<br>- 타겟 마커 및 아웃라인 렌더링 관리<br>- 해상도 및 플랫폼별 설정 조정 |
| 구현 디자인 패턴 | 싱글톤 패턴 (instance를 통한 전역 접근) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance가 null이면 현재 객체를 instance에 할당<br>2. instance가 이미 존재하면 현재 게임오브젝트 제거 | - |
| 해상도 조정 | 1. AdjustResolution() 호출로 디바이스별 해상도 설정 | AdjustResolution() |
| 프레임레이트 설정 | 1. Application.targetFrameRate = 60으로 고정 | - |
| 락온 카메라 초기화 | 1. lockOnCamera 게임오브젝트 활성화 (Start에서 비활성화될 예정) | lockOnCamera |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 카메라 시스템 초기화 | 1. CameraInitialSet() 메서드 호출<br>2. 외부 참조(PlayerController, SaveManager 등) 로드<br>3. Cinemachine 컴포넌트 캐싱<br>4. 타겟 마커 초기 색상 설정<br>5. 플랫폼별 마우스 속도 로드<br>6. HoldDefaultCameraValue() 코루틴 시작<br>7. 게임 시작 후 카메라 설정 완료 | CameraInitialSet() |

### Update()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 매프레임 카메라 로직 | 1. 플레이어 상태가 GHOST_FINISHSKILL이면 조기 반환<br>2. DetectTargetAlways() - 타겟 감지<br>3. CheckCurrentTargetState() - 현재 타겟 상태 확인<br>4. ControlTargetMarker() - 마커 위치 업데이트<br>5. CheckOutlineTarget() - 아웃라인 대상 확인<br>6. CameraHorizontalSwipe() - 조이스틱 입력 처리 | PlayerState <br>DetectTargetAlways() <br>CheckCurrentTargetState() <br>ControlTargetMarker() <br>CheckOutlineTarget() <br>CameraHorizontalSwipe() |

### CameraInitialSet()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 외부 참조 초기화 | 1. PlayerController에서 maskChange, player 획득<br>2. Singleton에서 commonData, cameraData 획득 | PlayerController <br>PlayerCommonData <br>CameraData |
| Cinemachine 컴포넌트 캐싱 | 1. lockOnCamera에서 CinemachineTransposer 추출<br>2. lockOnCamera에서 CinemachineGroupComposer 추출 | CinemachineTransposer <br>CinemachineGroupComposer |
| 카메라 활성화 설정 | 1. defaultCamera 활성화<br>2. lockOnCamera 비활성화 | defaultCamera <br>lockOnCamera |
| 타겟 마커 초기화 | 1. targetMarker 활성화<br>2. detectedTargetMarkerColor로 색상 설정 | CameraData |
| 마우스 속도 설정 | 1. 메인메뉴 활성화 여부 확인<br>2. 메인메뉴 활성화시: 회전속도 0으로 설정<br>3. 메인메뉴 비활성화시: saveManager에서 저장된 속도값 로드 | MenuUI <br>SaveManager |
| 카메라 초기값 유지 | 1. HoldDefaultCameraValue() 코루틴 시작 | HoldDefaultCameraValue() |

### AdjustResolution()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플랫폼별 목표 해상도 설정 | 1. #if UNITY_ANDROID 확인<br>2. Android: 1280x720 설정<br>3. 그 외: 1920x1080 설정 | - |
| 디바이스 해상도 계산 | 1. Screen.width/height로 현재 해상도 비율 계산<br>2. 목표 픽셀 수 계산(targetWidth * targetHeight)<br>3. 디바이스 비율에 맞게 조정된 높이 계산<br>4. 조정된 너비 계산 | Screen |
| 해상도 적용 | 1. Screen.SetResolution()으로 전체화면 해상도 설정<br>2. Debug.Log로 적용된 해상도 출력 | Screen |

### SetPCPlatform(bool isSet)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| PC 플랫폼 입력 설정 | 1. isSet이 true면:<br>   - m_YAxis.m_InputAxisName = "Mouse Y"<br>   - m_XAxis.m_InputAxisName = "Mouse X"<br>2. isSet이 false면:<br>   - 두 입력축명을 빈 문자열로 설정 | defaultCamera |

### ChangeCamera(CameraType cameraType)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 락온 카메라 전환 | 1. CameraType.LOCKON일 때:<br>   - defaultCamera 비활성화<br>   - lockOnCamera 활성화<br>   - finishSkillCamera 비활성화<br>   - HoldLockOnCameraValue() 코루틴 시작 | lockOnCamera <br>HoldLockOnCameraValue() |
| 피니시스킬 카메라 전환 | 1. CameraType.FINISHSKILL일 때:<br>   - 모든 카메라 비활성화 후 finishSkillCamera만 활성화 | finishSkillCamera |
| 기본 카메라 전환 | 1. 그 외(CameraType.DEFAULT):<br>   - defaultCamera 활성화<br>   - 나머지 카메라 비활성화<br>   - PC 플랫폼이 아니면 HoldDefaultCameraValue() 코루틴 시작 | defaultCamera <br>PlatformSwitcher <br>HoldDefaultCameraValue() |

### HoldDefaultCameraValue()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 보스 씬 카메라 설정 | 1. SceneManager에서 현재 씬 빌드 인덱스 확인(4 = 보스씬)<br>2. 3개 Rig의 CinemachineComposer 컴포넌트 추출<br>3. 보스씬이면 각 Rig별 궤도값(m_Orbits) 및 오프셋 설정:<br>   - Top Rig: Height=10, Radius=15, Offset.y=4.7f<br>   - Middle Rig: Height=5, Radius=15, Offset.y=2.5f<br>   - Bottom Rig: Height=1, Radius=11, Offset.y=1.4f | SceneManager <br>CinemachineComposer <br>defaultCamera |
| 일반 씬 카메라 설정 | 1. 보스씬이 아니면 m_YAxis.Value = 0.5f로 설정 | defaultCamera |
| 카메라 활성 상태 모니터링 | 1. 무한 루프에서 Time.deltaTime 누적<br>2. defaultCamera가 활성화되지 않으면 루프 종료<br>3. 2초 경과 후 루프 종료 | defaultCamera |
| 코루틴 대기 | 1. yield return null로 매프레임 대기 | - |

### HoldLockOnCameraValue()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 보스/일반 씬별 카메라 오프셋 설정 | 1. 보스씬(buildIndex==4) 판정<br>2. 보스씬: lockOnCameraFollowOffset_4, lockOnCameraTrackedObjectOffset_4 사용<br>3. 일반씬: lockOnCameraFollowOffset, lockOnCameraTrackedObjectOffset 사용<br>4. 각각을 lockOnCameraTransposer, lockOnCameraGroupComposer에 적용 | SceneManager <br>lockOnCamera <br>CinemachineTransposer <br>CinemachineGroupComposer |
| 카메라 활성 상태 모니터링 | 1. 무한 루프에서 Time.deltaTime 누적<br>2. lockOnCamera가 활성화되지 않으면 루프 종료<br>3. 2초 경과 후 루프 종료 | lockOnCamera |
| 코루틴 대기 | 1. yield return null로 매프레임 대기 | - |

### SetMouseSpeed(bool isXAxis, float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| X축 마우스 속도 설정 | 1. isXAxis가 true면 defaultCamera.m_XAxis.m_MaxSpeed = value | defaultCamera |
| Y축 마우스 속도 설정 | 1. isXAxis가 false면 defaultCamera.m_YAxis.m_MaxSpeed = value | defaultCamera |

### GetMouseSpeed(bool isXAxis)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| X축 마우스 속도 조회 | 1. isXAxis가 true면 defaultCamera.m_XAxis.m_MaxSpeed 반환 | defaultCamera |
| Y축 마우스 속도 조회 | 1. isXAxis가 false면 defaultCamera.m_YAxis.m_MaxSpeed 반환 | defaultCamera |

### SetBossRoomCamera()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 보스룸 카메라 설정 | 1. 3개 Rig 궤도값 직접 설정:<br>   - m_Orbits[0]: Height=10, Radius=15<br>   - m_Orbits[1]: Height=5, Radius=15<br>   - m_Orbits[2]: Height=1, Radius=11<br>2. 각 Rig의 CinemachineComposer TrackedObjectOffset.y 설정:<br>   - Rig0: 4.7f<br>   - Rig1: 2.5f<br>   - Rig2: 1.4f | defaultCamera <br>CinemachineComposer |

### DetectTargetAlways()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 감지 초기화 | 1. visibleTarget을 null로 초기화<br>2. 플레이어 상태가 DEAD면 조기 반환 | PlayerState |
| 범위 내 콜라이더 탐색 | 1. Physics.OverlapSphere()로 감지범위 내 모든 콜라이더 검색<br>2. maskChange.CurrentMask.position 중심으로 detectRange 반경 검색<br>3. enemyLayer 마스크 적용 | Physics <br>MaskChange <br>CameraData |
| 감지 결과 판정 | 1. 콜라이더 배열 길이 확인<br>2. 콜라이더 없으면: headTransform=null, isTargetDetected=false<br>3. 콜라이더 있으면: UIEffect.ShowPlayerHUDFadeEffect() 호출, isTargetDetected=true | CameraData <br>UIEffect |
| 시각 범위 내 최적 타겟 선택 | 1. 각 콜라이더에 대해:<br>   - Enemy 컴포넌트 확인 및 isDead 체크<br>   - 카메라 기준 각도 계산(Vector3.Angle)<br>   - 캐릭터 기준 거리 계산(Vector3.Distance)<br>   - 최대 각도(maximumAngleWithTarget) 초과 콜라이더 제외<br>   - 최대 거리(maximumDistanceWithTarget) 초과 콜라이더 제외<br>2. CalliSystem 컴포넌트 확인 및 Paint 오버플로우 체크<br>3. 거리 기반 정렬: 가장 가까운 타겟 우선 선택<br>4. 거리 내에 없으면 각도 기반으로 가장 중앙에 있는 타겟 선택<br>5. 첫 락온 타겟일 경우 HeadPosition 트랜스폼 할당 | Enemy <br>CalliSystem <br>CameraData <br>PlayGuide |

### CheckOutlineTarget()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 아웃라인 제거 | 1. visibleTarget이 null이면 ClearOutline() 호출 | ClearOutline() |
| 새 아웃라인 적용 | 1. visibleTarget이 있고 outlineTarget이 null이면 DrawOutlineOnTarget() 호출 | DrawOutlineOnTarget() |
| 아웃라인 유지 | 1. visibleTarget과 outlineTarget이 같은 부모 게임오브젝트면 그대로 유지 | - |
| 아웃라인 교체 | 1. visibleTarget과 outlineTarget이 다른 게임오브젝트면 ClearOutline() 호출 | ClearOutline() |

### DrawOutlineOnTarget()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 특수 타겟 판정 | 1. visibleTarget에 BrokenObject 또는 WisuSuppressionController 컴포넌트 확인 | BrokenObject <br>WisuSuppressionController |
| 아웃라인 자식 탐색 | 1. visibleTarget의 모든 자식 게임오브젝트 순회<br>2. "OutlineTarget" 태그를 가진 자식 찾기 | - |
| 아웃라인 타겟 저장 | 1. 찾은 자식 게임오브젝트를 outlineTarget에 할당 | - |
| 기존 머티리얼 캐싱 | 1. outlineTarget에서 MeshRenderer 추출<br>2. 기존 materials 배열을 targetMaterials에 저장 | MeshRenderer |
| 아웃라인 머티리얼 추가 | 1. 기존 머티리얼 개수 + 1 크기의 새 배열 생성<br>2. 기존 머티리얼들을 새 배열에 복사<br>3. 마지막 인덱스에 outlineMaterial 할당<br>4. MeshRenderer.materials에 새 배열 할당 | MeshRenderer <br>outlineMaterial |

### ClearOutline()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 아웃라인 제거 | 1. outlineTarget이 null이면 조기 반환<br>2. outlineTarget에서 MeshRenderer 추출<br>3. MeshRenderer.materials를 원래 targetMaterials로 복원 | MeshRenderer |
| 상태 초기화 | 1. targetMaterials = null<br>2. outlineTarget = null | - |

### InvokeFinishGuide()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 튜토리얼 UI 표시 | 1. PlayGuide.instance.ShowTutorialUI() 호출<br>2. TutorialState.FINISHATTACK 파라미터 전달 | PlayGuide <br>TutorialState |

### CheckCurrentTargetState()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 락온 활성화 상태 확인 | 1. isLockOnTarget이 true 여부 확인 | - |
| 현재 타겟 유효성 검증 | 1. currentTarget이 null인지 확인<br>2. currentTarget.IsDestroyed() 확인<br>3. Enemy 컴포넌트 확인 및 isDead 체크<br>4. enemy.hpBarCount <= 0 확인 | Enemy |
| 새 타겟 자동 전환 | 1. 타겟이 유효하지 않고 visibleTarget이 있으면:<br>   - targetGroup에서 이전 targetTransform 제거<br>   - currentTarget = visibleTarget 할당<br>   - visibleTarget = null<br>   - HeadPosition 트랜스폼 탐색<br>   - headTransform이 없으면 currentTarget.transform 사용, 있으면 headTransform 사용<br>   - targetGroup.AddMember()로 새 타겟 추가 | targetGroup <br>Enemy |
| 타겟 범위 이탈 확인 | 1. 타겟과의 거리가 detectRange 초과하면 DeactivateLockOn() 호출 | CameraData <br>DeactivateLockOn() |
| 타겟 없을 시 락온 해제 | 1. visibleTarget이 없으면 DeactivateLockOn() 호출 | DeactivateLockOn() |

### DeactivateLockOn()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 타겟 그룹 제거 | 1. targetGroup.RemoveMember(targetTransform) 호출 | targetGroup |
| 플레이어 애니메이션 업데이트 | 1. maskChange.CurrentAnimator.SetBool("isFocused", false) 호출 | MaskChange |
| 락온 상태 초기화 | 1. isLockOnTarget = false<br>2. currentTarget = null<br>3. targetTransform = null<br>4. headTransform = null | - |
| 카메라 전환 | 1. ChangeCamera(CameraType.DEFAULT) 호출 | ChangeCamera() |

### ControlTargetMarker()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 락온 타겟 마커 표시 | 1. currentTarget이 존재하면:<br>   - targetTransform이 CapsuleCollider를 가진지 확인<br>   - targetMarker 활성화<br>2. headTransform이 있으면 headTransform.position 사용<br>3. headTransform이 없으면:<br>   - CapsuleCollider 추출<br>   - localScale과 center, height로 타겟 높이 계산<br>   - 높이를 반영한 위치에 targetMarkerOffset 추가<br>4. MainCamera.WorldToScreenPoint()로 월드좌표를 화면좌표로 변환<br>5. targetMarker 색상을 LockOnTargetMarkerColor로 설정 | CapsuleCollider <br>CameraData <br>MainCamera |
| 감지 타겟 마커 표시 | 1. visibleTarget이 존재하면:<br>   - visibleTarget이 CapsuleCollider를 가진지 확인<br>   - targetMarker 활성화<br>2. headTransform이 있으면 headTransform.position 사용<br>3. headTransform이 없으면 위와 동일한 높이 계산 로직<br>4. 화면좌표로 변환 후 마커 위치 설정<br>5. targetMarker 색상을 detectedTargetMarkerColor로 설정 | CapsuleCollider <br>CameraData <br>MainCamera |
| 타겟 없을 시 마커 숨김 | 1. currentTarget과 visibleTarget이 모두 없으면 targetMarker 비활성화 | - |

### LockOnTarget()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 락온 토글 | 1. isLockOnTarget이 true면:<br>   - DeactivateLockOn() 호출로 락온 해제<br>   - 메서드 반환 | DeactivateLockOn() |
| 새 타겟 락온 활성화 | 1. visibleTarget이 있으면:<br>   - isLockOnTarget = true<br>   - currentTarget = visibleTarget<br>   - visibleTarget = null<br>   - maskChange.CurrentAnimator.SetBool("isFocused", true) 호출<br>   - HeadPosition 트랜스폼 탐색<br>   - targetMarker 스케일을 targetMarkerScale로 설정<br>   - targetTransform이 null이면 currentTarget.transform 사용, 있으면 그 값 사용<br>   - targetGroup.AddMember(targetTransform, 1f, 1) 호출<br>   - ChangeCamera(CameraType.LOCKON) 호출로 카메라 전환 | visibleTarget <br>MaskChange <br>CameraData <br>targetGroup <br>ChangeCamera() |

### CameraHorizontalSwipe()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 조이스틱 수평 입력 처리 | 1. joystick.Horizontal이 0이면 조기 반환<br>2. 0이 아니면 defaultCamera.m_XAxis.Value에 (joystick.Horizontal * cameraHorizontalSwipeRate) 더하기<br>3. 결과적으로 조이스틱 입력으로 카메라 좌우 회전 조절 | VariableJoystick <br>defaultCamera <br>CameraData |

