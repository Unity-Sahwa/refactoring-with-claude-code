# 코드 컨벤션

> 기준: Unity 공식 e-book ("Create a C# Style Guide") + 커뮤니티 관행 절충
> private 필드는 `_camelCase` (커뮤니티 관행 채택)
> `Old/` 폴더와 실험용(`_Test`) 코드는 이 컨벤션 검사 대상에서 제외한다(삭제 예정 레거시).

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
- MonoBehaviour가 있는 파일은 파일명과 클래스명이 반드시 일치해야 함
- 파일 하나당 MonoBehaviour는 하나만 존재해야 함

### ScriptableObject
```csharp
// 클래스명: Data 접미사
public class HumanMaskData : ScriptableObject { }
public class InputData : ScriptableObject { }
public class SoundData : ScriptableObject { }

// 에셋 파일명도 동일하게: HumanMaskData, InputData ...
```
- 데이터만 담는 ScriptableObject는 `Data` 접미사로 통일
- MonoBehaviour와 구분은 `: ScriptableObject` 상속으로 판단

### 인터페이스
```csharp
public interface IMaskSkill { }
public interface IPlayerInput { }
```
- `I` 접두사 + PascalCase
- 기능을 묘사하는 형용사 사용 권장

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
// public 인스턴스 필드 — 사용 금지 (10절 참고, SerializeField로 대체)

// 모든 멤버 필드(private / protected / [SerializeField] private / [Inject]) — _camelCase (줄 분리)
[SerializeField]
private Rigidbody _rigidbody;

[SerializeField, Range(0f, 100f)]
private float _moveSpeed;

[Inject]
private AudioChannel _audioChannel;

private int _currentHealth;
protected bool _isGrounded;

// 상수 — PascalCase (예외)
private const int MaxSlotCount = 5;
public const string SavePath = "SaveData";

// static readonly — PascalCase (예외)
private static readonly WaitForSeconds WaitOneSecond = new WaitForSeconds(1f);
```

**변수 네이밍 원칙:**
- 명사를 사용 (bool 제외)
- bool은 동사 접두사: `isDead`, `isWalking`, `hasDamageMultiplier`
- 의미 있는 이름 사용 — 약어 금지 (루프/수식 제외)
  ```csharp
  // X
  int hp;
  string tName;

  // O
  int healthPoints;
  string teamName;
  ```
- 변수 하나당 선언 한 줄 (가독성 우선)
- 클래스명이 `Player`면 멤버 변수에 `PlayerScore` 대신 `Score` 사용 (중복 금지)

### 프로퍼티
```csharp
// 단순 읽기 전용 — expression-bodied
public int CurrentHealth { get; private set; }
public bool IsAlive => CurrentHealth > 0;

// backing field가 있는 경우
private int _maxHealth;
public int MaxHealth => _maxHealth;

// getter/setter 모두 필요한 경우
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
public event Action<bool> OnControlDisabled;

// 이벤트 발생(subject) — On 접두사 메서드
public void OnDoorOpened()
{
    DoorOpened?.Invoke();
}

// 구독(observer) — Handle + 이벤트명
private void HandleDied() { }
private void HandleHealthChanged(int value) { }
```

### 열거형 (Enum)
```csharp
// 단수 명사 사용
public enum WeaponType
{
    Knife,
    Gun,
    RocketLauncher,
}

public enum FireMode
{
    None = 0,
    Single = 5,
    Burst = 7,
    Auto = 8,
}

// 비트와이즈 enum은 복수형 (Flags 어트리뷰트 사용 시)
[Flags]
public enum AttackModes
{
    None    = 0,  // 000000
    Melee   = 1,  // 000001
    Ranged  = 2,  // 000010
    Special = 4,  // 000100
    MeleeAndSpecial = Melee | Special
}
```
- 일반 enum: 단수 명사 + PascalCase
- `[Flags]` enum: 복수형

### 지역 변수 / 매개변수
```csharp
void ApplyDamage(int damageAmount)
{
    int newHealth = _currentHealth - damageAmount;
}
```
- camelCase

---

## 2. 파일 구조 (클래스 내부 순서)

```csharp
public class PlayerController : MonoBehaviour
{
    // 1. 이벤트
    public event Action OnDied;

    // 2. 상수
    private const float DefaultSpeed = 5f;

    // 3. SerializeField (Inspector 노출)
    [SerializeField]
    private Rigidbody _rigidbody;

