---
description: C#/Unity 코드 컨벤션. 코드 작성 시 이 규칙을 따르고, 검사 시 위반을 표로 보고한다.
---

# 코드 컨벤션

> 기준: Unity 공식 e-book ("Create a C# Style Guide") + 커뮤니티 관행 절충
> `Old/` 폴더와 실험용(`_Test`) 코드는 검사 대상에서 제외한다(삭제 예정 레거시).
> 주석 규칙은 이 문서가 아니라 `comment-convention`이 담당한다.
> 수치를 SO에 둘지 `[SerializeField]`에 둘지는 `Docs/SystemUsageGuide.md`의 ‘수치를 어디 둘 것인가’를 따른다.

---

## 0. 검사·보고 절차

1. 대상 `.cs` 파일을 직접 읽는다. 추측하지 않는다.
2. 아래 1~7절과 대조해 위반을 찾는다.
3. **코드를 수정하지 않는다. 보고만 한다.**
4. 아래 형식의 표로 출력한다.

| 파일:줄 | 규칙 | 현재 | 제안 |
|---|---|---|---|
| PlayerController.cs:42 | 1절 약어 금지 | `int idx` | `int index` |

- 위반이 없으면 `위반 없음` 한 줄만 출력한다.
- 규칙 칸에는 반드시 절 번호를 적는다.
- `[SerializeField]`·public 멤버의 rename 제안에는 비고로 `FormerlySerializedAs 필요`를 붙인다.
  (유니티는 필드명으로 값을 저장하므로 이름만 바꾸면 인스펙터 값이 사라진다.)

---

## 1. 네이밍

### 클래스 / 구조체 / 열거형
```csharp
public class PlayerController { }
public struct DamageMessage { }
public enum MaskType { Human, Animal, Ghost }
```
- PascalCase
- 역할을 명확히 드러내는 이름 (`Manager`, `Controller`, `Handler`, `Data` 등)
- **파일 하나당 public 타입은 하나만.** 파일명과 그 타입명이 반드시 일치해야 함
  (클래스든 구조체든 열거형이든 동일)
- 예외: 그 타입만 쓰는 전용 enum·struct는 같은 파일 안에 둘 수 있음
- 구조체: 명사 (`DamageMessage`, `PlayerStats`)
- 열거형: **무엇의 종류인지 접미사 필수** — `~Type`, `~Mode`, `~State`

### ScriptableObject
```csharp
public class HumanMaskData : ScriptableObject { }
public class InputData : ScriptableObject { }
```
- 데이터만 담는 ScriptableObject는 `Data` 접미사로 통일
- 에셋 파일명도 클래스명과 동일하게

### 인터페이스
```csharp
public interface IMaskSkill { }
public interface IPlayerInput { }
```
- `I` 접두사 + PascalCase
- 기능을 묘사하는 형용사 사용 권장
- 이름은 쉬운 단어로 (이 절 마지막 '쉬운 단어로 이름 짓기' 적용)

### 메서드
```csharp
public void TakeDamage(int amount) { }
private void HandlePlayerDied() { }
private IEnumerator CoPlayDamageEffect() { }

// bool을 반환하는 메서드는 질문 형태
public bool IsGameOver() { }
public bool HasStartedTurn() { }
```
- PascalCase
- 반드시 동사로 시작 (`Get`, `Set`, `Find`, `Handle`, `Play` 등)
- 이벤트 핸들러: `Handle` 접두사
- 코루틴: `Co` 접두사
- bool 반환 메서드: `Is`, `Has`, `Can` 등 질문 형태

### 필드
```csharp
// 멤버 필드(private / protected / [SerializeField] / [Inject]) — _camelCase
// 어트리뷰트는 같은 줄에 붙여 쓴다 (프로젝트 규칙)
[Inject] private AudioChannel _audioChannel;
[SerializeField] private Rigidbody _rigidbody;
[SerializeField, Range(0f, 100f)] private float _moveSpeed;

private int _currentHealth;
protected bool _isGrounded;

// 상수 — PascalCase (예외)
private const int MaxSlotCount = 5;
private const string RunParam = "IsRunning";
```

**직렬화 필드 rename 규칙:**
- 대상: `public` 필드 / `[SerializeField]` 필드 / `[field: SerializeField]` 프로퍼티
- 유니티는 **필드명을 키로** 값을 저장한다. 이름만 바꾸면 씬·프리팹·에셋에 저장된 값과 오브젝트 참조가 전부 날아간다.
- 이름을 바꿀 때는 `[FormerlySerializedAs("이전이름")]`을 붙인다. (`using UnityEngine.Serialization;`)
- 해당 에셋을 모두 열어 재저장(마이그레이션)한 뒤에는 어트리뷰트를 제거한다. 남겨두면 영구 부채가 된다.

