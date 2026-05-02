# 원본 코드 분석: InputSystem

> 분석 대상: `Assets/1. Code/Scripts/InputSystem/` (Test 폴더 제외)
> 분석 기준: 유지보수성, SOLID 원칙, 객체지향, Unity 최적화

---

## 기존 기능 목록

| 기능 | 진입 메서드 | 비고 |
|---|---|---|
| 입력 액션 열거 정의 | `InputActionType` enum | Movement, NormalAttack, SpecialAttack, FinishAttack, Dash, LockOn, Interaction, Menu |
| 플랫폼 유형 열거 정의 | `InputPlatformType` enum | PC, Mobile |
| InputActionAsset 중앙 보유 및 액션 조회 | `InputBindingData.GetAction()` | enum 이름 자동 탐색 + 커스텀 매핑 |
| InputActionAsset 활성화/비활성화 | `InputBindingData.OnEnable() / OnDisable()` | ScriptableObject 생명주기 기반 |
| 바인딩 override JSON 저장 | `InputBindingData.SaveOverrideJson()` | `SaveBindingOverridesAsJson()` 위임 |
| 바인딩 override JSON 적용 | `InputBindingData.ApplyOverrideJson()` | `LoadBindingOverridesFromJson()` 위임 |
| 입력 이벤트 브로드캐스트 (PC/조이스틱) | `InputEventBroadcaster.SubscribeActions()` | performed/canceled 콜백 구독 |
| 입력 이벤트 브로드캐스트 (모바일 버튼) | `InputEventBroadcaster.SubscribeMobileButton()` | IMobileButton 이벤트 구독 |
| 이동 입력 벡터 브로드캐스트 | `InputEventBroadcaster.BroadcastMove()` | `OnMoveInput` 이벤트 발행 |
| 입력 차단 처리 | `InputEventBroadcaster.HandlePressed() / HandleReleased()` | IInputBlocker.IsInputBlocked 조회 |
| 이벤트 구독 해제 (메모리 정리) | `InputEventBroadcaster.OnDestroy()` | action/mobile 양쪽 구독 해제 |
| 런타임 키 리바인딩 시작 | `InputRebindingService.StartRebinding()` | PerformInteractiveRebinding() |
| 리바인딩 취소 | `InputRebindingService.CancelRebinding()` | 진행 중 operation 취소 |
| 리바인딩 완료 후 저장 | `InputRebindingService.HandleRebindComplete()` | IInputSaveHandler로 위임 저장 |
| 주입 후 저장된 바인딩 복원 | `InputRebindingService.Inject()` | LoadBindings → ApplyOverrideJson |
| 리바인딩 완료 이벤트 발행 | `InputRebindingService.OnBindingChanged` | platform 정보 포함 |
| 모바일 UI 버튼 눌림/뗌 이벤트 발행 | `MobileButtonBinder.OnPointerDown() / OnPointerUp()` | IPointerDownHandler/IPointerUpHandler |
| 입력 전체 차단 여부 제공 | `IInputBlocker.IsInputBlocked` | GameManager 구현 예정 |
| 키 바인딩 저장/로드 위임 | `IInputSaveHandler.SaveBindings() / LoadBindings()` | SaveManager 구현 예정 |
| 모바일 버튼 이벤트 노출 | `IMobileButton.OnButtonDown / OnButtonUp` | MobileButtonBinder 구현 |

---

## 문제 분석

### [결합도] InputBindingData가 ScriptableObject임에도 런타임 상태를 직접 관리

```csharp
// InputBindingData.cs
private void OnEnable()
{
    BuildCache();
    _actionAsset?.Enable();
}
```

ScriptableObject는 에셋 단위 데이터 컨테이너이나 `_actionAsset.Enable()`로 런타임 활성화 상태를 직접 관리한다. 여러 씬에서 동일 에셋을 참조할 때 활성화 충돌이 발생할 수 있다.

---

### [SOLID-SRP] InputBindingData가 데이터 보관·활성화·저장을 동시에 담당

```csharp
// InputBindingData.cs
public string SaveOverrideJson() => _actionAsset?.SaveBindingOverridesAsJson() ?? string.Empty;
public void ApplyOverrideJson(string json) { ... _actionAsset?.LoadBindingOverridesFromJson(json); }
private void BuildCache() { ... }
```

캐시 구성(BuildCache), 액션 활성화(OnEnable/OnDisable), override JSON 저장·로드까지 세 가지 책임이 혼재한다. 저장 관련 기능은 `IInputSaveHandler` 또는 별도 서비스로 분리되어야 한다.

---