    // 4. private 필드
    private int _currentHealth;
    private bool _isGrounded;

    // 5. 프로퍼티
    public int CurrentHealth { get; private set; }

    // 6. Unity 라이프사이클 (순서 준수)
    private void Awake() { }
    private void OnEnable() { }
    private void Start() { }
    private void Update() { }
    private void FixedUpdate() { }
    private void LateUpdate() { }
    private void OnDisable() { }
    private void OnDestroy() { }

    // 7. public 메서드
    public void TakeDamage(int amount) { }

    // 8. private 메서드
    private void Die() { }

    // 9. 이벤트 핸들러
    private void HandleMaskChanged(MaskType mask) { }

    // 10. 코루틴
    private IEnumerator CoDie() { }
}
```

---

## 3. 접근 제한자

```csharp
// 항상 명시 — 생략 금지
private void Update() { }      // O
void Update() { }               // X

private int _health;            // O
int _health;                    // X
```

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

// 중첩도 반드시 중괄호
for (int i = 0; i < 10; i++)
{
    for (int j = 0; j < 10; j++)
    {
        ExampleAction();
    }
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
    case 2:
        int n = 1;
        DoAnotherThing(n);
        break;
}
```
- `case`는 `switch`에서 한 단계 들여쓰기

---

## 5. 간격 (Spacing)

### 수평 간격
```csharp
// O — 쉼표 뒤 공백
CollectItem(myObject, 0, 1);

// X
CollectItem(myObject,0,1);

// O — 괄호 안쪽 공백 없음
DropPowerUp(myPrefab, 0, 1);

// X
DropPowerUp( myPrefab, 0, 1 );

// O — 함수명과 괄호 사이 공백 없음
DoSomething();

// X
DoSomething ();

// O — 배열 인덱스 안쪽 공백 없음
x = dataArray[index];

// O — 조건문/비교 연산자 앞뒤 공백
while (x == y) { }
if (x == y) { }
```

### 수직 간격
- 관련 메서드끼리 묶기 (같은 기능 담당 메서드는 인접 배치)
- 변수 선언부와 메서드 사이 빈 줄 하나
- 클래스/인터페이스 사이 빈 줄 두 개
- 가독성에 도움이 될 때만 if-else 블록 사이 빈 줄 추가
- 한 줄 최대 길이: 120자 권장

---

## 6. var 사용

타입이 오른쪽에서 명확히 보일 때만 허용
```csharp
var player = new Player();                    // O — 타입 보임
var rb = GetComponent<Rigidbody>();           // O — 타입 보임
var powerUps = new List<PowerUps>();          // O — 타입 보임

var data = LoadData();                        // X — 타입 불명확
var result = _saveManager.GetSlotInfo(0);    // X — 타입 불명확
```

---

## 7. null 체크

```csharp
// null 조건 연산자 허용
_animator?.SetBool("IsRunning", true);
OnDied?.Invoke();

// 단, 복잡한 체이닝은 가독성 위해 분리
if (_animator == null)
{
    return;
}
```

---

## 8. #region

사용 비권장.
클래스가 region으로 나눠야 할 만큼 크다면 클래스를 분리해야 한다는 신호.

---

## 9. 주석

```csharp
// 한국어 혼용 허용 (이 프로젝트 규칙)

// 나쁜 주석 — 코드 반복
_health -= amount; // 체력을 amount만큼 감소시킨다

// 좋은 주석 — 이유 설명
// 슈퍼아머 상태에서는 경직이 없으므로 피격 애니메이션만 재생
if (_playerState.HasSuperArmor)
{
    PlayHitAnimation();
}
```

**주석 규칙:**
- `//` 뒤에 공백 하나 (` // 내용`)
- 가능하면 코드 줄 끝이 아닌 별도 줄에 작성
- 코드 자체가 설명이 되면 주석 생략 (잘 지은 이름이 최고의 주석)
- 주석 처리된 코드는 제거 (버전 관리로 복원 가능)
- TODO 주석은 완료 시 즉시 제거

**주석 밀도 규칙:**
- 메서드마다 그 위에 한 줄 요약을 단다. 단, 이름만으로 자명한 메서드
  (Awake의 단순 참조 연결, 단순 위임 등)는 생략한다.
- 비직관적 로직 라인(트릭, 수식, 한눈에 안 들어오는 API)엔 그 줄 위에 설명을 단다.
- "왜 이렇게 구현했나"는 선택이 비직관적일 때만 단다.
  (모든 메서드에 이유를 달면 13절 '과도한 주석' 스멜이 된다.)

