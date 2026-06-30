# 플레이어 물리 이동 시스템 개선 — 기술 회고록

> 한 줄 요약: **"지형을 뚫는다"는 증상을 MovePosition + 자체 센서로 막으려다 한계에 부딪혔고, 결국 충돌 책임을 CharacterController로 통일해서 풀었다.**
> 이 문서는 그 과정을 처음부터 끝까지, 원리와 공식까지 박아서 남긴다. 다음에 같은 곳에서 헤매지 않으려고.

---

## 초록 (먼저 결론부터)

우리는 "캐릭터가 빠르게 움직이면 벽을 뚫는다"는 문제를 만났다.
처음엔 원래 쓰던 `Rigidbody.MovePosition`을 그대로 두고, 이동 직전에 충돌을 검사하는 **센서(PlayerSensor)**를 붙여 막으려 했다.
센서를 9단계에 걸쳐 다듬었지만, **MovePosition + sweep 검사**의 구조적 한계(시작 겹침, 코너 관통, 박힘)를 못 벗어났다.
결국 **충돌을 누가 책임지는가**라는 구조를 다시 정해서, `CharacterController.Move`로 갈아탔다.
이게 우리 요구(빠른 관통 방지 + 정확한 스킬 궤적 + 애니 동기화)에 가장 맞았다.
그 과정에서 "가파른 경사 미끄러짐"이 안 되는 문제를 만났고, **로그로 원인을 확정**해서 해결했다.

---

## 1. 어떤 이동이 요구되는가 — "이렇게만 되면 된다"고 생각했던 평화로운 시절

기능을 만들 때는 단순했다. 요구를 "이렇게만 되면 된다" 식으로 적으면 이랬다.

- **일반 이동**: 스틱/키 입력대로, 카메라가 보는 방향 기준으로 **걷기만 하면 된다.** 가는 쪽을 바라보게 **돌기만 하면 된다.**
- **스킬 이동**: 스킬마다 정한 방향·속도·시간(`ISkillMove`)대로 **그만큼 날아가기만 하면 된다.** 돌진은 앞으로, 회전낙법은 아래로. x·y·z 다 쓴다.
- **추락**: 발 밑이 비면 **떨어지기만 하면 된다.**
- **넉백**: 맞으면 **뒤로 밀리기만 하면 된다.** (연출이지 핵심 재미는 아니다)

여기엔 한 줄이 빠져 있었다. 그리고 그 빠진 한 줄이 며칠을 잡아먹었다.

> **"어떤 경우에도 지형을 뚫지 않으면서."**

평화로울 땐 안 보였다. 스킬로 빠르게 날기 시작하니 보였다.

---

## 2. 어떤 방식으로 이동했는가 — MovePosition

### 2.1 우리가 쓰던 구조

- 캐릭터: **Rigidbody (비 kinematic) + CapsuleCollider + 중력(useGravity) 켜짐**, Collision Detection = **Discrete**
- 이동: `PlayerMovement`(일반), `PlayerSkillMoveHandler`(스킬)가 **각자** 매 `FixedUpdate`에서 `rb.MovePosition(목표위치)` 호출

코드로는 이렇게 단순했다.

```csharp
Vector3 delta = 방향 * Time.deltaTime * 속도;
rb.MovePosition(rb.position + delta);
```

### 2.2 MovePosition의 원리 (자세히)

`transform.position = x` 와 `rb.MovePosition(x)` 는 겉보기엔 같아 보여도 속은 완전히 다르다.

**물리 엔진(PhysX)은 매 물리 스텝(FixedUpdate)마다 이 순서로 돈다:**

```
1) 속도로 위치 적분   : p ← p + v·Δt
2) 충돌 감지          : 누가 누구랑 겹치나 (broad → narrow phase)
3) 충돌 해소(solver)  : 파고든 만큼 밀어내고, 반발/마찰 처리
```

- **transform.position 직접 대입**은 이 파이프라인을 **건너뛴다.** 외부에서 위치를 강제로 꽂는 "순간이동"이다.
  물리는 "이전 → 새 위치" 사이 경로를 **모른다.** 그래서 벽을 지나도 모른다. 속도(velocity)도 안 생긴다.

