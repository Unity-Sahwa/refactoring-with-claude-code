# 디자인 패턴 가이드

> 이 코드베이스의 결합도를 낮추고 확장성을 높이기 위해 적용할 패턴들

---

## 1. Observer Pattern (옵저버)

**적용 대상**: SaveManager→MenuUI, Player→HUD, 스킬→UIEffect, SceneSwitcher→각 시스템

**현재 문제**
```csharp
// SaveManager가 UI를 직접 호출
menuUI.RecordSlot(index, sceneIndex, areaIndex, time);

// Player 사망 시 MenuUI를 직접 호출
MenuUI.instance.DisablePlayerControl(true);
```

**패턴 적용 후**
```csharp
// SaveManager — 이벤트 발행만
public event Action<SaveSlotData> OnSlotSaved;
public void SaveSloatData()
{
    // 저장 로직...
    OnSlotSaved?.Invoke(slotData);
}

// SaveSlotUI — 구독해서 처리
void Start()
{
    saveManager.OnSlotSaved += UpdateSlot;
}

// Player — 이벤트 발행만
public event Action OnDied;
public event Action<float> OnHPChanged;

// HpHUD — 구독
player.OnHPChanged += ChangeHPStack;

// GameState — 구독
player.OnDied += () => SetPlayerControl(false);
```

**효과**
- SaveManager, Player는 UI/MenuUI를 전혀 알 필요 없음
- 새 구독자 추가 시 기존 코드 수정 불필요

---

## 2. State Pattern (상태)

**적용 대상**: MenuUI.MenuSwitch(), 플레이어 상태 전환

**현재 문제**
```csharp
// ESC 키 하나에 120줄 if-else
public void MenuSwitch()
{
    if (loadingUI.LoadBG.activeSelf) return;
    else if (settingWindow.activeSelf) { ... }
    else if (loadSlotWindow.activeSelf) { ... }
    else if (quitWindow.activeSelf) { ... }
    else if (pauseMenu.activeSelf) { ... }
    else if (mainMenu.activeSelf) { ... }
    else { /* 포즈 진입 */ }
}
```

**패턴 적용 후**
```csharp
public abstract class UIScreen : MonoBehaviour
{
    public abstract void OnShow();
    public abstract void OnHide();
    public abstract void OnBackPressed(); // ESC
}

public class PauseMenuScreen : UIScreen
{
    public override void OnBackPressed() => Hide();
}

public class SettingScreen : UIScreen
{
    public override void OnBackPressed() => SaveAndHide();
}

public class UIManager : MonoBehaviour
{
    private Stack<UIScreen> screenStack = new();

    public void Push(UIScreen screen) { screen.OnShow(); screenStack.Push(screen); }
    public void Pop() { screenStack.Peek().OnBackPressed(); screenStack.Pop(); }
    // ESC → 항상 Pop()
}
```

**효과**
- ESC 동작이 각 화면 클래스에 캡슐화됨
- 새 화면 추가 시 UIScreen 상속만 하면 됨

---

## 3. Strategy Pattern (전략)

**적용 대상**: PlayerController 마스크별 입력 처리, MaskChange 전환 로직

**현재 문제**
```csharp
// 마스크 추가 시 이 블록을 계속 수정해야 함
if (maskChange.CurrentMask == maskChange.HumanMask)
{
    humanMaskSkill.NormalAttack();
    humanMaskSkill.DashCooldown();
    ...
}
else
{
    animalMaskSkill.NormalAttack();
    animalMaskSkill.DashCooldown();
    ...
}
```

**패턴 적용 후**
```csharp
public interface IMaskSkill
{
    void NormalAttack();
    void SpecialAttack();
    void Dash();
    void HandleCooldowns();
    void InitializeSkill();
}

public class HumanMaskSkill : PlayerSkill, IMaskSkill { ... }
public class AnimalMaskSkill : PlayerSkill, IMaskSkill { ... }

// PlayerController
private IMaskSkill currentMaskSkill;

void Update()
{
    if (Input.GetKeyDown(attackKey)) currentMaskSkill.NormalAttack();
    if (Input.GetKeyDown(dashKey))   currentMaskSkill.Dash();
    currentMaskSkill.HandleCooldowns();
}

// 마스크 전환 시
public void OnMaskChanged(IMaskSkill newSkill)
{
    currentMaskSkill = newSkill;
}
```

