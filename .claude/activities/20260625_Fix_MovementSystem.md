# 플레이어 물리 이동 시스템 개선 — 기술 회고록

> **이동할 때, "지형을 뚫는다"는 증상을 MovePosition + 자체 센서로 막으려다 한계에 부딪혔고, 결국 충돌 책임을 CharacterController로 통일해서 풀었다.**

---

## 요약

문제의 시작은 **캐릭터가 빠르게 움직이면 지형을 뚫는다** 였다.
처음엔 원래 쓰던 `Rigidbody.MovePosition`을 그대로 두고, 이동 직전에 충돌을 검사하는 **센서(PlayerSensor)** 구현하여 지형 뚫는 문제가 해결하려고 했다. 하지만 겹친 지형에서의 뚫림, 급경사를 그대로 타고 올라가는 문제가 뒤이어 드러났다.

결국 이동 방식과 충돌 감지에 대한 이해가 부족했다고 판단하고, 처음으로 돌아가 프로젝트에 맞는 이동 방식을 고르는 것부터 시작했다.

```
1. 빠른 관통 방지 — 어떤 속도에서도 지형을 뚫지 않을 것
2. 정확한 충돌 정보 수집 — 무엇에 어떤 각도로 닿았는지를 믿을 수 있게 알 것
3. 제어 용이성 — 애니메이션에 맞춰 벡터로 급변하는 움직임을, 의도대로 정확히 제어하기 쉬울 것
4. 낮은 교체 비용 — 기존 코드를 크게 갈아엎지 않고 적용할 수 있을 것
```

가장 적합한 **CharacterController.Move** 방식으로 교체했고, "빠른 이동 시 지형 뚫림"을 해결했다. 이동거리를 sweep하고 이동하여 스킬의 빠른 이동이 지형을 뚫지 못하도록 하기 때문이다.

가파른 경사에서 미끄러지지 않는 문제가 남았는데, `CharacterController`의 `Slop Limit`는 경사를 못올라가게만 할 뿐이라 CharacterController가 제공하는 `OnControllerColliderHit`에서 닿은 면의 각도를 직접 얻어, 그 각도가 `Slope Limit`을 넘으면 중력을 그 경사면을 따라 흘려보내는 방법으로 해결했다.

---

## 1. 어떤 이동이 요구되는가

기능을 재구현할 때는 단순했다.

- **일반 이동**: 카메라가 보는 방향 기준으로 이동과 회전. 장애물이 있으면 정지
- **스킬 이동**: 정해진 방향,속도,시간(`ISkillMove`)대로 이동. 장애물이 있으면 정지
- **넉백**: 맞으면 뒤로 밀리기. 장애물이 있으면 정지

하지만 캐릭터 지형 테스트 과정에서 **"어떤 상황에도 지형을 뚫지 않도록"** 구현하는 것은 어려운 요구사항이었다.

---

## 2. 어떤 방식을 사용하는가
- Rigidbody(IsKinematic = false, UseGravity = true, interpolate = None, Collision Detection = Discrete)
- CapsuleCollider
- 이동 상태에서는 `PlayerMovement`,스킬 상태에서는 `PlayerSkillMoveHandler`에서 Rigidbody의 `MovePosition(목표위치)` 호출

- 기존 프로젝트에서도 MovePosition을 사용했고, 캐릭터(Rigidbody)를 이동하기에 좋은 방법이기 때문

---

## 3. 무엇이 문제인가

### 3.1 리팩토링 이전 프로젝트의 문제

- 플레이어 앞뒤로 매 프레임 Ray를 발사, 스킬 사용시 캐릭터 정면에 6개의 Ray를 발사해 장애물 및 적을 체크하여 이동 할지를 검사.
- 투박하게 막은 방식이라 일반 지형·장애물·겹친 장애물에서는 뚫리지 않았다.
- 단, 경사로에서 돌진 스킬을 쓰면 뚫렸다.
- 또 벽에 닿으면 그 자리에 멈춰, 벽을 따라 비스듬히 이동하는 것도 안 됐다.

### 3.2 현재의 문제