```csharp
// XML summary — public 메서드에 사용 가능 (IntelliSense 지원)
/// <summary>
/// 플레이어에게 데미지를 입힌다.
/// </summary>
public void TakeDamage(int amount) { }

// SerializeField 설명은 Tooltip으로 대체
[Tooltip("이동 속도 (m/s)")]
[SerializeField]
private float _moveSpeed;
```

---

## 10. SerializeField vs public

```csharp
// O — SerializeField로 Inspector 노출, 외부 접근 차단
[SerializeField]
private Transform _target;

// O — Range로 Inspector 슬라이더 표시
[SerializeField, Range(0f, 100f)]
private float _health;

// O — Serializable 구조체로 Inspector 그룹화
[Serializable]
public struct PlayerStats
{
    public int MovementSpeed;
    public int HitPoints;
    public bool HasHealthPotion;
}

[SerializeField]
private PlayerStats _stats;

// X — 불필요한 public 노출
public Transform target;
```

---

## 11. 메서드 원칙

- **인자 수 최소화**: 인자가 많을수록 복잡도 증가. 인자 수를 줄여 가독성과 테스트 용이성 확보
- **사이드 이펙트 금지**: 메서드 이름에 명시된 동작만 수행. 외부 상태를 예상 밖으로 변경하지 않음
- **flag 대신 별도 메서드**: bool 플래그로 두 가지 모드를 처리하지 말고, 이름이 명확한 두 메서드로 분리
  ```csharp
  // X
  float GetAngle(bool inDegrees) { }

  // O
  float GetAngleInDegrees() { }
  float GetAngleInRadians() { }
  ```
- **DRY (Don't Repeat Yourself)**: 중복 로직은 공통 메서드로 추출
  ```csharp
  // X — 로직 중복
  private void PlayExplosionA(Vector3 pos) { ... }
  private void PlayExplosionB(Vector3 pos) { ... }

  // O — 공통 추출
  private void PlayFXWithSound(ParticleSystem particle, AudioClip clip, Vector3 pos) { ... }
  ```
- **expression-bodied(`=>`) 사용 조건**: 본문이 한 문장이고 한 줄에 들어갈 때만 쓴다.
  분기·반복이 있거나 여러 문장이면 중괄호 블록으로 쓴다.
  (근거: Microsoft C# 가이드, C# Coding Guidelines AV2410)
- **Extension 메서드**: Unity API 확장 시 static 클래스에 정의
  ```csharp
  public static class TransformExtensions
  {
      public static void ResetTransformation(this Transform transform)
      {
          transform.position = Vector3.zero;
          transform.localRotation = Quaternion.identity;
          transform.localScale = Vector3.one;
      }
  }
  ```

---

## 12. 네임스페이스

```csharp
namespace Refactoring
{
    public class GameManager : MonoBehaviour { }
}

namespace Refactoring
{
    public class HpHUD : MonoBehaviour { }
}
```
- PascalCase, 특수문자/언더스코어 없음
- 서드파티 충돌 방지를 위해 프로젝트 전체에 일관된 네임스페이스 적용
- 서브 네임스페이스: `Sahwa.Core`, `Sahwa.UI`, `Sahwa.AI` 등 점(`.`)으로 계층화
- 파일 상단 `using` 지시어로 반복 타이핑 방지

---

## 13. 코드 스멜 (Common Pitfalls)

코드 스멜은 문제 있는 코드가 숨어있을 수 있다는 징후다. 아래 증상이 보이면 리팩토링을 고려해라.

| 스멜 | 설명 |
|------|------|
| **불가사의한 네이밍** | 클래스, 메서드, 변수명이 의도를 드러내지 않는다. |
| **불필요한 복잡성** | 모든 가능성을 예측하려 과도하게 설계한다. 긴 메서드, 모든 걸 하는 God 클래스. |
| **경직성** | 작은 변경이 여러 곳의 수정을 요구한다. 단일 책임 원칙 위반 신호. |
| **취약성** | 사소한 변경으로 관계없는 곳이 깨진다. |
| **이동 불가능성** | 코드를 다른 곳에 재사용하려면 의존성이 너무 많이 따라온다. |
| **중복 코드** | 복사-붙여넣기한 로직이 보인다. 공통 메서드로 추출해라. |
| **과도한 주석** | 모든 줄에 주석이 달려있다. 잘 지어진 이름이 최고의 주석이다. |