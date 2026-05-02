# PlayerHitBoxCollider 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerHitBoxCollider |
| 현재 역할 | 플레이어 공격 히트박스 충돌 처리<br>- 적 타격 감지<br>- 피해 정보 구성<br>- 효과 음향 재생 |
| 구현 디자인 패턴 | MonoBehaviour (충돌 감지) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### OnEnable()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 컴포넌트 활성화 시 초기화 | 1. 데이터 로드<br>2. 플래그 초기화 (canShakeCamera, canSetTimeScale, canPlayAudio = true) | PlayerCommonData <br>PlayerHumanMaskData <br>PlayerAnimalMaskData |

### SetMessage(Enemy enemy, string tagName, DamageMessage damageMessage)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피해 메시지 설정 | 1. tagName 확인 (HumanNormalAttack, HumanFirstSkill, etc) <br>2. 태그별 피해량 설정 <br>3. damageMessage 구조체 채우기 (데미지, 색상, 스택) <br>4. 적 고유 정보 저장 | DamageMessage <br>Enemy |

### OnCollisionEnter(Collision collision)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 공격 충돌 처리 | 1. collision 파라미터에서 충돌 상대 Collider 추출<br>2. 충돌 상대의 Enemy 컴포넌트 확인<br>3. Enemy 없으면 함수 조기 종료<br>4. 충돌 태그 이름 확인 (HumanNormalAttack, HumanFirstSkill 등)<br>5. SetMessage()로 해당 공격의 피해/색상/스택 설정<br>6. DamageMessage 구조체 생성<br>7. GameTimeScale.SetTimeScale() - 타임 스케일 조정 (슬로우 모션)<br>8. PlayerCameraEffect.ShakeCamera() - 카메라 쉐이크<br>9. PlayerSound.SetPlayerSound() - 공격음 재생<br>10. PlayerEffect로 타격 이펙트 활성화<br>11. Enemy.ApplyDamage(damageMessage) 호출로 적에 피해 전달<br>12. CalliSystem 체크로 페인트 시스템과 연동<br>13. 일회성 충돌 처리 (중복 피해 방지) | Collision <br>Collider <br>Enemy <br>DamageMessage <br>GameTimeScale <br>PlayerCameraEffect <br>PlayerSound <br>PlayerEffect <br>IDamageable |
