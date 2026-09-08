# 리팩토링 근거 — 의존성 주입 (레거시 대비 실측)

**주제: 전역 접근을 명시적 계약으로 바꾸기.** 샱글톤 26개를 없애고 의존을 필드 선언부에 드러나게 함.

여기 적힌 것은 양쪽 코드 실측이거나 본인 답변임. 추측은 적지 않음.

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| DI 시스템 | **없음** | `DISystem` 4파일 278줄 |
| 싱글톤 클래스 | **26개** | 0개 |
| 싱글톤 접근 호출 (`X.instance`) | **230회** | `.Instance` 잔존 1건 (풀링 객체 참조, 싱글톤 아님) |
| `DontDestroyOnLoad` | 3곳 | 0 |
| 주입 필드 | — | `[Inject]` **161개** (49파일) |
| 그중 선택적 주입 | — | 90개 |

## before: 싱글톤 26개가 의존 해결 수단이었음
`CameraController` · `CameraData` · `CoroutineManager` · `LanguageManager` · `PlatformSwitcher` · `SaveManager` ·
`SceneSwitcher` · `TextManager` · `MobileInput` · `PlayerController` · `MaskChange` · `PlayerMovement` · `Player` ·
`TimelineHelper` · `CheatData` · `PartnerData` · `PlayerAnimalMaskData` · `PlayerCommonData` · `PlayerGhostMaskData` ·
`PlayerHumanMaskData` · `UIEffectData` · `CheatMode` · `HpHUD` · `LoadingUI` · `MenuUI` · `UIEffect`

문제
- 어떤 클래스가 무엇에 의존하는지 **선언부만 봐서는 알 수 없음**. 메서드 본문의 `X.instance` 호출을 전부 읽어야 함
- 초기화 순서가 `Awake` 실행 순서에 좌우됨. 레거시 `MouseSettingUI`에
  `// TODO: TimelineHelper에서 savemanager 불러오는 문제 -> instance부터 하는건지` 주석이 남아 있음 (인지했으나 미해결)
- 데이터 클래스(`PlayerHumanMaskData` 등)까지 싱글톤이라 교체·테스트가 불가능

## after: `[Inject]` 어트리뷰트 기반 주입
| 파일 | 줄 | 책임 |
|---|---|---|
| `InjectAttribute` | 17 | 주입 표시. `Optional` 여부만 가짐 |
| `AttributeInjector` | 217 | 타입 수집 → 씬 객체·SO 등록 → 어트리뷰트 필드에 주입 |
| `DataContainer` | 33 | SO 수치 에셋 공급 |
| `IDataProvider` | 11 | SO 공급 계약 |

동작
1. `[RuntimeInitializeOnLoadMethod(AfterAssembliesLoaded)]`로 **어셈블리 로드 직후 1회** 타입과 `[Inject]` 필드를 캐싱.
   런타임 리플렉션 재조회 없음
2. 씬 `Awake`에서 `FindObjectsByType<MonoBehaviour>`로 씬 객체 등록. 인터페이스·추상 타입도 키로 등록해 계약 타입 주입 가능
3. SO는 씬 객체가 아니라 `IDataProvider`를 통해서만 수집
4. `List<T>` 필드면 해당 타입 전부를, 단일 필드면 첫 번째를 주입

효과
- 의존이 **필드 선언부에 드러남**. `[Preserve, Inject] private IHealthModifier _health;` 한 줄로 계약이 보임
- 구체 타입이 아니라 인터페이스를 요구하므로 구현 교체 가능
- 선택적 주입(`Inject(true)`) 90개로 "없어도 되는 의존"과 "필수 의존"이 구분됨.
  없으면 필수는 `LogError`, 선택은 `LogWarning`

## 스스로 기록한 한계
- 같은 타입 구현체가 여럿이면 **첫 번째만 주입되고 경고만 남음** (`AttributeInjector.InjectInstance`)
- 런타임에 생성되는 객체는 주입 대상이 아님. `Awake` 시점의 씬 객체만 스캔
- `Script Execution Order`를 -100으로 설정해야 동작함 (코드 주석에 명시)
- 부모 클래스 필드를 주입받으려면 `private`가 아닌 `protected`/`public`이어야 함 (코드 주석에 명시)

## 정리
바뀐 것은 "전역 접근을 없앴다"가 아니라 **의존이 보이게 됐다**는 점임.
before는 230번의 `X.instance` 호출이 코드 본문에 흩어져 있어 의존 관계를 읽어내려면 전부 추적해야 했음.
after는 필드 선언 161개가 그대로 의존 목록이고, 그중 90개는 없어도 되는 것으로 표시돼 있음.

## 주입 대상 분포 — 계약으로 받는가, 물건으로 받는가
after의 주입 필드 146개를 타입별로 분류함 (선언부 파싱 기준).

| 구분 | 수 | 비율 |
|---|---|---|
| 인터페이스 (`I~`) | 101 | 69% |
| `List<인터페이스>` | 6 | 4% |
| 구체 타입 | 39 | 27% |

구체 타입 39개의 내역 — 교체 대상이 아닌 것들임
- 채널: `HitChannel` 6, `AudioChannel` 5 (요청/응답 통로 그 자체)
- SO 데이터: `EffectCatalog`, `AudioCatalog`, `TextTableData`, `PlayerCameraShakeData`
- 씬 컴포넌트 목록: `List<CameraRole>` 4, `List<PlatformObject>` 2, `List<PlayerCharacter>`, `List<Button>` 등
- 외부 타입: `InputActionAsset` 4, `CinemachineImpulseSource` 2

가장 많이 주입되는 계약 상위
| 인터페이스 | 주입처 수 |
|---|---|
| `ICurrentCharacterProvider` | 16 |
| `IPlayerStateEventSubscriber` | 13 |
| `IHealthInfo` | 6 |
| `ICharacterSwapNotifier` | 5 |

before는 인터페이스가 프로젝트 전체에 `IDamageable`·`IEvent` 2개뿐이었음.
즉 의존을 계약으로 표현할 수단 자체가 거의 없었고, 전부 구체 타입 또는 싱글톤 직접 접근이었음.

## 데이터 접근 방식의 변화
레거시는 수치 데이터 클래스까지 싱글톤이었음. 실제 접근 횟수(실측):

| 레거시 데이터 싱글톤 | `.Instance` 접근 |
|---|---|
| `PlayerHumanMaskData` | 17 |
| `CameraData` | 13 |
| `PlayerCommonData` | 11 |
| `PlayerAnimalMaskData` | 7 |
| `CheatData` | 4 |
| `PlayerGhostMaskData` | 2 |
| `UIEffectData` | 1 |
| 합계 | **55** |

문제
- 수치를 쓰는 쪽이 데이터 클래스의 구체 타입을 알아야 함. 다른 값 세트로 바꿔 끼울 수 없음
- 데이터가 `MonoBehaviour` 싱글톤이라 씬에 존재해야만 값을 읽을 수 있음

after
- 수치는 SO(`StateData`·`EffectCatalog`·`AudioCatalog`·`TextTableData` 등)로 분리
- SO는 씬 객체가 아니므로 `FindObjects`로 잡히지 않음 → `IDataProvider`/`DataContainer`를 통해서만 수집해 주입
- 값 세트 교체가 SO 교체로 끝남