```csharp
[FormerlySerializedAs("_speed")]
[SerializeField] private float _moveSpeed;
```

**참조 획득 규칙:**

| 대상 | 방법 |
|---|---|
| 외부 시스템·서비스 참조 | `[Inject]` |
| 자기 GameObject의 컴포넌트 (`Rigidbody`, `Animator` 등) | `[SerializeField]` 또는 `Awake`의 `GetComponent` |
| 인스펙터에서 개별 조정할 값 / 테스트용 값 | `[SerializeField]` |
| 값이 묶음으로 존재하는 시스템 데이터 | ScriptableObject(`~Data`)로 그룹화 |

**한정자 규칙:**

> 대원칙: 한정자는 **정의대로만** 쓴다. 부가효과는 사용 이유가 될 수 없다.
> - `static` = 타입에 속함(인스턴스마다가 아니라 타입당 하나). "멤버 변수를 안 써서"는 사유 불가
> - `readonly` = 불변 의도. "지금 안 바꿔서"는 사유 불가
> - `private` = 캡슐화 의도. "밖에서 안 써서"는 사유 불가
> - `sealed` = 상속 금지 의도. "자식 클래스가 없어서"는 사유 불가
>
> 이유 주석에는 **정의에 해당하는 이유**를 적는다. 부가효과를 적으면 위반으로 본다.

- `public` 인스턴스 필드: 원칙 금지. 쓴다면 **왜 public이어야 하는지 주석 필수.** 이유 없으면 삭제
- `const`: 허용·권장 (매직넘버·매직스트링 제거 용도)
- `static`, `static readonly`: **왜 타입에 속해야 하는지 주석 필수.** 이유 없으면 삭제

**변수 네이밍 원칙:**
- 명사를 사용 (bool 제외)
- bool은 동사 접두사: `isDead`, `isWalking`, `hasDamageMultiplier`
- 변수 하나당 선언 한 줄
- 클래스명이 `Player`면 멤버 변수에 `PlayerScore` 대신 `Score` 사용 (중복 금지)

**약어 규칙:**
- 아래 화이트리스트만 허용. 목록 밖 약어는 금지
  `id`, `ui`, `hp`, `mp`, `bgm`, `sfx`, `fx`, `hud`, `ai`, `db`, `url`
- 새 약어가 필요하면 이 목록에 먼저 추가한다
- 표기: 3글자 이상 약어는 첫 글자만 대문자 — `_bgmVolume`, `PlayBgm()`, `HudPanel`
- 그 외 예외: 루프 카운터(`i`, `j`), 수식 변수(`x`, `y`, 보간 비율 `t`)

```csharp
// X
RectTransform rt;
int idx;

// O
RectTransform panelRect;
int index;
```

### 프로퍼티
```csharp
public int CurrentHealth { get; private set; }
public bool IsAlive => CurrentHealth > 0;

public int MaxHealth => _maxHealth;
private int _maxHealth;

public int MaxHealth
{
    get => _maxHealth;
    set => _maxHealth = value;
}
```
- PascalCase
- 단일 라인 읽기 전용은 `=>` expression-bodied 사용
- 멀티라인은 `{ get; set; }` 구문 사용

### 이벤트
```csharp
// 선언 — On + 과거형 또는 동사원형
public event Action OnDied;
public event Action<int> OnHealthChanged;

// 구독(observer) — Handle + 이벤트명
private void HandleDied() { }
private void HandleHealthChanged(int value) { }
```

### 열거형 (Enum)
```csharp
// 단수 명사 + 종류 접미사
public enum WeaponType
{
    Knife,
    Gun,
    RocketLauncher,
}

// 비트와이즈 enum은 복수형 (Flags 어트리뷰트 사용 시)
[Flags]
public enum AttackModes
{
    None    = 0,
    Melee   = 1,
    Ranged  = 2,
    Special = 4,
    MeleeAndSpecial = Melee | Special
}
```

### 지역 변수 / 매개변수
```csharp
void ApplyDamage(int damageAmount)
{
    int newHealth = _currentHealth - damageAmount;
    RectTransform panelRect = _panel.GetComponent<RectTransform>();
}
```
- camelCase
- 약어 규칙은 필드와 동일 (위 화이트리스트 적용)

### 쉬운 단어로 이름 짓기 (클래스 / 메서드 / 변수 전부 해당)