### [결합도] TargetTypes 프로퍼티가 호출마다 new List 인스턴스 생성

```csharp
// InputEventBroadcaster.cs
public List<Type> TargetTypes => new List<Type> { typeof(IInputBlocker), typeof(IMobileButton) };

// InputRebindingService.cs
public List<Type> TargetTypes => new List<Type> { typeof(IInputSaveHandler) };
```

DI 시스템이 이 프로퍼티를 반복 조회하면 매번 새 List가 할당된다. 불필요한 GC 압박을 유발한다.

---

### [결합도] InputRebindingService.InterfaceTypes에 구체 클래스 타입 직접 노출

```csharp
// InputRebindingService.cs
public Type[] InterfaceTypes => new[] { typeof(InputRebindingService) };
```

`IInjectTarget`의 목적은 인터페이스 단위 등록이나, 구체 클래스 타입을 직접 노출한다. 이 서비스를 주입받는 쪽이 구체 타입에 의존하게 되어 인터페이스 기반 DI 원칙에 위배된다.

---

### [SOLID-OCP] Movement 처리 분기가 InputEventBroadcaster에 하드코딩

```csharp
// InputEventBroadcaster.cs
if (actionType == InputActionType.Movement)
{
    performed = ctx => BroadcastMove(ctx.ReadValue<Vector2>());
    canceled = _ => BroadcastMove(Vector2.zero);
}
else { ... }
```

새로운 Vector2 계열 액션이 추가되면 이 분기를 직접 수정해야 한다. 액션 타입별 처리 전략을 외부에서 주입하거나 `InputBindingData`에 ValueType 정보를 포함해 확장점을 열어야 한다.

---

### [Unity 최적화] _mappings 미설정 시 BuildCache가 무의미하게 실행되고 GetAction은 매번 문자열 탐색

```csharp
// InputBindingData.cs
private void BuildCache()
{
    _actionCache = new Dictionary<InputActionType, InputAction>();
    foreach (var mapping in _mappings) { ... }  // _mappings가 비어 있으면 아무것도 캐시 안 됨
}

public InputAction GetAction(InputActionType actionType)
{
    if (_actionCache != null && _actionCache.TryGetValue(actionType, out InputAction cached))
        return cached;
    return _actionAsset?.FindAction(actionType.ToString());  // 매번 문자열 탐색
}
```

`_mappings`가 비어 있는 기본 설정에서 캐시는 구성되지 않고, 이후 `GetAction()`은 항상 `FindAction()` 문자열 탐색으로 폴백한다. 캐시의 의미가 없다. 최초 접근 시 자동 캐시를 구성하는 지연 초기화가 필요하다.

---

### [결합도] MobileButtonBinder가 Inject() 미구현 상태에서 IInjectTarget을 등록 수단으로 오용

```csharp
// MobileButtonBinder.cs
public class MobileButtonBinder : MonoBehaviour, IMobileButton, IInjectTarget, IPointerDownHandler, IPointerUpHandler
{
    public Type[] InterfaceTypes => new[] { typeof(IMobileButton) };
    // Inject() 메서드 없음
}
```

`IInjectTarget`은 `InterfaceTypes` 제공과 `Inject()` 수신을 묶은 계약이다. `MobileButtonBinder`는 자신을 DI에 노출하는 용도로만 사용하며 `Inject()`를 구현하지 않는다. 인터페이스가 두 의미(등록/주입)를 동시에 담고 있어 계약이 불명확하다.

---

## 문제 요약표

| 구분 | 문제 | 심각도 |
|---|---|---|
| 결합도 | InputBindingData(ScriptableObject)가 InputActionAsset Enable/Disable 생명주기를 직접 관리 | 중 |
| SOLID-SRP | InputBindingData가 캐시 구성·액션 활성화·override JSON 처리를 단일 클래스에서 수행 | 중 |
| 결합도 | TargetTypes 프로퍼티가 호출마다 new List 생성 (InputEventBroadcaster, InputRebindingService) | 하 |
| 결합도 | InputRebindingService.InterfaceTypes에 구체 클래스 타입 직접 노출 — 인터페이스 기반 DI 원칙 위반 | 중 |
| SOLID-OCP | Movement 분기가 InputEventBroadcaster에 하드코딩 — 새 Vector2 액션 추가 시 직접 수정 필요 | 중 |
| Unity 최적화 | _mappings 미설정 시 BuildCache 무의미 실행, GetAction이 매번 문자열 FindAction 호출 | 하 |
| 결합도 | MobileButtonBinder가 Inject() 미구현 상태에서 IInjectTarget을 등록 수단으로 오용 | 하 |
