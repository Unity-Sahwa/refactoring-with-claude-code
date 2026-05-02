# PlayerSkill 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerSkill |
| 현재 역할 | 플레이어 스킬의 기본 추상 클래스<br>- 스킬 실행에 필요한 공통 필드/메서드 제공<br>- 이펙트 제어 및 위치 설정 유틸리티<br>- 스킬 서브클래스들의 부모 클래스 |
| 구현 디자인 패턴 | 추상 클래스 (Abstract Class) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### StartSet()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 일회성 초기화 확인 | 1. activeStartOnce가 false 여부 확인<br>2. true면 이미 초기화되었으므로 메서드 반환<br>3. false면 초기화 진행 | - |
| 플레이어 관련 객체 초기화 | 1. PlayerController.instance에서 playerController 획득<br>2. playerController.playerMovement에서 playerMovement 획득<br>3. playerController.maskChange에서 maskChange 획득<br>4. Player.instance에서 player 획득 | PlayerController <br>PlayerMovement <br>MaskChange <br>Player |
| 카메라 및 UI 초기화 | 1. CameraController.instance에서 cameraController 획득<br>2. UIEffect.instance에서 UIEffect 획득 | CameraController <br>UIEffect |
| 데이터 객체 초기화 | 1. PlayerCommonData.Instance에서 commonData 획득<br>2. PlayerHumanMaskData.Instance에서 humanData 획득<br>3. PlayerAnimalMaskData.Instance에서 animalData 획득<br>4. CameraData.Instance에서 cameraData 획득 | PlayerCommonData <br>PlayerHumanMaskData <br>PlayerAnimalMaskData <br>CameraData |
| 초기화 완료 표시 | 1. activeStartOnce = true로 설정하여 중복 초기화 방지 | - |

### ControlObject(ref GameObject controlObject, ref bool activeOnce, ref bool inActiveOnce, float gameTime = 0, float startTime = 0, float waitTime = 99, float duration = 99)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 게임오브젝트 활성화 타이밍 | 1. gameTime >= startTime + waitTime 조건 확인<br>2. activeOnce가 false이고 조건 만족하면:<br>   - controlObject.SetActive(true)로 활성화<br>   - activeOnce = true로 표시<br>3. 일회성 활성화 보장 | GameObject |
| 게임오브젝트 비활성화 타이밍 | 1. gameTime >= startTime + waitTime + duration 조건 확인<br>2. inActiveOnce가 false이고 조건 만족하면:<br>   - controlObject.SetActive(false)로 비활성화<br>   - inActiveOnce = true로 표시<br>3. 일회성 비활성화 보장 | GameObject |
| 파라미터 설명 | - gameTime: 현재 게임 시간<br>- startTime: 시작 기준 시간<br>- waitTime: 대기 시간 (활성화 전)<br>- duration: 지속 시간 (활성화 후)<br>- ref 파라미터: 외부에서 상태 추적 가능 | - |

### SetEffectPosition(ref GameObject effectObject, Transform effectPosition)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 이펙트 위치 설정 | 1. effectObject.transform.position = effectPosition.transform.position으로 위치 동기화 | GameObject <br>Transform |
| 이펙트 회전 설정 | 1. effectObject.transform.rotation = effectPosition.transform.rotation으로 회전 동기화 | GameObject <br>Transform |
| 목적 | 이펙트 게임오브젝트를 지정된 트랜스폼 위치/회전과 동일하게 설정 | - |