- **MovePosition**은 "다음 스텝까지 이 목표로 가라"는 **예약**이다. 물리가 다음 스텝에서 처리하며, 내부적으로 도달에 필요한 속도를 **역산**한다:

  ```
  v_required = (target - current) / Δt        (Δt = Time.fixedDeltaTime, 기본 0.02s = 50Hz)
  ```

  이 속도로 스텝을 굴리므로 위 파이프라인(1~3)을 정상적으로 탄다. velocity가 생기고, 보간(interpolation)도 이걸 기준으로 화면을 부드럽게 메운다.

> 비유: `transform` = 벽 너머로 뿅 순간이동. `MovePosition` = "저기까지 빨리 걸어가" 명령.
> 보통은 걸어가다 벽에서 멈추지만 — **한 걸음이 너무 크면 벽을 한 번에 넘어버린다.**

(근거: [Unity Scripting API: Rigidbody.MovePosition](https://docs.unity3d.com/ScriptReference/Rigidbody.MovePosition.html), [Unity Manual: Rigidbody interpolation](https://docs.unity3d.com/Manual/rigidbody-interpolation.html))

### 2.3 왜 이 방식으로 했나

- 이동 거리·시간을 **코드로 정확히 제어**할 수 있다. 스킬은 "0.3초 동안 30m/s로 앞" 같은 정해진 궤적이라 이게 중요했다.
- `transform`보다 낫다고 봤다. 어쨌든 물리를 "통해서" 가니까 충돌도 어느 정도 처리될 거라 믿었다.
- 그 믿음이 절반만 맞았다는 게 문제의 시작이었다.

---

## 3. 무엇이 문제가 되었는가 — 터널링과 "두 손"의 간섭

### 3.1 터널링 (벽 뚫기) — 공식으로

한 물리 스텝에 이동하는 거리는:

```
d = |v| · Δt
```

벽(또는 바닥) 두께를 `T`라 하자. 충돌 검사가 스텝의 양 끝(시작·끝 위치) 중심으로 일어나는 Discrete 방식에서는:

```
d ≥ T  →  스텝 사이에 벽을 통째로 건너뛴다 → 충돌 미검출 → 관통(터널링)
```

예를 들어 스킬 속도 `|v| = 30 m/s`, `Δt = 0.02s` 면 한 스텝 이동 `d = 0.6 m`.
벽이 그보다 얇거나, 시작·끝이 우연히 벽 양쪽이면 그냥 뚫린다.

(근거: ["When a bullet is in front of the wall in one frame but behind it in the next, no collision is detected" — DigitalRune CCD](https://digitalrune.github.io/DigitalRune-Documentation/html/138fc8fe-c536-40e0-af6b-0fb7e8eb9623.htm), [Unity tunneling 설명](https://unityqueen.com/2026/02/22/unity-continuous-vs-discrete-collision-detection-how-to-prevent-tunneling/))

### 3.2 더 깊은 근본 — 캐릭터를 미는 "손"이 둘이었다

- **한 손은 물리 엔진.** 중력으로 당기고, 콜라이더로 벽에 부딪힌다.
- **다른 손은 우리 코드.** MovePosition으로 "여기로 가" 하고 위치를 직접 찍는다.
- 이 둘이 따로 논다. 그래서 서로 방해한다.
  - 빠르게 움직이면 → 물리가 못 막는다 → **벽을 뚫는다.**
  - 좁은 구석(90도 코너)에 가면 → 콜라이더가 양쪽에 끼인다.
  - 떨어지려 하면 → MovePosition이 위치를 덮어쓴다 → 안 떨어진다 → **공중에서 걷는다.**

이게 나중에 깨달은 진짜 원인이다. 증상이 여러 개로 보였지만 뿌리는 하나였다.

---

## 4. 어떻게 개선하려고 했는가 — 센서 도입, 그리고 개선→문제→분석의 반복

발상은 단순했다. **"이동하기 전에, 갈 자리에 뭐가 있는지 미리 검사해서 막자."**
이동 컴포넌트가 `MovePosition` 직전에 `PlayerSensor.ResolveMove(현재위치, 이동량, 콜라이더)`를 불러 보정된 이동량을 받는 구조다.

그 다음은 두더지 잡기였다. 고치면 새 구멍이 났다.

| # | 개선 | 그래서 터진 문제 | 분석 |
|---|---|---|---|
| 1 | 전방 정지 + 지면 관통 방지(SphereCast 2회) | 공중 회전 중 앞 장애물 못 봄 | 한 지점만 쏴서 키를 못 덮음 |
| 2 | CapsuleCast로 키 전체 덮기 | 경사 보정이 과해 더 멀리 날아감 | 보정 로직이 위로 끌어올림 |
| 3 | 경사 보정 제거, 콜라이더 치수 그대로 캐스트 | 평지에서도 아래로 뚫림 | **시작 겹침** (발이 지면에 붙어 출발) |
| 4 | 스킨(skin): 콜라이더보다 작게 + 벽 앞 여유 | 여전히 깊이 박히면 통과 | sweep은 시작 겹친 콜라이더를 무시 |
| 5 | 백오프(back-off): 검사 시작점을 뒤로 뺌 | 벽에 붙어 반대로 가려는데 막힘 | 뒤로 뺀 시작점이 등 뒤 벽을 잡음 |
| 6 | collide & slide(투영): 남은 이동을 면에 흘림 | 코너에서 옆 벽 파고듦 | 투영 결과를 재검사 안 함(캐스트 1회) |
| 7 | Dot 판정: 등 뒤 면(`n·dir>0`)은 무시 | — (벽 붙어 반대로 가기 해결) | 면의 앞/뒤를 방향으로 구분 |
| 8 | 겹침 안전장치(`hit.distance≈0`이면 정지) | 깊은 겹침·박힘은 여전히 못 품 | depenetration이 없음 |
| 9 | 디버그 시각화(캡슐·충돌점 Gizmos) | — (원인 확인 도구) | "보여야 고친다" |

### 이 단계에서 얻은 가장 중요한 통찰

> **"센서를 거치는 것"과 "충돌을 인식하는 것"은 다르다.**
> `ResolveMove`는 항상 호출된다. 하지만 그 안의 CapsuleCast가 충돌을 **못 잡으면** 이동량을 그대로 돌려준다.
> 즉 "거쳐도 뚫린다." 고칠 지점은 막는 로직이 아니라 **검출이 되게 만드는 것**이었다.

---

## 5. 어떻게 해결했는가 — 센서를 버리고 충돌 방식 자체를 교체

### 5.1 기존(MovePosition + 센서) 방식의 한계 (구조적이라 못 벗어남)

- **시작 겹침**: Sweep(CapsuleCast)는 출발 시 이미 겹친 콜라이더를 **무시**한다. 캐릭터는 늘 발을 지면에 붙이고 서 있어서 **항상 겹쳐서 출발**한다. 백오프는 미봉책.
- **코너 관통**: 한 벽에 투영한 결과가 옆 벽을 향하는데 재검사 안 함. 반복(iteration)이 필요하나 비용.
- **박힘 해소 없음**: 한번 박히면 빠져나오는 로직이 없다.
- **공중 걷기**: 코너 끼임 + MovePosition이 중력 y를 덮어씀.
- **공통 뿌리**: 물리와 수동, **두 손의 간섭**. 아래층(센서)만 고쳐선 안 풀린다.

### 5.2 어떤 방법들이 있었나 — 비교

| 방법 | 원리 한 줄 | 빠른 관통 방지 | 정확한 궤적 | 애니 동기화 | 물리 상호작용 | 비용 |
|---|---|---|---|---|---|---|
| MovePosition 단독 | 위치를 목표로 보냄 | ✗ | ◎ | ◎ | △ | 낮음 |
| MovePosition + 센서 | 이동 전 sweep 보정 | △ | ○ | ○ | △ | 높음(끝없는 패치) |
| velocity + Continuous CCD | 물리에 맡기고 CCD로 터널링 방지 | ◎ | ✗(물리가 궤적 변형) | ✗(발 미끄러짐) | ◎ | 중간 |
| **CharacterController.Move** | 이동 거리를 직접 sweep해 막음 | ◎ | ◎ | ◎ | △(직접) | 중간 |

### 5.3 우리 프로젝트 기능 × 해결방법 매트릭스 (O / X / △)

우리가 진짜 필요한 기능을 줄로, 방법을 칸으로 놓고 따져봤다.

| 우리 기능 | MovePosition 단독 | MovePosition+센서 | velocity+CCD | CharacterController |
|---|:---:|:---:|:---:|:---:|
| 일반 이동(카메라 기준) | O | O | O | O |
| 스킬 이동: 정해진 궤적 정확히 | O | O | **X** (물리가 휨) | O |
| 스킬 이동: 애니메이션 동기화 | O | O | **X** (발 미끄러짐) | O |
| 빠른 이동에도 안 뚫림 | **X** | **△** (한계 남음) | O | O |
| 추락(중력) | O | O | O | △ (직접 구현) |
| 경사·턱 처리 | X | △ | △ | O (내장) |
| 넉백/물리 상호작용 | △ | △ | O | △ (직접 구현) |
| 코너에서 안 끼고 안 뚫림 | X | **X** | O | O |
| 유지보수(구멍 안 생김) | O | **X** (계속 메움) | △ | O |

표를 보면 답이 보인다. **velocity는 "정확한 스킬 궤적"이라는 우리 핵심에서 X**가 뜨고, **센서는 "안 뚫림/코너/유지보수"에서 X**가 뜬다. **CharacterController만 X가 없다** (△는 직접 구현으로 메울 수 있는 것들이고, 그건 연출성 기능이라 감당 가능).

### 5.4 그래서 우리에게 어울리는 방법 — CharacterController.Move

선택 근거를 우선순위로:
1. **뚫림 방지가 최우선** → Move는 이동 거리를 훑어(sweep) 막는 게 기본 동작.
2. **스킬 궤적은 애니에 맞춰 정확해야** → Move는 "정확히 이만큼"이라 적합. velocity는 부적합.
3. **물리 상호작용은 연출용** → 약점이 치명적이지 않음. 넉백은 직접 구현.

### 5.5 CharacterController의 원리 (자세히, 공식 포함)

CharacterController는 Rigidbody 물리를 안 쓴다. `Move(motion)`을 부르면 **내부에서 캡슐을 그 거리만큼 쓸어가며(swept)** 충돌을 처리한다. 이건 학계의 **"collide & slide"** 알고리즘이다 (Kasper Fauerby, *Improved Collision Detection and Response*).

**핵심 수식 — 충돌면을 따라 미끄러지기 (sliding plane projection):**

이동 벡터 `v`가 법선 `n`(단위벡터)인 면에 부딪히면, 면을 파고드는 성분을 제거하고 면을 따라가는 성분만 남긴다.

```
v_slide = v - (v · n) · n
```

이게 `Vector3.ProjectOnPlane(v, n)` 의 정체다. 이걸 충돌이 없을 때까지 **여러 번 반복(iteration)** 하면 코너에서도 자연스럽게 미끄러진다. (Fauerby 알고리즘은 보통 최대 3~5회 반복 + 아주 작은 이동은 멈춤.)

**경사 판정 — 면의 기울기 각도:**

```
θ = acos( n · up )          (= Unity의 Vector3.Angle(n, Vector3.up))
θ ≤ slopeLimit  → 걸어 오를 수 있는 바닥
θ >  slopeLimit  → 못 오르는 벽/급경사
```

**제공하는 값:** `isGrounded`(접지), `collisionFlags`(닿은 부위 None/Sides/Above/Below), `velocity`(실제 이동 속도), `slopeLimit`/`stepOffset`/`skinWidth`.

**한계(중요):** CharacterController는 **중력이 자동이 아니다.** 직접 y속도를 누적해 Move에 넣어야 한다. 그리고 `slopeLimit`은 "올라가기"만 막을 뿐 **가파른 경사에서 미끄러뜨려 내려주진 않는다.**

(근거: [Fauerby, Improved Collision Detection and Response (PDF)](https://www.peroxide.dk/papers/collision/collision.pdf), [Unity Manual: CharacterController.Move](https://docs.unity3d.com/ScriptReference/CharacterController.Move.html))

### 5.6 적용 과정 (격리 테스트로)

기존 시스템은 **건드리지 않고**, `Movement_Test/` 폴더에 새로 만들어 실제 입력·상태·스킬 데이터로 검증했다.

- `CharacterControllerPlayerMover_Test` — 일반이동·스킬이동·추락·미끄러짐을 **매 프레임 한 벡터로 합쳐 `Move` 한 번** 호출(유일한 이동 지점). 합산 공식:

  ```
  velocity = (입력 수평 × moveSpeed)
           + Σ(스킬 i 의 로컬속도를 캐릭터 회전으로 돌린 값)
           + 중력 처리(아래 참조)
  controller.Move(velocity × Δt)
  ```

- **중력·미끄러짐:**
  ```
  vy ← vy + gravity·Δt          (항상 누적, 하한 maxFallSpeed)
  if (바닥 && 안 가파름 && vy<0)  vy ← groundedStick   (-2, 바닥에 붙임)
  if (가파름)  velocity += ProjectOnPlane((0,vy,0), groundNormal)   // 면 따라 미끄러짐
  else         velocity.y += vy
  ```

- **콜라이더 분리 방침:** 이동 = CharacterController, **피격 = 애니메이션 본을 따라가는 히트박스.** 피격 콜라이더가 애니를 따라가니 CharacterController와 맞출 필요가 없다(액션 게임 정석). 이동용 CapsuleCollider는 제거 가능.

### 5.7 적용 중 터진 문제와 디버깅 — "센서 쓰면 된다"는 말을 끝내 버린 순간

여기서 솔직하게 적어야 한다. **이 과정에서 AI(나)는 계속 "내가 만든 센서/방식이면 된다"는 쪽으로 끌고 가려 했다.** 백오프를 키우면, 스킨을 조정하면, iteration을 넣으면 된다고. 그게 다 미봉책이었다.

실제로 문제를 끝낸 건 사용자의 한마디였다:
> *"OnControllerColliderHit으로 각도는 다 잘 잡히는데, 그럼 이걸로 45도 넘으면 미끄러지게 하면 되잖아?"*

디버깅으로 원인을 하나씩 확정한 순서:

| 발견 | 원인 | 처리 |
|---|---|---|
| 가파른 경사에 서있는데 안 미끄러짐 | slopeLimit은 "올라가기"만 막음 (미끄러뜨리는 기능이 없음) | 중력을 면에 투영해 직접 미끄러뜨림 |
| 정지하면 미끄러짐이 꺼짐 (로그: `groundAngle=0, normalY=1.00`) | 경사 법선을 `OnControllerColliderHit`(이동 중에만 호출)에 의존 | (1차) 매 프레임 발밑 SphereCast로 측정 시도 |
| 그 SphereCast도 자주 실패(`none`) | 피벗/검사 거리 문제로 발밑을 못 잡음 | (폐기) — 더 단순한 길로 |
| `OnControllerColliderHit`이 Mover에서 호출 안 됨 | 이 콜백은 **CharacterController가 붙은 그 오브젝트의 스크립트**만 받음. Mover는 매니저 오브젝트에 있었음 | 캐릭터에 `ControllerHitLogger_Test`를 붙여 콜백을 받고, Mover가 그 `GroundNormal`을 읽음 |
| 벽쪽으로 갈 때만 안 미끄러짐 | 미끄러짐 조건에 `isGrounded`를 걸어둠 → 벽 옆구리에 닿으면 `isGrounded=false` | **`isGrounded` 제외, 각도만으로 판정** (`θ > slopeLimit`) |

**왜 OnControllerColliderHit이 정지 중에도 잘 잡혔나?** 중력이 매 프레임 아래로 Move를 일으켜 경사면에 계속 닿기 때문이다. 그래서 정지처럼 보여도 콜백이 호출되고 법선이 갱신된다 — 이게 발밑 SphereCast보다 단순하고 정확한 해법이었다.

> **교훈:** AI가 자기가 만든 구조(센서)를 옹호할 때, 사람이 더 단순한 엔진 내장 신호(`OnControllerColliderHit`의 각도)로 끌고 온 게 정답이었다.

---

## 6. 앞으로 AI를 어떻게 쓸 것인가

이번에 가장 비싸게 배운 것들이다.

1. **구조를 먼저 사람이 정한다.** AI는 증상별 패치를 잘 만든다. 하지만 "충돌 책임을 물리/수동 중 누가 지나" 같은 **위층 결정**을 안 하면, 아래층(센서 디테일)만 끝없이 고치게 된다. 큰 갈림길은 AI에게 옵션을 정리받되 결정은 사람이.
2. **원인은 추측이 아니라 측정으로 확정한다.** "고쳐줘"보다 "원인을 로그/Gizmos로 먼저 보자"가 빨랐다. `groundAngle=0` 로그 한 줄이 며칠을 5분으로 줄였다.
3. **AI의 전제를 의심한다.** AI는 자기가 만든 것(센서)을 옹호하는 쪽으로 흐른다. "거치는데 왜 뚫리지?" 같은 본질 질문으로 사람이 끊어야 한다.
4. **한 번에 크게 말고 작게 쪼개 검증한다.** 큰 구조 변경을 한 방에 했다가 버벅임으로 통째 되돌렸다. `_Test`로 격리 검증이 옳았다.
5. **되돌릴 안전망을 둔다.** 기존은 그대로, 새 시도는 별도 폴더/파일. AI가 큰 변경을 칠 때 생명줄.
6. **AI 강점에 집중시킨다.** 방법 비교표, API 리스트업, 출처 있는 웹검색 — 트레이드오프 정리는 AI가 빠르고 정확하다. "무엇이 있나"는 AI, "무엇을 택하나"는 사람.
7. **최종 검증은 사람이 실제 실행으로.** AI는 컴파일·플레이를 못 한다. 씬 배선·조작 테스트는 사람이.

---

## 7. 문제 재현 방법 (다음에 검증할 때 이대로)

### 7.1 터널링(벽 뚫기) 재현 — MovePosition

```csharp
// 비 kinematic Rigidbody + Collision Detection = Discrete 인 캐릭터에서
void FixedUpdate()
{
    Vector3 fast = transform.forward * 40f;     // 빠른 속도
    rb.MovePosition(rb.position + fast * Time.deltaTime);  // 얇은 벽을 향해 → 뚫림
}
```
- 얇은(두께 < `|v|·Δt = 40·0.02 = 0.8m`) 벽을 향해 돌진 → 통과.
- Collision Detection을 Continuous로 바꾸면 완화되는지 비교.

### 7.2 90도 코너 관통 재현

- 길쭉한 박스 2개를 **십자(+)** 로 겹쳐 배치 → 90도 안쪽 구석 4개.
- 그 구석으로 빠른 돌진 스킬을 박는다 → 센서(1회 캐스트)는 옆 벽을 못 봐 파고듦.

### 7.3 공중 걷기 재현

- 90도 안쪽 코너에 끼인 채 수평 입력 유지 → MovePosition이 y를 덮어써 안 떨어짐.

### 7.4 CharacterController 해법 검증 (현재 테스트 구성)

1. 캐릭터(H·A) 오브젝트에 **CharacterController** + **`ControllerHitLogger_Test`** 둘 다 붙인다. (Radius/Height/Center를 캡슐에 맞춤, Slope Limit=45, Step Offset≈0.1)
2. 매니저 오브젝트에 **`CharacterControllerPlayerMover_Test`** 를 붙인다(DI로 입력·상태·현재캐릭터 주입). 기존 `PlayerMovement`/`PlayerSkillMoveHandler`는 끈다.
3. 검증:
   - 십자 코너에 돌진 → 안 뚫리는지.
   - 회전낙법(아래 큰 속력) → 바닥 안 뚫리는지.
   - 45도 초과 경사/벽 → 못 서고 미끄러져 내려오는지(벽쪽으로 밀어도).

---

## 8. 부록 — 산출물과 남은 과제

**산출물**
- `Movement_Test/CharacterControllerPlayerMover_Test.cs` — CC 기반 통합 이동.
- `Movement_Test/ControllerHitLogger_Test.cs` — 캐릭터 충돌 면 법선 제공.
- `Sensor/PlayerSensor.cs` — 1차 시도(MovePosition+센서)의 결과물 + 상단에 한계를 `대원_TODO`로 박제.

**남은 과제**
- 공중에서 마지막 충돌 법선이 남아 미끄러지는 부작용 → 로거에 "충돌 없는 프레임 법선 리셋" 추가 검토.
- 스킬 큰 속력의 벽타기(슬라이드가 면 따라 위로) → 스킬 중 가파른 면 충돌 시 멈추는 처리.
- `_Test` 승격 + 기존 MovePosition 계열 정리, 넉백·피격 히트박스·동적 발판 설계.

---

## 참고 문헌

- [Unity Scripting API — Rigidbody.MovePosition](https://docs.unity3d.com/ScriptReference/Rigidbody.MovePosition.html)
- [Unity Scripting API — CharacterController.Move](https://docs.unity3d.com/ScriptReference/CharacterController.Move.html)
- [Unity Manual — Rigidbody interpolation](https://docs.unity3d.com/Manual/rigidbody-interpolation.html)
- [Unity Manual — Continuous collision detection (CCD)](https://docs.unity3d.com/2020.1/Documentation/Manual/ContinuousCollisionDetection.html)
- [Kasper Fauerby — Improved Collision Detection and Response (PDF)](https://www.peroxide.dk/papers/collision/collision.pdf)
- [DigitalRune — Continuous Collision Detection (배경)](https://digitalrune.github.io/DigitalRune-Documentation/html/138fc8fe-c536-40e0-af6b-0fb7e8eb9623.htm)
- [UnityQueen — Continuous vs Discrete Collision Detection (tunneling)](https://unityqueen.com/2026/02/22/unity-continuous-vs-discrete-collision-detection-how-to-prevent-tunneling/)
