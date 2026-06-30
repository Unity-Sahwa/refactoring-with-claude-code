# 카메라 시스템 분석

> 대상 원본: `Scripts-Origin/Camera/`(CameraController 749줄, CameraData, SetBossRoomCamera), `Scripts-Origin/Player/Control/PlayerFunction/PlayerCameraEffect.cs`
> 형식: analysis-origin (수집 → 분석) + 카메라 사용 흐름

---

## 기존 코드 위치 및 역할

### CameraController
| 위치 | 역할 |
| --- | --- |
| [CameraController.Awake()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 자기 자신을 전역 인스턴스로 등록하고, 해상도를 맞추고 프레임 상한을 60으로 두고 락온 카메라를 켠다. |
| [CameraController.Start()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 카메라 초기 세팅 메서드를 호출한다. |
| [CameraController.Update()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 처형 상태가 아니면 매 프레임 "적 탐지 → 락온 대상 점검 → 마커 표시 → 외곽선 점검 → 좌우 스와이프"를 순서대로 호출한다. |
| [CameraController.CameraInitialSet()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 플레이어·데이터 참조와 카메라 부품을 잡고, 마커를 켜고, 메뉴 여부에 따라 마우스 속도를 정하고, 기본 카메라 값 고정 코루틴을 시작한다. |
| [CameraController.AdjustResolution()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 플랫폼별 목표 픽셀 수에 맞춰 화면 해상도를 계산해 전체화면으로 설정한다. |
| [CameraController.SetPCPlatform()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | PC 여부를 받아, 기본 카메라의 회전 입력축(Mouse X/Y)을 연결하거나 끊는다. |
| [CameraController.ChangeCamera()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 카메라 종류(기본/락온/처형)를 받아 해당 가상 카메라만 켜고 나머지를 끄며, 필요하면 값 고정 코루틴을 시작한다. |
| [CameraController.HoldDefaultCameraValue()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 2초 동안 기본 카메라의 궤도 높이·반경·오프셋을 보스씬/일반에 맞는 값으로 계속 고정한다(코루틴). |
| [CameraController.HoldLockOnCameraValue()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 2초 동안 락온 카메라의 따라가기·바라보기 오프셋을 보스씬/일반에 맞는 값으로 고정한다(코루틴). |
| [CameraController.SetMouseSpeed()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 축(X/Y)과 값을 받아 기본 카메라의 해당 축 최대 회전 속도를 설정한다. |
| [CameraController.GetMouseSpeed()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 축(X/Y)을 받아 기본 카메라의 현재 최대 회전 속도를 돌려준다. |
| [CameraController.SetBossRoomCamera()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 기본 카메라의 궤도 3개(상·중·하)를 보스룸용 높이·반경·오프셋으로 한 번에 설정한다. |
| [CameraController.DetectTargetAlways()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 매 프레임 플레이어 주변을 구체로 훑어 적을 찾고, 시야각·거리 조건으로 조준할 적 하나를 골라 두며, 적이 있으면 전투 HUD를 띄우고 머리 위치를 찾는다. |
| [CameraController.CheckOutlineTarget()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 조준 대상이 없으면 외곽선을 지우고, 새로 생기거나 바뀌면 그리거나 다시 그리도록 분기한다. |
| [CameraController.DrawOutlineOnTarget()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 조준 대상의 자식 중 외곽선용 오브젝트를 찾아, 그 메시 재질 배열 끝에 외곽선 재질을 끼워 넣는다. |
| [CameraController.ClearOutline()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 외곽선을 넣었던 메시의 재질을 원래대로 되돌린다. |
| [CameraController.InvokeFinishGuide()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 처형 공격 튜토리얼 UI를 띄운다. |
| [CameraController.CheckCurrentTargetState()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 락온 중일 때 고정 대상이 죽거나 사라지면 주변의 다른 적으로 자동 옮기고, 너무 멀어지면 락온을 푼다. |
| [CameraController.DeactivateLockOn()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 락온을 해제한다 — 타겟 그룹에서 빼고, 애니의 집중 플래그를 끄고, 상태를 비우고, 기본 카메라로 되돌린다. |
| [CameraController.ControlTargetMarker()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 고정 대상이나 조준 대상의 머리 위 지점을 화면 좌표로 바꿔 마커를 그 자리에 놓고 색을 정한다(대상 없으면 끔). |
| [CameraController.LockOnTarget()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 락온 토글 — 이미 락온이면 풀고, 조준 대상이 있으면 그 적을 고정 대상으로 삼아 타겟 그룹에 넣고 락온 카메라로 전환한다. |
| [CameraController.CameraHorizontalSwipe()](../../../../Scripts-Origin/Camera/Control/CameraController.cs) | 조이스틱의 좌우 입력만큼 기본 카메라의 가로 회전값을 더한다(모바일 스와이프). |

### PlayerCameraEffect
| 위치 | 역할 |
| --- | --- |
| [PlayerCameraEffect.Start()](../../../../Scripts-Origin/Player/Control/PlayerFunction/PlayerCameraEffect.cs) | 모든 카메라의 줌·따라가기·바라보기 보정값(Recomposer)을 기본값 1로 초기화한다. |
| [PlayerCameraEffect.Initialize()](../../../../Scripts-Origin/Player/Control/PlayerFunction/PlayerCameraEffect.cs) | 죽은 상태면 아무 것도 하지 않는다(현재 사실상 빈 동작). |
| [PlayerCameraEffect.ShakeCamera()](../../../../Scripts-Origin/Player/Control/PlayerFunction/PlayerCameraEffect.cs) | 흔들림 정보를 받아, 지금 켜진 카메라에 2차 노이즈와 임펄스(반동·충격·폭발·진동)를 설정해 카메라를 흔든다. |
| [PlayerCameraEffect.ToggleCameraRecomposer()](../../../../Scripts-Origin/Player/Control/PlayerFunction/PlayerCameraEffect.cs) | 줌·보정 정보를 받아 일정 시간 동안 카메라 줌을 바꿨다가 끝나면 원래대로 되돌린다(코루틴). |

### SetBossRoomCamera
| 위치 | 역할 |
| --- | --- |
| [SetBossRoomCamera.Execute()](../../../../Scripts-Origin/Camera/Control/SetBossRoomCamera.cs) | 이벤트로 호출되어 기본 카메라 값 고정 코루틴을 시작한다(보스룸 진입 연출용). |

<br>
<br>
<br>

# 분석

### 기능 필요성

3인칭 액션에서 카메라는 단순히 화면을 비추는 게 아니라, 플레이어가 적을 향해 싸우도록 돕는 조준 장치다. 주변 적을 찾아 표시하고(마커·외곽선), 한 적을 골라 고정(락온)하면 카메라가 그 적을 계속 비추고, 이동·회전·스킬 조준이 그 고정 대상을 기준으로 움직인다. 즉 카메라 시스템은 "어떤 적을 상대하는가"라는 정보의 출처다. 지금은 이 모든 일(탐지·락온·마커·외곽선·화면 셋업·흔들림)이 한 클래스의 한 `Update`에 묶여 있어, 한 부분을 고치면 다른 부분이 함께 흔들린다.

### 필요한 기능

1. **적 탐지**
   - 매 프레임 플레이어 주변을 훑어 시야각·거리 조건에 맞는 "조준 가능한 적"을 하나 고른다.
   - 방향: 탐지는 "찾은 적"만 내놓는 정보 제공자로 두고(OverlapSphere 유지), 마커·외곽선·락온이 그 결과를 가져다 쓴다.

2. **락온(고정 조준)**
   - 입력으로 한 적을 고정하고, 그 적이 죽거나 멀어지면 자동으로 옮기거나 푼다.
   - 방향: 락온은 "락온 켜짐 여부 + 고정 대상"이라는 상태만 노출하고, 카메라·애니메이션이 그 상태를 보고 스스로 반응한다.

3. **카메라 전환**
   - 기본 / 락온 두 가지(처형은 아래 참조)를 전환한다.
   - 방향: 전환은 락온 상태를 보고 가상 카메라 우선순위로 바꾸는 방식. 종류를 enum으로 세지 않고 "락온 on/off"면 충분.

4. **조준 표시 (마커·외곽선)**
   - 조준/고정 대상 머리 위 마커, 대상 외곽선.
   - 방향: 탐지 결과를 구독해 표시만. 외곽선은 재질을 코드로 만지는 현 방식 대신 렌더링 단계(URP)에서 처리 검토.

5. **화면 셋업**
   - 시작 시 해상도·프레임 상한 1회 설정.
   - 방향: 탐지·락온과 얽히지 않는 독립 1회 셋업으로 분리.

6. **카메라 연출 (흔들림·줌)**
   - 스킬·타격 시 흔들림(Impulse)과 줌(Recomposer).
   - 방향: 지금처럼 스킬이 요청하는 구조 유지. 단 카메라 시스템 분리와는 별개 묶음.

### 적용 범위

`Scripts-Origin` 안에서 카메라 관련 호출을 추적한 결과(텍스트 검색 기반, 일부 `(확인 필요)`):

| 기능 | 노출 | 부르는 곳 |
| --- | --- | --- |
| `CurrentTarget`(고정 대상) 읽기 | public 프로퍼티 | PlayerMovement(이동·회전이 대상 바라봄), PlayerMaskChange, HumanMaskSkill 등 스킬 타겟팅 — **가장 많이 소비됨** |
| `LockOnTarget()` | public | MobileInput(모바일 버튼), PlayerController(PC 입력) |
| `isTargetDetected` | public | PlayerController |
| `SetPCPlatform()` | public | PlatformSwitcher |
| `SetMouseSpeed()` / `GetMouseSpeed()` | public | MouseSettingUI |
| `ChangeCamera()` / `DefaultCamera` / `TerrainLoadCamera` | public | SceneSwitcher(씬 전환), PlayerMovement(재정렬) |
| `HoldDefaultCameraValue()` | public 코루틴 | SetBossRoomCamera(이벤트) |
| `ShakeCamera()` / `ToggleCameraRecomposer()` | public | Human/Animal/GhostMaskSkill (스킬별 다수) |

- **공용으로 묶을 부분**: "탐지 결과"는 마커·외곽선·락온이 함께 쓰므로 한 정보원에서 제공.
- **대상별로 갈라야 할 부분**: 탐지·락온·카메라전환·마커·외곽선·화면셋업은 성격이 달라 쪼갠다. 흔들림·줌(PlayerCameraEffect)은 스킬이 쓰는 별개 묶음.
- **적용 범위 한정**: 소비처가 가장 많은 `CurrentTarget`(락온 대상)부터 안정적으로 노출하는 게 우선. 화면셋업·스와이프는 결합이 약해 후순위.

### 예상 문제

1. **한 `Update`의 순차 호출 의존**
   - 지금은 `DetectTargetAlways → CheckCurrentTargetState → ControlTargetMarker → CheckOutlineTarget` 순서에 결과가 의존한다. 순서를 바꾸거나 하나를 빼면 옆이 깨진다.
   - 방향: 탐지가 "찾은 적"을 내놓고 나머지가 그것을 구독하게 해 호출 순서 의존을 끊는다.

2. **`CurrentTarget`을 너무 많은 곳이 직접 읽음**
   - 이동·회전·스킬·마스크 변경이 `CameraController.instance.CurrentTarget`을 직접 본다. 카메라를 손대면 전투·이동이 함께 흔들린다.
   - 방향: 고정 대상을 카메라 클래스가 아니라 "락온 상태" 통로로 노출하고, 소비처는 그 통로만 의존하게 한다.

3. **데이터가 코드에 박힘**
   - `HoldDefaultCameraValue`/`HoldLockOnCameraValue`가 보스씬(`buildIndex == 4`) 여부로 카메라 높이·반경·오프셋 숫자를 코드에 직접 적는다. 씬이 늘면 분기도 는다.
   - 방향: 이 값들을 `CameraData`로 옮기고 씬 인덱스 하드코딩(`== 4`, `== 0`)을 없앤다.

4. **외곽선이 메시 재질을 직접 조작**
   - 적의 `MeshRenderer.materials`에 외곽선 재질을 끼워 넣어, 적마다 수동 관리되고 재질 복제본이 는다.
   - 방향: 렌더링 단계(URP Render Objects)에서 처리 검토. 보조 기능이라 핵심(탐지·락온) 뒤로 미룸.

5. **처형(FinishSkill) 카메라가 섞여 있음**
   - 카메라 종류에 처형이 끼어 있고, `Update`는 처형 중이면 통째로 막힌다.
   - 방향: 처형 연출은 타임라인으로 분리해 일반 카메라 전환에서 뺀다.

6. **싱글톤·SerializeField 의존**
   - `CameraController.instance`·`CameraData.Instance` 등 전역 접근이 많고, 외부 참조 대부분이 인스펙터 주입이라 테스트·교체가 어렵다.
   - 방향: 책임 분리 후 의존성 주입으로 바꾼다. 단 소비처가 많아 마지막 단계.
   - (검토 후 제외: 락온에 TargetGroup을 계속 쓰는 방안 — 단일 락온이라 불필요해 제외, 락온 카메라는 따라가기=플레이어·바라보기=적으로 충분.)

<br>
<br>
<br>

## 카메라 사용 흐름 (이렇게 흐른다)

1. **탐지 (매 프레임)**: 카메라가 플레이어 주변을 훑어 → 시야각·거리에 맞는 적 하나를 **조준 대상**으로 골라 둠 → 적이 있으면 전투 HUD를 띄우고, 마커와 외곽선을 그 대상에 표시.
2. **락온 켜기 (입력)**: 플레이어가 락온 버튼을 누름(PC=PlayerController, 모바일=MobileInput) → 조준 대상을 **고정 대상(CurrentTarget)** 으로 삼음 → 락온 카메라로 전환 + 애니 "집중" 켜짐 + 마커 색 바뀜.
3. **고정 대상 소비 (전투의 중심)**: 고정 대상이 정해지면 → **이동·회전**(PlayerMovement)이 그 적을 바라보게 돌고, **스킬**(MaskSkill)이 그 적을 조준해 날아가고, **마스크 변경**도 그 값을 참고. 즉 락온 대상은 카메라만의 것이 아니라 전투 전체가 읽는 공유 정보.
4. **자동 유지/해제**: 고정 대상이 죽거나 사라지면 → 주변의 다른 적으로 자동 이동, 없으면 **락온 해제** → 기본 카메라로 복귀.
5. **연출 끼어들기**: 스킬·타격이 일어나면 → 스킬이 `PlayerCameraEffect`에 **흔들림/줌**을 요청(카메라 전환과 별개). 처형은 타임라인 연출로 분리 예정.
6. **씬 전환·플랫폼**: 씬이 바뀌면 SceneSwitcher가 지형 로딩 카메라/기본 카메라를 토글하고 기본 카메라로 되돌림. 플랫폼(PC/모바일)에 따라 회전 입력축·스와이프 사용이 갈림.

> 구체적 클래스 분해·인터페이스 정의는 이 문서 범위 밖. 위 흐름과 분리 방향만 확정한다. (진행 중 프로토타입은 `_Test` 클래스로 별도 존재)