이 프로젝트는 한국인 개발자가 읽음. **중학교 수준 영단어**로 이름을 지어라.
영어 사전을 찾아봐야 뜻을 아는 단어는 쓰지 마라.

```csharp
// X — 뜻은 정확하지만 사전을 찾아야 하는 단어
public void InstantiateProjectileEntity() { }
private bool _isTraversalOccluded;

// O — 같은 뜻, 쉬운 단어
public void SpawnBullet() { }
private bool _isPathBlocked;
```

| 어려운 말 | 쉬운 말 |
|---|---|
| Instantiate / Initiate | Create, Spawn, Start |
| Terminate / Dispose | Stop, Clear, Remove |
| Retrieve / Acquire | Get, Find |
| Validate / Verify | Check, IsValid |
| Propagate | Send, Notify |
| Accumulate | AddUp, Total |
| Occlude | Block, Hide |
| Threshold | Limit, MinValue, MaxValue |

- 단, Unity·C#이 이미 쓰는 이름(`Instantiate`, `Dispose`, `Serialize` 등)은 그대로 둔다.
- 도메인 용어(기획서에 나오는 말: `Mask`, `Trigger`, `Hitbox` 등)도 그대로 둔다.

---

## 2. 파일 구조 (클래스 내부 순서)

> 근거: Unity 공식 e-book 동반 예제 StyleExample.cs
> "Organize your class in the following order: Fields, Properties, Events,
> Monobehaviour methods, public methods, private methods, other Classes."

```csharp
public class PlayerController : MonoBehaviour
{
    // 1. 상수
    private const float DefaultSpeed = 5f;

    // 2. 필드 — SerializeField / Inject 먼저, 그다음 순수 private
    [SerializeField] private Rigidbody _rigidbody;
    [Inject] private AudioChannel _audioChannel;

    private int _currentHealth;
    private bool _isGrounded;

    // 3. 프로퍼티
    public int CurrentHealth { get; private set; }

    // 4. 이벤트
    public event Action OnDied;

    // 5. MonoBehaviour 메서드 (실행 순서대로)
    private void Awake() { }
    private void OnEnable() { }
    private void Start() { }
    private void Update() { }
    private void FixedUpdate() { }
    private void LateUpdate() { }
    private void OnDisable() { }
    private void OnDestroy() { }

    // 6. public 메서드
    public void TakeDamage(int amount) { }

    // 7. private 메서드 (이벤트 핸들러·코루틴 포함)
    private void Die() { }
    private void HandleMaskChanged(MaskType mask) { }
    private IEnumerator CoDie() { }

    // 8. 이 타입 전용 중첩 타입
}
```

---

## 3. 접근 제한자

```csharp
private void Update() { }      // O
void Update() { }              // X

private int _health;           // O
int _health;                   // X
```
- 항상 명시. 생략 금지

---

## 4. 중괄호

Allman 스타일 — 항상 새 줄, 생략 금지
```csharp
// O
if (isGrounded)
{
    Jump();
}

// X — 생략
if (isGrounded)
    Jump();

// X — K&R
if (isGrounded) {
    Jump();
}
```

### Switch 문
```csharp
switch (someExpression)
{
    case 0:
        DoSomething();
        break;
    case 1:
        DoSomethingElse();
        break;
}
```
- `case`는 `switch`에서 한 단계 들여쓰기

---

## 5. 간격 (Spacing)

### 수평 간격
```csharp
CollectItem(myObject, 0, 1);   // O — 쉼표 뒤 공백
CollectItem(myObject,0,1);     // X

DropPowerUp(myPrefab, 0, 1);   // O — 괄호 안쪽 공백 없음
DropPowerUp( myPrefab, 0, 1 ); // X

DoSomething();                 // O — 함수명과 괄호 사이 공백 없음
DoSomething ();                // X

x = dataArray[index];          // O — 배열 인덱스 안쪽 공백 없음
if (x == y) { }                // O — 비교 연산자 앞뒤 공백
```

### 수직 간격
- 관련 메서드끼리 묶기
- 변수 선언부와 메서드 사이 빈 줄 하나
- 클래스/인터페이스 사이 빈 줄 두 개
- 한 줄 최대 길이: 120자 권장

## 6. #region

사용 비권장.
클래스가 region으로 나눠야 할 만큼 크다면 클래스를 분리해야 한다는 신호.

## 7. 네임스페이스

이 프로젝트의 네임스페이스는 `Refactoring` 하나로 고정한다.
```csharp
namespace Refactoring
{
    public class GameManager : MonoBehaviour { }
}
```
