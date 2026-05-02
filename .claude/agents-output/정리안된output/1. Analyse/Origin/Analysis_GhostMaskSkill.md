# GhostMaskSkill 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | GhostMaskSkill |
| 현재 역할 | 유령 마스크 피니시 스킬 관리<br>- 타겟 감지<br>- 피니시 스킬 실행<br>- 적 제거 연출 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기화 및 참조 설정 | 1. 기본 데이터 로드<br>2. skillCameraAnimator 캐싱<br>3. finishTargetList 생성<br>4. canUseFinishSkill = true<br>5. 무기 비활성화 | PlayerController <br>Player <br>PlayerCommonData |

### InitializeSkill()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 상태 초기화 | 1. 코루틴 중지<br>2. 스킬 상태 초기화<br>3. 카메라 DEFAULT로 변경 | PlayerSkillMove <br>PlayerEffect <br>PlayerState <br>PlayerSound <br>CameraController |

### DetectTargetToFinish()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 타겟 감지 및 HUD 활성화 | 1. Physics.OverlapSphere로 범위 내 적 검색<br>2. 조건에 맞는 적 필터링<br>3. finishTargetList 업데이트<br>4. SkillHUD.ActivateFinishHUD() 호출 | MaskChange <br>Physics <br>CameraData <br>Enemy <br>CalliSystem <br>SkillHUD |

### CheckEnableFinish()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피니시 스킬 실행 가능 확인 | 1. canUseFinishSkill 확인 <br>2. finishTargetList.Count > 0 확인 <br>3. 두 조건 모두 만족 시 true 반환 <br>4. 피니시 스킬 실행 가능 여부 판단 | 없음 |

### Finish()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피니시 스킬 입력 처리 | 1. CheckEnableFinish() 확인<br>2. InitializeSkill() 호출<br>3. CoFinish() 코루틴 시작<br>4. canUseFinishSkill = false | - |

### CoFinish()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피니시 스킬 메인 코루틴 | 1. 무적 활성화<br>2. 카메라 변경<br>3. 플레이어 상태 설정<br>4. 적 NavMeshAgent 정지<br>5. 휴먼/동물별 애니메이션 재생<br>6. 시간 기반 무기 전환<br>7. 시간 기반 적 제거<br>8. 스킬 이동, 이펙트, 사운드, 카메라 쉐이크 처리<br>9. 타임 스케일 조정 | MaskChange <br>PlayerState <br>PlayerAnimation <br>Physics <br>Enemy <br>PlayerSound <br>PlayerSkillMove <br>PlayerEffect <br>PlayerCameraEffect <br>GameTimeScale <br>CameraController <br>HpHUD <br>Player |

### SkillCameraAnimation()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 카메라 애니메이션 재생 | 1. skillCameraAnimator.SetTrigger() 호출 <br>2. 피니시 스킬 실행 시 카메라 연출 애니메이션 시작 <br>3. 카메라 연출을 통한 시각적 강조 | Animator |

### FinishSkillCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피니시 스킬 쿨다운 관리 | 1. canUseFinishSkill 확인<br>2. 경과 시간 계산<br>3. SkillHUD 쿨다운 표시<br>4. 쿨다운 만료 시 canUseFinishSkill = true | SkillHUD <br>PlayerGhostMaskData |