PlayerSensor.cs에서 플레이어가 MovePosition로 이동하기 직전에 `ResolveMove(현재위치, 이동량, 콜라이더)`로 이동량을 보정한다.(이동 방향으로 지형이 충돌하는지 Ray 발사)
아래는 센서의 기능 설명이다.

1. 이동방향으로 플레이어 콜라이더 모양의 CapsuleCast을 발사하여 지형 검사.
2. 검사 시작점을 이동 반대로 조금 빼서, 이미 겹친 상태로 검사하여 충돌을 놓치는 경우를 줄인다. 
3. 지형에 충돌하면 그 직전까지만 전진하고, 남은 이동량은 그 면을 따라 미끄러지게(면을 뚫는 방향 성분만 제거) 흘린다.
4. 충돌한 면이 이동 방향 쪽이 아니라 뒤쪽이면 무시하고 그대로 간다. (이동하기도 전에 뒤에서 충돌난 경우)
5. 닿은 거리가 거의 0이면(이미 겹친 상태) 그 방향 이동을 멈춘다.

아래는 센서의 문제이다.

- **장애물들이 겹쳐진 부분, 터레인의 코너에서 뚫린다.** CapsuleCast를 한 프레임에 한 번만 쏘고, 면을 따라 미끄러뜨린 결과를 다시 검사하지 않는다. 그래서 한 벽을 따라 흘린 이동이 옆 벽을 향하면, 그 옆 벽은 검사 없이 뚫고 들어간다.
- **한번 박히면 못 빠져나온다.** 이미 박힌 상태에서는 이동을 0으로 막을 뿐, 박힌 자리에서 밀어내 빼주는 처리가 없다. 그래서 박힌 채 고정될 수 있다.

틀어막기 식으로 개선하는 방식이라고 생각한다. 이런 방식으로는 계속 문제가 생길 것이라 생각되므로, 이동방식이 적절한가부터 판단하기로 하였다.
 
#### 재현
- 아래 클래스들과 현재 설계와 연결되는 부분을 수정한다.(설계가 살짝씩 수정됨)
- 기존 이동 및 회전 클래스들을 빼고, 아래 클래스들을 씬 아무 곳에 배치하면 된다.
- 각 캐릭터에 Rigidbody(Non-kinematic, useGravity), CapsuleCollider를 부착한다.

Assets\1.Code\Scripts\PlayerSystem\Movement\Problem\IPlayerSensor.cs
Assets\1.Code\Scripts\PlayerSystem\Movement\Problem\PlayerMovement.cs
Assets\1.Code\Scripts\PlayerSystem\Movement\Problem\PlayerSensor.cs
Assets\1.Code\Scripts\PlayerSystem\Movement\Problem\PlayerSkillMoveHandler.cs


---

## 4. 어떻게 개선했는가

### 4.1 CharacterController.Move 방식 선정

- velocity 이동 방식은 물리에 맞게 자연스럽게 휘둘리는 방식이라 정해진 궤적을 이동해야 하는 현재 방식과는 맞지 않음. 

- CharacterController는 이동 및 충돌로 인한 뚫림 방지를 기본 지원.
- CharacterController는 바닥 충돌시 OnControllerColliderHit로 바닥정보 정확히 전달받아 활용 가능
  - isGrounded를 통해 fall 상태 구현 쉽게 가능
  - 바닥면의 각도(slopLimit)를 통해 올라갈 수 있는 경사 지정 가능 및 경사 미끄러짐에 활용
  - 올라갈 수 있는 계단 각도 설정 쉽게 가능(stepOffset)
- 급경사 및 겹치는 지형에서 기존 빠른 속도의 10배로 테스트해도 지형 뚫림 없음.


### 4.2 추가로 생긴 문제와 해결