**효과**
- 세 번째 마스크 추가 → IMaskSkill 구현 클래스 하나 추가, PlayerController 수정 없음

---

## 4. Command Pattern (커맨드)

**적용 대상**: PlayerController 입력 처리

**현재 문제**
```csharp
// Update()에 입력 + 실행이 섞여있음
if (Input.GetKeyDown(saveManager.InputKeys[KeyAction.ATTACK_NORMAL]))
{
    playerSkillInput.StoreInput(PlayerStateType.HUMAN_NORMALATTACK);
    if (!playerState.isPerfomingSklill) humanMaskSkill.NormalAttack();
}
```

**패턴 적용 후**
```csharp
public interface ICommand
{
    void Execute();
}

public class NormalAttackCommand : ICommand
{
    private IMaskSkill skill;
    public NormalAttackCommand(IMaskSkill skill) { this.skill = skill; }
    public void Execute() => skill.NormalAttack();
}

// PlayerInputHandler
private Dictionary<KeyAction, ICommand> commandMap;

void Update()
{
    foreach (var pair in commandMap)
    {
        if (Input.GetKeyDown(inputSettings.GetKey(pair.Key)))
            pair.Value.Execute();
    }
}
```

**효과**
- 입력과 실행 로직 완전 분리
- 키 리바인딩, 입력 큐, 모바일/PC 동일 처리 가능

---

## 5. Template Method Pattern (템플릿 메서드)

**적용 대상**: PlayerSkill 스킬 실행 흐름

**현재 문제**
PlayerSkill 베이스에 공통 흐름 정의가 없고,
각 스킬 클래스가 각자 방식으로 쿨다운/실행을 구현.

**패턴 적용 후**
```csharp
public abstract class PlayerSkill : MonoBehaviour
{
    // 흐름은 고정 (변하지 않는 것)
    public void TryExecute()
    {
        if (!CanUse()) return;
        OnSkillStart();
        StartCoroutine(ExecuteCoroutine());
        StartCooldown();
    }

    // 세부 구현은 자식이 (변하는 것)
    protected abstract bool CanUse();
    protected abstract void OnSkillStart();
    protected abstract IEnumerator ExecuteCoroutine();
    protected virtual void StartCooldown() { /* 기본 구현 */ }
}
```

**효과**
- 모든 스킬이 동일한 흐름을 따름
- 각 스킬은 "뭘 하는지"만 구현, "언제 하는지"는 베이스가 보장

---

## 6. Repository Pattern (레포지토리)

**적용 대상**: SaveManager 저장/로드

**현재 문제**
저장 방식(CSV 파일)이 SaveManager 내부에 직접 구현되어 있어
포맷 변경 시 SaveManager 전체를 수정해야 한다.

**패턴 적용 후**
```csharp
public interface ISaveRepository
{
    void Save(int slot, GameSaveData data);
    GameSaveData Load(int slot);
    bool Exists(int slot);
    void Delete(int slot);
}

public class JsonFileSaveRepository : ISaveRepository
{
    public void Save(int slot, GameSaveData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(GetPath(slot), json);
    }
    // ...
}

// 테스트/개발용
public class PlayerPrefsSaveRepository : ISaveRepository { ... }

// SaveManager는 인터페이스에만 의존
public class SaveManager : MonoBehaviour
{
    [SerializeField] private ISaveRepository repository;
}
```

**효과**
- 저장 방식 교체 시 Repository 구현체만 바꾸면 됨
- 테스트 시 가짜 Repository 사용 가능

---

## 7. ScriptableObject Data-Driven (데이터 주도 설계)

