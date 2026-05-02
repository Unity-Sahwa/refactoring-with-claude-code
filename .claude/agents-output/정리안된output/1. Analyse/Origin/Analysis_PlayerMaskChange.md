# PlayerMaskChange 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerMaskChange |
| 현재 역할 | 플레이어 마스크 전환 관리<br>- 휴먼/동물/유령 마스크 전환<br>- 마스크 오브젝트 활성화/비활성화<br>- 마스크 변경 쿨다운 관리<br>- 캐릭터 상태 동기화 |
| 구현 디자인 패턴 | 싱글톤 패턴 |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance가 null이면 현재 객체를 instance로 설정<br>2. instance가 이미 존재하면 현재 게임오브젝트 제거 | - |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 데이터 및 상태 초기화 | 1. PlayerCommonData.Instance 획득<br>2. PlayerHumanMaskData.Instance 획득<br>3. PlayerAnimalMaskData.Instance 획득<br>4. canUseChangeMask = true (마스크 변경 가능)<br>5. InitialSetUp() 호출로 캐릭터 초기화 | PlayerCommonData <br>PlayerHumanMaskData <br>PlayerAnimalMaskData |

### InitialSetUp()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 캐릭터 오브젝트 초기화 | 1. 자식 오브젝트에서 휴먼/동물 마스크 GameObjects 획득<br>2. 각 마스크의 Animator, Rigidbody 컴포넌트 캐싱<br>3. CurrentMask를 휴먼으로 초기 설정<br>4. ChangeMask(MaskType.HUMAN, false, false) 호출로 기본 마스크 적용 | - |

### ChangeCharacter()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 캐릭터 오브젝트 변경 (휴먼 ↔ 동물) | 1. canUseChangeMask 확인 (변경 불가 상태 체크)<br>2. canUseChangeMask = false (변경 불가 설정)<br>3. lastTimeCastedChangeMask = Time.time (시간 기록)<br>4. 현재 활성 캐릭터의 위치/회전을 새 캐릭터로 동기화<br>5. CurrentMask 참조 업데이트<br>6. 스킬 객체들 (HumanMaskSkill, AnimalMaskSkill) Initialize() 호출<br>7. 이전 캐릭터 비활성화, 새 캐릭터 활성화<br>8. CameraController.LockOnTarget() 상태에 따라 Animator.isFocused 설정 | CameraController |

### ChangeMask(MaskType maskType, bool useFunction, bool useSound)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 마스크 오브젝트 변경 및 효과 재생 | **1단계 UI 업데이트:**<br>1. SkillHUD.ChangeIcon(maskType) 호출로 스킬 아이콘 변경<br><br>**2단계 마스크 객체 활성화:**<br>2. MaskType에 따라 해당 오브젝트만 활성화:<br>   - HUMAN: humanMask 활성화, 나머지 비활성화<br>   - ANIMAL: animalMask 활성화, 나머지 비활성화<br>   - GHOST: ghostMask 활성화, 나머지 비활성화<br><br>**3단계 변경 효과 (useFunction=true):**<br>3. PlayerEffect.TogglePlayerEffect() - 방사형 이펙트 재생<br>4. OrbitPartnerMask.HandCollisionOnOff() - 충돌 처리<br><br>**4단계 음향 효과 (useSound=true):**<br>5. PlayerSound.SetPlayerSound() - 마스크 변경 사운드 재생 | SkillHUD <br>PlayerEffect <br>PlayerSound <br>OrbitPartnerMask <br>Player |

### ChangeMaskCooldown()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 마스크 변경 쿨다운 관리 | 1. canUseChangeMask == false 확인 (쿨다운 중)<br>2. Time.time - lastTimeCastedChangeMask >= commonData.changeMaskCoolTime 확인<br>3. 쿨다운 종료 시 canUseChangeMask = true 설정<br>4. ChangeCharacter() 호출 시에만 쿨다운 시작 | PlayerCommonData |