| 문제 | 원인 | 해결 |
|---|---|---|
| 45도 초과 경사에 서 있어도 안 미끄러짐 | `slopeLimit`은 올라가기만 막을 뿐 미끄러뜨리지 않음 | `OnControllerColliderHit`에서 충돌 면 법선을 얻어, 각도가 `slopeLimit`을 넘으면 중력을 그 면에 투영(`ProjectOnPlane`)해 미끄러뜨림 |
| 클래스 분리 후 `OnControllerColliderHit`이 Mover에서 호출 안 됨 | 이 콜백은 CharacterController가 붙은 오브젝트의 스크립트만 받음. Mover는 매니저 오브젝트에 있었음 | 캐릭터에 `ControllerHitLogger_Test`를 붙여 콜백을 받고, Mover가 그 `GroundNormal`을 읽음 |
| 벽쪽으로 계속 이동할 때, 안 미끄러짐 | 미끄러짐 조건에 `isGrounded`를 검. 벽 옆면에 닿으면 `isGrounded=false` | `isGrounded` 조건 제거, 각도(`θ > slopeLimit`)만으로 판정 |

CharacterController는 중력이 자동이 아니라서 y속도를 직접 누적해 `Move`에 넣어야 한다. 경사 각도는 `θ = Vector3.Angle(n, Vector3.up)`이고, `θ ≤ slopeLimit`이면 오를 수 있는 바닥, `θ > slopeLimit`이면 못 오르는 벽/급경사다.


### 4.3 결과적으로 어떻게 구성했는가

이동을 한 곳에서 처리하되, "어떤 속도를 낼지"는 종류별로 쪼개 위임하는 구조로 정리했다.

- **`PlayerCharacterMover` — 유일한 `Move` 호출자이자 DI 진입점.** 주입받은 의존성을 각 속도 소스 생성자로 넘겨 만들고, 매 프레임 모든 소스의 속도를 더해 `CharacterController.Move`를 **딱 한 번** 호출한다. 충돌·관통 방지는 CharacterController에게 맡긴다.
- **`IVelocitySource` — Mover는 구체 요인을 모른 채 IVelocitySource 구현체의 Evaluate(in MoveParams)를 받아 합산만 한다. 현재 구현은 `WalkVelocitySource`(일반 이동)·`SkillVelocitySource`(스킬 이동)·`GravityVelocitySource`(중력/경사 미끄러짐) 셋. 요인 추가/교체가 소스 한 줄 추가로 끝난다.
- **`MoveParams` — 프레임 입력 묶음(DeltaTime, Transform, Controller, GroundNormal, MoveDirection).** 모든 소스가 같은 한 프레임 데이터를 읽는다.
- **`GroundProbe`(캐릭터에 부착) — `OnControllerColliderHit`을 받아 발밑 면 법선 제공.** 콜백은 CharacterController가 붙은 오브젝트만 받으므로 캐릭터 쪽에 두고, Mover가 그 `GroundNormal`을 읽어 급경사 판정에 쓴다.
- **`CharacterRotator` — 회전(방향)만 담당.** 이동 속도와 분리.

핵심은 *합산 후 1회 이동* + *요인별 분리*다. 각 소스가 이벤트 구독·누적 상태를 스스로 관리하고, 캐릭터 교체 시 `OnCharacterChanged`로 누적값을 리셋한다.

---


## 5. 어떻게 AI랑 협업할 것인가

AI는 지형이 뚫리면 문제가 된다는 것을 학습을 통해 알게 된다. 즉, 현재 일어난 일을 명확히 이해하는 것이 아니라 학습에 기반하여 대답한다는 것이다. 
그렇기 때문에 CharacterController의 Hit를 받아오면 되는 상황에서 계속 센서의 CapsuleCast를 고쳐 바닥 충돌을 감지하도록 한다. 

AI가 금방 해결할 수 없는 문제에는 문제를 해결해달라는 대신에 원인을 분석할 디버깅 툴을 구현해달라고 해야한다. 사용자가 직접 원인을 분석할 수 있게 말이다. AI 덕분에 디버깅이 쉬워졌기 때문에 원인을 찾는 것도 빨라졌다. 

즉, '원인을 찾는 방법의 구현'을 AI에게 요구하는 것이다. 그리고 원인이 발견되면, 해당 원인을 해결할 가장 좋은 방식을 AI와 함께 찾고 설계하는 것이다. 설계의 방향과 흐름은 사용자가 결정하는 것이고, AI는 그와 관련된 방법들을 제공하고 구현하는 역할을 한다.

문제가 되는 코드는 하나의 폴더에 모아서 재구현할 수 있도록 보관한다