**적용 대상**: MaskChange, 스킬 데이터, Area 이름, 씬별 카메라 값

**현재 문제**
```csharp
// 마스크 종류, 스킬 종류, Area 이름이 코드 안에 하드코딩
areaName = areaIndex switch { 0 => "은혜 굴 샛길", 1 => "은혜 굴 입구 1", ... };
bool isBossScene = (SceneManager.GetActiveScene().buildIndex == 4);
```

**패턴 적용 후**
```csharp
[CreateAssetMenu]
public class MaskDefinition : ScriptableObject
{
    public MaskType type;
    public GameObject characterPrefab;
    public IMaskSkill skillComponent;
    public GameObject[] effects;
}

[CreateAssetMenu]
public class AreaLocalizationTable : ScriptableObject
{
    public string[] koreanNames;
    public string[] englishNames;
    public string GetName(int index, bool isKorean) => ...;
}

[CreateAssetMenu]
public class SceneCameraSettings : ScriptableObject
{
    public int buildIndex;
    public CameraOrbitData[] orbits;
}
```

**효과**
- 기획 데이터가 코드 밖으로 분리
- 새 마스크/Area/씬 추가 시 코드 수정 없이 에셋만 추가

---

## 8. Facade Pattern (파사드)

**적용 대상**: SceneSwitcher, 복잡한 초기화 시퀀스

**현재 문제**
SceneSwitcher가 씬 전환 시 Player, Camera, UI, HUD, Cursor를 직접 조작.

**패턴 적용 후**
```csharp
// SceneSwitcher는 단순 이벤트만 발행
public class SceneSwitcher : MonoBehaviour
{
    public static event Action OnSceneTransitionBegin;
    public static event Action OnSceneTransitionComplete;

    public void LoadScene(int index)
    {
        OnSceneTransitionBegin?.Invoke();
        StartCoroutine(DoLoad(index));
    }
}

// 각 시스템이 자신의 초기화를 직접 처리
public class PlayerSceneHandler : MonoBehaviour
{
    void OnEnable() => SceneSwitcher.OnSceneTransitionComplete += Initialize;
    void Initialize() { /* HP 복원, 위치 설정 등 */ }
}

public class CameraSceneHandler : MonoBehaviour
{
    void OnEnable() => SceneSwitcher.OnSceneTransitionComplete += Reset;
    void Reset() { /* 카메라 리셋 */ }
}
```

**효과**
- SceneSwitcher는 "전환"만 담당, 각 시스템이 자신의 초기화를 책임짐
- 새 시스템 추가 시 SceneSwitcher 수정 불필요

---

## 우선순위별 적용 순서

| 순위 | 패턴 | 적용 이유 |
|------|------|----------|
| 1 | **Observer** | MenuUI 결합 문제의 핵심 해결책. 가장 파급 효과 큼 |
| 2 | **Strategy (IMaskSkill)** | PlayerController 마스크 분기 제거, OCP 해결 |
| 3 | **ScriptableObject Data-Driven** | 하드코딩 제거, 기획 데이터 분리 |
| 4 | **State (UIManager)** | MenuSwitch 복잡도 해결 |
| 5 | **Template Method** | PlayerSkill 구조 통일 |
| 6 | **Repository** | SaveManager 저장 방식 분리 |
| 7 | **Command** | 입력 시스템 완전 분리 |
| 8 | **Facade** | SceneSwitcher 의존 정리 |

---

## 참고: Unity에서 Observer 구현 방법 비교

| 방법 | 장점 | 단점 |
|------|------|------|
| `C# event Action` | 간단, 타입 안전 | 씬 간 관리 주의 |
| `UnityEvent` | 인스펙터에서 연결 가능 | 성능 오버헤드 |
| `ScriptableObject Event` | 씬 간 독립적 | 설정 복잡 |
| `MessageBus (자체 구현)` | 유연성 최고 | 디버깅 어려움 |

이 프로젝트 규모에서는 **`C# event Action`** 이 가장 적합.
