# 포트폴리오 활동 목록 (초안)

## 이 문서를 읽는 AI에게
- 이 문서는 포트폴리오 작성 시 참고할 **사실 목록**임. 여기 적힌 것만 사용할 것.
- 적혀 있지 않은 성과·수치·역할을 추측해서 채우지 말 것. 부족하면 작성자에게 물을 것.
- 수치(용량, tris, GPU 비중 등)는 실측값이므로 임의로 반올림하거나 각색하지 말 것.
- 미해결로 적힌 항목을 해결한 것처럼 쓰지 말 것. 그대로 미해결로 다룰 것.

## 프로젝트 개요
- 제목: 사화 - (조선 : 뒤집힌 역사)
- 구현된 요소: 락온, 콤보 공격, 처형, 보스전, 캐릭터 스왑, 세이브 슬롯, 다국어, 컷씬
- Unity 6 (6000.3.11f1) / URP
- 대상 플랫폼: Android (Google Play 업로드), PC 입력도 지원
- 기간: 2026-04-07 ~ 2026-09-07 (5개월, 커밋 기준)
- 인원: 1인 개발
- 담당 범위: 외부에서 가져온 에셋(모델·애니메이션·이펙트 등)을 제외한 **코드·시스템 설계·엔진 설정·최적화·툴·문서 전부**
- 개발 방식: Claude Code를 사용해 코드를 작성하고, 본인은 설계 결정·검수·흐름 파악을 담당
- 진행 순서: 임시저장 커밋 상태로 출시를 먼저 하고, 코드 정리·리팩터링은 출시 이후에 진행

---

`[확인필요]` = 사실·수치 미확인, `[대화필요]` = 내용 더 채워야 함. (현재 남은 항목 없음)

## [설계 — 의존성 주입]
- `[Inject]` 어트리뷰트 기반 DI(`AttributeInjector`)를 직접 구현, 씬 Awake 시점에 인터페이스 구현체를 자동 배선
- 초기 DIContainer(수동 등록) 방식에서 어트리뷰트 스캔 방식으로 전면 교체 (이슈 #40)
- 싱글톤 Instance 패턴을 걷어내고 주입으로 대체
- 선택적 주입 옵션으로 없어도 되는 의존을 구분
- 같은 타입 구현체가 여럿일 때 첫 번째만 주입되고 경고만 남는 한계를 스스로 진단해 기록
- ScriptableObject 수치 에셋을 공급하는 `DataContainer`/`IDataProvider` 구현
- SO를 무차별 수집하던 방식을 그룹 단위 수집으로 개선
- `DontDestroyOnLoad` 의존을 제거하고 주입 기반으로 전환(SettingsHolder)

## [설계 — 플레이어 상태 시스템]
- 상태 클래스 상속 구조를 폐기하고 `StateRunner` 단일 클래스 + 데이터 조합으로 재설계
- `StateData`(ScriptableObject)로 상태별 수치·애니메이션·이펙트·오디오 설정을 코드 밖으로 분리
- `PlayerStateType`, `StateTriggerType`, `StateEventCategory` enum으로 상태·트리거·이벤트 분류
- `EventChannel` 기반 상태 이벤트 발행 구조 — 상태가 외부 시스템을 직접 알지 않음
- `AnimationTracker`로 애니메이션 진행률·종료 판정을 상태 로직에서 분리
- `IMotionControl`로 진행률 구간 계약 정의
- `PlayerStateInputGate` — 상태에 따라 입력을 막는 게이트 분리
- `IntervalDataEntry`로 구간별 동작(이동·판정 타이밍)을 데이터로 지정
- 버그 수정: 캐릭터 전환 중 콤보 고장, 대쉬 중 공격 시 대쉬 이동이 스킬에 그대로 적용되던 문제

## [설계 — 플레이어 이동]
- 지형을 뚫는 문제를 계기로 이동 시스템 전면 재설계(이슈 #45), CharacterController로 교체
- 이동 원인을 `IVelocitySource`로 추상화 — 걷기 / 중력 / 스킬 이동을 각각 독립 구현
- 원인이 늘어도 `PlayerCharacterMover`는 변경되지 않는 구조
- `MoveParams` 읽기 전용 struct로 이동 계산 입력을 값 전달
- 회전을 `CharacterRotator`로 분리, 락온 대상에 회전 고정 기능 구현
- `GroundProbe`로 접지 판정을 외부에서 조회 가능하게 분리
- 지속 중력과 경사면 미끄러짐 처리
- 스킬 물리 이동(`ISkillMove`, `SkillMoveDataEntry`)을 데이터 기반으로 구현
- `IMoveDirectionProvider`로 이동 방향을 값으로 넘겨 발소리 처리기의 직접 참조 제거

## [설계 — 전투 / 히트박스]
- 히트박스 프리팹 방식을 걷어내고 Overlap 판정만으로 재구현
- 히트박스를 플레이어 상태 데이터에 묶어 상태별로 켜지도록 구성
- `DamageInfo` / `IDamageable`로 공격자·피격자가 서로의 구체 타입을 모르게 분리
- 히트스탑 처리 구현
- 피격 반응 처리기 3종 분리: `HitEffectHandler`(타격 이펙트), `HitFlashHandler`(점멸), `HitStopHandler`
- 피격 시 Vignette 화면 효과 처리
- `InkColorType` 기반 덧칠 시스템 연동
- 코루틴 `WaitForSeconds` 객체 재활용으로 할당 절감

## [설계 — 카메라]
- 기본 / 락온 카메라 전환 시스템 구현(`CameraSwitcher`, `CameraRole`, `CameraKind`)
- 락온 대상 감지 및 락온 상태 이벤트 발행(`LockOnTargetDetector`, `ILockOnState`, `ILockOnTarget`)
- 처형 대상 감지 구현
- 카메라 흔들림·줌을 데이터(`ShakeData`, `CameraShakeDataEntry`, `CameraZoomDataEntry`)로 분리
- `SeeThroughWall` — 벽에 가릴 때 시야 처리
- `IPointerLookControl` 인터페이스로 `PlatformController`의 Cinemachine 직접 의존 제거
- `CameraDebugHud` — 카메라 상태 시각 디버깅 도구
- 캐릭터 스왑 시 카메라 추적 대상도 함께 전환

## [설계 — 입력]
- 폴링 기반 입력을 이벤트 기반으로 리팩터링
- 게임 상태(게임플레이 / 컷씬 / 메뉴)별로 입력 핸들러를 분리하고 `InputHub`가 분배
- `IDomainInputHandler`로 도메인별 처리 계약 통일
- 제공자 인터페이스 분리: `IInputMoveProvider`, `IInputPressedProvider`, `IMenuInputProvider`, `ICutsceneInputProvider`
- 모바일 입력 버튼과 PC 입력을 같은 경로로 흡수
- 키 리바인딩 설정(`InputKeySettings`)과 저장 연동
- 시스템 평가 후 구독 해제 불가 구조와 우회 전달 경로를 정리(이슈 #99)

## [설계 — 오디오]
- 재생 요청/응답 통로(`AudioChannel`)를 두고 요청자와 재생자를 분리
- `AudioCatalog` + `AudioCatalogEntry`로 클립·수치를 데이터화, 인스펙터에 ID가 이름으로 보이도록 Drawer 작성
- `AudioPlayRequest`로 재생 요청 파라미터를 값 전달
- 오디오 소스 풀링, 초기 개수 8개로 확정(const)
- 반납 시점에 오디오 소스가 파괴된 경우를 대비한 방어 코드 추가
- 통합 볼륨 제어기(`VolumeController`) 구현
- Unity 믹서 파라미터 이름과 `VolumeCategory` enum을 일치시켜 중간 변환 코드·변수 제거
- 애니메이션 이벤트 기반 발소리 요청 구현
- `SoundSettings`의 볼륨을 고정 필드에서 이름-값 리스트로 교체해 카테고리 추가 시 코드 수정이 필요 없게 함

## [설계 — UI]
- 창(Window) 시스템 구현: `UIRoot`, `UIWindow`, `IWindow`, `WindowType`, `UINavigator`
- 확인 창 파생 구조(`ConfirmWindow` → 새 게임 / 불러오기 / 종료)
- 세이브 슬롯 목록 UI(`SlotListWindow`, `SaveSlotButtonView`, `LoadSlotButton`, `ISaveSlots`)
- 설정 창: 볼륨 슬라이더, 마우스 감도 슬라이더, 키 리바인딩 버튼, 언어 버튼
- HUD: 체력 HUD, 락온 마커, 스크롤 크레딧
- UI 이펙트 처리 시스템 — 효과 종류를 SO에서 드롭다운으로 선택하고 채널로 재생/정지 요청(`UIFader`, `UIShake`)
- 버튼 클릭 사운드 처리(`ButtonClickSound`, `NoClickSound`)

## [설계 — 저장]
- 슬롯 기반 세이브/로드 구현(`SaveManager`, `SaveSlotManager`, `SlotLoadRunner`)
- 저장 경로·파일 처리를 `ISaveFileHandler`/`LocalSaveFileHandler`로 분리해 교체 가능하게 함
- `ISaveData` 계약으로 저장 대상(입력 키, 사운드, 오브젝트 활성 상태, 위치)을 각각 분리
- `SerializableVector3` — Unity 타입 직렬화 대응
- 씬 인덱스가 세이브에 저장되므로 씬을 중간에 끼워넣으면 안 된다는 제약을 문서화
- 씬 전환 시 값 유지를 static으로 처리하고, 도메인 리로드 off 환경 대응으로 초기화 코드를 각 지점에 배치

## [설계 — 다국어 / 설정 / 플랫폼]
- 텍스트 테이블(`TextTableData`) + 키 기반 지역화(`LocalizedText`)
- `TextKeyAttribute` + Drawer로 인스펙터에서 텍스트 키를 드롭다운 선택
- CSV 임포트로 번역 데이터 일괄 갱신(`TextTableCsv`)
- Unity Localization + Addressable 패키지를 도입했다가 필요 없다고 판단해 제거하고 자체 구현으로 전환
- 게임 상태 관리(`GameStateManager`, `GameStateType`) — 게임 / 컷씬 / 메뉴 구분, 일시정지, 커서 제어
- PC / 모바일 판별과 플랫폼별 오브젝트 토글(`PlatformController`, `PlatformObject`)

## [설계 — 구조 정리]
- 파일 44개를 코드 변경 0줄로 책임별 폴더에 재배치, 리뷰 가능한 커밋 유지
- enum을 `~Type` 접미어로 통일하고 파일 단위로 분리(`WindowType`, `ButtonActionType`, `LanguageType`, `InkColorType` 등)
- 직렬화된 enum이 항목 추가 시 밀리는 문제를 값 명시·번호대 분리 규칙으로 정리해 가이드 작성
- 리팩터링 완료 후 레거시 Old 폴더 4개와 참고용 코드를 일괄 삭제
- 사용처가 사라진 인터페이스·컴포넌트를 추적해 제거

## [최적화 — 계측 환경 구축]
- Unity Profiler가 대부분의 모바일 기기에서 GPU 타이밍을 지원하지 않는 한계를 확인
- Arm Performance Studio(Streamline)를 붙여 Mali GPU 하드웨어 카운터를 실측
- 비루팅 기기에서는 Development Build로만 캡처 가능하다는 조건까지 확인해 기록
- Frame Debugger(드로우콜 구조), URP Rendering Debugger(오버드로우), Unity Profiler(CPU)를 역할별로 나눠 사용
- 씬1~5의 Fragment 사용률·오버드로우·코어 사용률을 표로 비교해 병목이 픽셀 처리 단계임을 특정

## [최적화 — 보스 이펙트 셰이더]
- 불기둥 이펙트 9개 동시 생성 시 10FPS대로 하락하는 구간을 프레임 단위로 분석
- GPU 시간의 74.9%(19.2ms)가 실제 이펙트가 아닌 `Setup Camera`에서 발생함을 발견
- 프리팹 → 머티리얼 → 셰이더그래프까지 역추적해 Scene Color 노드(화면 전체 복사)를 원인으로 특정
- `_Distortionpower`가 0이라 왜곡 효과가 시각적으로 전혀 안 쓰이는데 복사 비용만 지불 중임을 확인
- 그래프의 Branch가 HDRP 판별용이라 URP에서는 항상 Scene Color 경로로 귀결되는 죽은 분기임을 분석
- 대응안 3가지(머티리얼 교체 / 그래프 수정 / 생성 개수 제한)를 비교하고 영향 범위가 3개 머티리얼뿐임을 근거로 그래프 직접 수정 선택
- 결과: 시각적 변화 없이 프레임의 74.9%를 차지하던 화면 복사 제거

## [최적화 — 조명 전환 hitch]
- 조명 intensity가 0이 되는 순간 748ms 프레임이 발생하는 것을 Profiler로 포착
- 시간의 82%가 셰이더 GPU 프로그램 생성 + 드라이버 컴파일 대기임을 확인
- Frame Debugger로 조명 on/off 상태의 Terrain 셰이더 키워드를 비교해 그림자 키워드 조합 변경이 원인임을 규명
- Shader Variant Collection 등록을 시도했으나 Terrain 셰이더가 트래킹에 잡히지 않는 Unity 한계를 확인 — 미해결로 기록
- 대안(더미 렌더 워밍업 / 스크립트로 variant 직접 추가)까지 조사해 남김

## [최적화 — 그림자]
- 나무 그림자가 캐스케이드 수만큼 원본 메시를 다시 그리는 비용 구조 파악
- 저폴리 프록시 메시 시도 → 잎 카드가 Decimate로 찢어져 실패, 원인까지 기록
- 알파 클립 Quad로 그림자 대체, URP Unlit에 ShadowCaster 패스가 없어 그림자가 안 나오는 원인을 찾아 Lit으로 교체
- Quad 단면 Cull 방향에 따라 그림자가 사라지는 문제 확인
- 실측 비교(나무 1그루): tris 4.4k → 2.6k, ShadowCaster 4 → 3
- Quad가 오브젝트 수만큼 드로우콜을 먹는 역효과를 발견해 메시 병합 도구로 대응
- URP Asset의 Shadow Max Distance·Cascade Count 등 설정값을 근거와 함께 조정

## [최적화 — 빌드 용량]
- Play Console 500MB 초과(Total 550.73MB / Download 522MB) 문제 대응
- `com.unity.build-report-inspector`를 도입해 추측 대신 용량 분포를 계측, 상위가 전부 텍스처임을 확인
- Texture Compression 기본값이 개별 텍스처 `Automatic`일 때만 적용된다는 원인 특정
- 포맷을 ETC2 / ASTC 4x4 / 6x6 / 8x8로 비교해 비트레이트·품질 근거로 ASTC 6x6 채택
- 기존 Max Size보다 키우지 않는 조건으로 일괄 적용 도구 작성
- 결과: Total 550.73MB → 389.69MB, Download 522.18MB → 361.15MB (-161MB)
- 압축 후 터레인 번들거림 발생 → 가설 4개를 하나씩 기각하고 Smoothness Source(Diffuse Alpha)가 원인임을 확정

## [최적화 — 드로우콜 / 배칭]
- 프레임당 190 draw call을 Frame Debugger로 분해해 터레인 타일 6개 + 레이어 5개(Add Pass) + 풀 디테일이 대부분임을 확인
- 값이 동일한 복제 머티리얼이 SRP Batcher 배칭을 깨뜨리는 문제를 shader + 전 프로퍼티값 시그니처 비교로 검출
- 같은 머티리얼 오브젝트를 메시 하나로 병합해 드로우콜·셰도우캐스터를 1개로 고정
- 로우폴리 오브젝트로 교체

## [최적화 — 풀링 / 프리로딩]
- 보스 스킬 프리팹의 생성·파괴 반복 구조를 오브젝트 풀링으로 교체
- 새 구조를 만들지 않고 기존 `IPreloadTargetProvider`/`EventPreloadRender` 프리로드 구조에 편입
- 인스펙터 프리팹 슬롯 34개가 실제로는 6종 중복 배선이었음을 밝혀 정리
- 플레이어 이펙트 풀 + 프리로드 구현

## [툴 개발]
- `MeshCombinerTool` — 같은 머티리얼 오브젝트를 메시 하나로 병합(빌트인 메시 저장 경로 이슈까지 대응)
- `MaterialDuplicateFinder` — 값이 동일한 복제 머티리얼을 그룹으로 검출
- `TextureCompressTool` — Android 텍스처 압축·Max Size 일괄 적용
- `SceneUsageFinder` — 씬 전체에서 대상 사용처 추적
- `SceneComponentBatchEditor` — 씬 내 컴포넌트 값 일괄 수정
- `RemoveMissingScriptsTool` — Missing 스크립트 일괄 제거
- `UIOrderViewer` — 씬의 Canvas 렌더 우선순위 확인
- `StateDataEditor` — 상태 데이터 편집 UI
- PropertyDrawer 5종(오디오 카탈로그, 카메라 흔들림, 텍스트 키, 텍스트 테이블, 오브젝트 토글)으로 인스펙터 편집성 개선
- `TextTableCsv` — 다국어 텍스트 CSV 임포트
- 시스템 관계도 HTML 자동 생성 스크립트, 선 겹침·관통 자동 검증 스크립트

## [설계 평가 · 문서화]
- 시스템 12개를 클래스 단위로 평가해 문제를 목록화(교체 불가, 조용한 실패, 책임 과다 등을 축으로)
- 히트박스 평가에서 "적 10마리 동시 사용 시 Provider가 키당 인스턴스 1개만 만들어 판정이 엉킨다"는 구조적 결함을 확장 전에 발견
- Overlap 결과 버퍼가 32개로 고정돼 그 이상은 조용히 무시되는 문제 등 "터지지 않지만 틀린" 동작을 식별
- 자기 구조의 한계를 파일·줄번호 근거로 명시한 회고 작성(런타임 스폰 객체 주입 불가, 씬 넘김 static 값 5곳 분산)과 "다음에 할 것" 정리
- 코드 작성자용 시스템 사용 규칙 문서 작성
- 비개발자용 가이드 별도 작성 — 씬 필수 프리팹 11종, 데이터 등록 절차 등 대상 독자를 나눠 씀
- 시스템 관계도·클래스 다이어그램·플로우 문서화
- URP 설정 항목(Opaque Texture, Terrain Holes, HDR, MSAA, 그림자 등)을 의미·문제·적용 기준으로 정리
- 최적화 기록을 문제 → 원인 분석 → 개선 → 결과 형식으로 통일해 남김

## [AI 협업 — 코드 검수]
- 코드 작성을 AI에 맡기고 사람은 검수·이해·흐름 파악을 담당하는 방식으로 전환
- 잡아낸 것: 이미 설계해 둔 시스템을 쓰지 않고 `SerializeField`·`public`으로 직접 참조하려는 코드
- 잡아낸 것: 매 프레임 도는 `Update`에 비용 큰 작업을 넣거나, 줄일 수 있는데 그냥 `Update`를 쓰는 코드
- 잡아낸 것: 씬 전체를 훑는 `Find` 계열 사용
- AI는 웹에서 보편적으로 쓰이는 방식을 따르므로, 이 프로젝트에서 틀린 부분은 문제를 겪어가며 규칙으로 주입해야 한다는 결론
- AI가 반복하던 문제: 대화가 끝난 뒤 허락 없이 파일을 수정
- AI가 반복하던 문제: 사용자인 척 스스로에게 질문을 만들고 거기에 답함
- AI가 반복하던 문제: 이미 작성해 둔 규칙을 어김
- 롤백까지 간 사례는 없음. 누락·맥락 오해로 인한 실수는 에러 코드를 넘겨주면 스스로 해결함

## [AI 협업 — 얻은 결론]
- 권한 제어를 빡빡하게 걸어야 함. 코드는 변경이 눈에 띄지만 설정 변경·내부 변경은 조용히 지나갈 수 있음
- 사람이 두루뭉술하게 묻기 때문에 AI 답이 마음에 안 드는 경우가 생김
- 본인 기준으로 완벽하게 질문해도 엉뚱한 답이 나올 수 있음 — 사람을 대하듯 다뤄야 한다는 판단

## [AI 협업 — 규칙 설계와 비용]
- 코드 컨벤션, 주석 컨벤션, 시스템 평가, 코드 평가, 커밋, 이슈, 근거 조사 규칙을 커맨드/스킬로 문서화해 적용
- 규칙을 한 번에 확정하지 못하고 여러 차례 수정·간소화하며 다듬음(커밋 이력에 그대로 남아 있음)
- 코드 설계 에이전트를 만들었다가 재구현하고 결국 커맨드 방식으로 정리
- 초기 학습 비용(도구 습득 · 시행착오 · 규칙 반복 수정)이 커서 진행이 느렸음
- git-flow·PR 규칙 문서를 만들었다가 실효가 없어 삭제
- 규칙은 처음부터 전부 설계하려 하면 끝이 안 남 — 모르는 것을 모르기 때문에 그 자체가 시간 낭비라는 결론
- 대신 만들다 막히는 지점이 나오고 기존 규칙과 결이 다르다고 판단되면, 그때 새 항목이 생겼다고 보고 규칙을 쌓아가는 방식을 택함

## [AI 협업 — 커밋 메시지 규칙 설계]
- 지금 커밋 메시지는 생략이 많아, 6개월 뒤의 본인이나 다른 사람이 읽고 이해하거나 눌러볼 만하지 않다고 판단
- 커밋 종류(버그 / 리팩터링 / 기능 구현 / 문서)마다 필요한 서술이 다르고, 그 안에 다시 하위 분류가 존재함
- 따라서 제목·본문 규칙만 정해서는 부족하고, 분류의 분류까지 규칙이 필요하다는 결론에 도달
- 다만 그 분류 체계를 처음부터 전부 만드는 시도는 해봤고 끝이 보이지 않아 포기 — 막히는 지점에서 하나씩 늘리는 방식으로 전환

## [AI 협업 — 작업 단위 쪼개기]
- 한꺼번에 작업시키고 마지막에 커밋을 맡기는 방식은 위험하다고 판단
- 작업 단위를 작게 하면 AI가 변경의 의도를 정확히 파악해 커밋 메시지 품질이 올라감
- 단위가 커지면 납득할 결과가 나올 때까지 규칙을 세세하게 만들어야 함
- 단위를 작게 하면 규칙 수정 빈도가 줄고 본인도 메시지를 이해하기 쉬워짐
- 앞으로는 파일 내부 변경까지 더 쪼개는 방향으로 갈 생각

## [AI 협업 — 토큰 비용과 자동화 포기]
- Pro 요금제 기준으로 코드 작성 → 평가 → 요약까지 전부 AI에 맡기려 했으나 토큰이 곧바로 바닥남
- 클래스 하나가 아니라 여러 클래스를 만들어 기능을 돌게 해야 하므로 생성·검사·평가에 드는 시간과 토큰이 큼
- 그렇게 뽑은 결과가 만족스러운 경우는 드물었고, 만족할 때까지 시행착오를 반복하는 데 본인의 시간과 피로도까지 들어감
- 판단: 자동화 포기, 부분 자동화도 하지 않음. 만족스러운 결과물을 하나씩 내는 방향으로 진행
- 외부의 좋은 방법을 가져와도 본인이 이해하고 납득할 결과물이 될지 장담할 수 없다고 보고, 직접 삽질하며 접근하기로 함

## [협업 프로세스]
- 총 426커밋, GitHub Issue 99개를 시스템 라벨과 함께 운영
- 이슈 템플릿(bug / feature) 작성
- 기능 단위 feature 브랜치와 PR 병합 사용(#29, #32, #38, #42 등)
- 후반부 커밋 규칙 확립: `[시스템] 무엇을 어떻게`, 한 커밋 = 한 변경 단위
- 초·중반 "임시저장" · "기타" 커밋이 다수 남아 있는 점은 미해결
- 그 이유: AI로 시스템 흐름 전체를 한 번에 수정해보려다 커밋 시점을 계속 미룸
- 그 이유: 커밋 메시지 작성에 시간이 오래 걸림
- 그 이유: 기획자에게 출시 빌드 모습을 빨리 보여주려고 커밋 정리를 뒤로 미룸
- 그 이유: 나중에 주석·비효율 부분을 다시 손볼 때 같이 커밋하면 된다고 생각함
- 실제 진행: 임시저장 커밋 상태로 출시를 먼저 하고, 코드 정리·리팩터링은 출시 이후에 진행하는 순서를 택함
- 그 결과 출시 직전 구간은 이슈·커밋 관리보다 빌드 산출이 우선순위가 됨

## [제외]
- 셰이더 Variant WarmUp(미사용), Gimmick·Enemy 코드

---

# [리팩토링 근거 — 플레이어 시스템 (레거시 대비 실측)]

비교 대상
- before: `Unitysahwa-refactoring-with-claudecode(before)/Assets/Scripts/Player` (36파일)
- after: `Unitysahwa-refactoring/Assets/1.Code/Scripts/PlayerSystem` (81파일)
- 범위: 플레이어 한정. 다른 시스템은 미조사.

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| 총 줄 수 | 8,854 | 4,387 |
| 최대 단일 파일 | `AnimalMaskSkill` 1,154줄 | `PlayerPartner` 212줄 |
| 싱글톤 `.Instance` 호출 | 47 | 0 (남은 4건은 풀링 객체 참조 `active.Instance`, 싱글톤 아님) |
| `GetComponent` 계열 | 59 | 10 |
| `interface` 정의 | 0 | 26 |
| `StartCoroutine` | 87 | 4 |
| 캐릭터 타입을 직접 아는 파일 | 25 (분기 150건 이상) | 4 (각 1건) |

주의: after는 스킬 수치가 SO로 빠져 코드 줄에 잡히지 않음. "코드량만의 비교"임을 명시할 것.
주의: float 리터럴 총량은 118 → 94로 대비가 약함. 지표로 쓰지 말 것.

## 중복 실측
- `CoFirstAttack`: Animal·Human 양쪽 모두 **111줄로 길이 동일**, 약 70% 동일
- `CoDash`: 128줄 / 121줄, 약 84% 동일
- 동일 이름·동일 역할 메서드 12개가 두 클래스에 중복

## 기능별 구조 대비

### 콤보 공격
| 축 | before `CoFirstAttack` | after `StateRunner` |
|---|---|---|
| 규모 | 111줄 × 6벌 (3콤보 × 2캐릭터) | 100줄 1개가 상태 16종 처리 |
| 직접 참조 | 컴포넌트 9개 | 인터페이스 1개 (`IPlayerStateEventRaiser`) |
| 1회 실행 보장 | `activeXxxOnce` 불리언 6개 수동 | 정렬 이벤트 목록 + 커서 `_readIndex` 1개 |
| 타이밍 기준 | 절대 초 `Time.time + waitTime` | 애니 진행률 0~1 |
| 중단 처리 | 애니 해시 분기 + `yield break` 산재 | `Exit()` → `RaiseReset()` 1곳 |

핵심: 줄 수가 아니라 책임 위치. before는 "언제·무엇을·어떻게"를 한 메서드가 전부 앎.
after는 언제=진행률(`StateRunner`), 무엇=`StateData` SO, 어떻게=구독자로 분리.

### 이동
| 축 | before | after |
|---|---|---|
| 이동 원인 실행 | 각 스킬 코루틴이 `playerSkillMove.StartCoroutine(SkillMove(...))` 직접 호출 | `IVelocitySource.Evaluate`가 속도만 반환 (Walk/Skill/Gravity 3종) |
| 합산 지점 | 없음 (원인끼리 서로 모름) | `PlayerCharacterMover` 1곳 |
| 방향 확장 | bool 4개 + Vector3 4개 필드 세트 추가 | 구현 클래스 1개 추가, Mover 무변경 |
| 의존 방향 | `PlayerSkillMove`가 `HumanMaskSkill`을 `SerializeField` 역참조 | Mover → 인터페이스만 |

### 히트박스
| 축 | before | after |
|---|---|---|
| 히트박스 실체 | 씬에 배치한 GameObject 배열, 수동 배선 | 없음. `Physics.Overlap*NonAlloc`으로 그 순간만 판정 |
| 대상 선택 | `SelectHitBox`의 하드코딩 switch 6 case | `HitboxDataEntry`가 위치·회전·형태를 SO에 기술 |
| 판정 코드 | `OnCollisionEnter` 단일 메서드 244줄 (41~284행, 사이에 다른 메서드 없음 확인) | `TryHit` 39줄 + `IDamageable` 계약 |
| 직접 참조 | `PlayerHitBoxCollider`가 컴포넌트 7개 `SerializeField` | 주입 3개, 전부 인터페이스/채널 |
| 중복 타격 방지 | 없음 (`HashSet`/`List`/`Contains`/카운트 0건 확인. 파일에 "타격 횟수 추가해야함" 미구현 주석 존재) | `HashSet<IDamageable> AlreadyHit` |

콤보 1타 추가 비용: before는 씬 오브젝트 + 배선 + switch case + enum(3파일 이상) / after는 SO 배열 항목 1개.

### 피격 반응
- before: `OnCollisionEnter` 244줄이 `switch (playerCurrentSubState)`로 공격 종류를 분기하고,
  각 case마다 이펙트·타임스케일·카메라쉐이크·사운드 4줄을 통째로 복제. 확인된 것만 Human 4종 + Animal 4종 = 8벌
- before: 값이 `humanData.firstNormalAttackHitEffect` 식으로 공격별 필드 4개씩 존재
  (`PlayerHumanMaskData` 102필드 / `PlayerAnimalMaskData` 103필드 / `PlayerGhostMaskData` 42 / `PlayerCommonData` 47의 정체)
- after: 히트박스 데이터가 `CombatInfo`를 실어 보내고 `HitEffectHandler`·`HitStopHandler`·`HitFlashHandler`·`HitVignetteEffect`가 각자 구독. 분기 0, 복제 0
- 스킬 1개 추가 비용: before = enum + Data 필드 4개 + case 1개 + 반응 4줄 복제 / after = SO 항목 1개

### 캐릭터 스왑
| 축 | before `MaskChange` (269줄) | after `PlayerCharacterSwitcher` (74줄) |
|---|---|---|
| 참조 컴포넌트 | `SerializeField` 9개 + `CameraController.instance` 직접 호출 | 주입 1개 (`List<PlayerCharacter>`) |
| 캐릭터별 필드 | Human/Animal 각각 GameObject·Animator·Rigidbody 3세트 9필드 + Current 3필드 | 없음. `PlayerCharacter.GetCharacterComponent<T>()`로 조회 |
| 전환 시 하는 일 | 위치·회전 복사, current 3개 갱신, `humanSkill.InitializeSkill()` 직접 호출, `SetActive`, 카메라 락온 조회해 `isFocused` 세팅, `skillHUD.ChangeIcon`, 마스크 오브젝트 4개 토글 | 위치·회전 복사, `SetActive`, `OnCharacterSwapped` 발행 |
| 전환 후속 처리 | 스왑 클래스가 직접 호출 | 구독자가 각자 처리. 구독처 23곳 |
| 결합 방향 | 다른 21개 파일이 `MaskChange.instance` / `maskChange.` 로 역참조 | 인터페이스 3종만 노출 (`ICharacterSwappable`, `ICharacterSwapNotifier`, `ICurrentCharacterProvider`) |

핵심: before는 스왑 클래스가 후속 처리 전부를 알아야 함. after는 "바꿨다"만 알림.

### 이펙트 수명
- before: `Instantiate` 0건. 씬에 미리 배치한 `GameObject[]` 슬롯 16개를 인스펙터에 수동 배선하고 코루틴이 `SetActive` 토글. 풀 고갈 개념 없음
- before `Destroy` 6건은 전부 `else if (instance != this) Destroy(gameObject)` — 싱글톤 중복 제거용. Player 폴더에만 싱글톤 클래스 6개
- after: `EffectCatalog`(SO)에 id·프리팹·풀 크기 등록 → `PlayerEffectProvider`가 풀 자동 생성, `Rent`/`Return`. 프리로드 대상도 자동 제공. 풀 고갈 시 경고
- 차이의 본질: 양쪽 다 미리 만듦. **관리 주체가 사람(인스펙터) → 코드(카탈로그)로 이동**

## 서술 시 주의 (감사 지적 사항)
- `PlayerCharacterSwitcher.SwapPlayerCharacter`는 "타입이 다른 첫 번째"를 고르는 2종 전제 구현.
  기획상 캐릭터 2종이 확정 사항이므로 결함이 아니라 범위 결정. "코드 0줄"이 아니라 "확정된 2종 범위 안에서 데이터만으로 대응"으로 쓸 것
- 초안에 적힌 "상태 클래스 상속 구조를 폐기"는 레거시 실물과 다름. 실제 before는 상속이 아니라
  **캐릭터별 거대 스킬 클래스 복제**(`AnimalMaskSkill`/`HumanMaskSkill`/`GhostMaskSkill`). 서술 수정 필요
- 초안의 "대쉬 중 공격 시 대쉬 이동이 스킬에 적용되던 버그"는 리팩토링 이후 코드에서 난 것.
  레거시 비교 근거로 연결하지 말 것
- `_Refactoring/Prototype/Player/States`(BaseState + 파생 8개)는 테스트용이며 before가 아님

---

# [미평가 — 평가 후 삭제할 것]

## 1. 기능 동등성 증명
구조를 바꿨는데 같은 게임이 돌아간다는 증거가 필요함. 없으면 "코드를 줄인 게 아니라 기능이 빠진 것"으로 읽힘.

확인된 사실 (본인 답변)
- 레거시 프로젝트는 실행 가능한 상태로 남아 있음
- 기능은 동일. 차이는 연출과 공격 사이 히트박스 몇 부분이 추가된 정도
- `CheatMode`: 에디터 전용이라 제외
- `TimelineHelper`: 결합 덩어리라 전부 분리함
- `PlayerWaypoints`: 기믹 시스템으로 대체 (SavePoint 계열 클래스로 추정 — 확인 필요)

해야 할 것
- [ ] 기능 대조표 작성 — 레거시 클래스별 기능 ↔ 현재 대응 클래스 매핑. 옮겨간 자리를 보여 누락 없음을 증명
- [ ] 녹화 3종 확보: 에디터 플레이 / APK 실기기 / AAB(Play 배포본) 실기기. 각각 같은 구간
- [ ] 레거시 쪽도 동일 구간 녹화. 캡션에 측정 조건(에디터인지 실기기인지) 명시
- [ ] 추가된 히트박스·연출 부분은 "동등"이 아니라 "추가"로 따로 표기

## 2. 왜 그 설계였나
결과만 있고 선택 근거가 없음. 면접에서 "왜 상속이 아니라 데이터 조합인가"를 반드시 물음.
- 필요한 것: 검토했던 대안과 기각 사유 (상속 기반 State / 기존 구조 부분 개선 / 외부 FSM 에셋 등)
- 필요한 것: 진행률 기반 이벤트를 택한 이유 — 절대 초 방식의 어떤 실패를 겪었는지
- 미확인: 이 결정들이 기록으로 남아 있는지 (이슈·커밋·문서)

## 3. 성능 영향 — 비교 불가로 확정 (측정하지 않음)
레거시는 Unity 2022 LTS, 현재는 Unity 6. 메이저 2단계 차이라 URP 개편·GC·컴파일러가 모두 다름.
어떤 수치가 나와도 코드 기여분과 엔진 기여분을 분리할 수 없음. 실기기 측정은 발열·백그라운드 편차까지 더해짐.
after 단독 수치는 비교 대상이 없어 리팩토링 성과와 연결되지 않으므로 쓰지 않음.

"구조를 늘렸는데 느려지지 않았냐"는 질문에는 아래 정적 사실로 답함.
- before는 `StartCoroutine` 87건. 각 스킬 코루틴이 `while` + `yield return null`로 매 프레임 돌며
  `activeXxxOnce` 플래그 6개를 계속 검사함. `Update` 수(3개)가 적은 게 프레임당 작업이 적다는 뜻이 아님
- after는 `StartCoroutine` 4건. `StateRunner`가 정렬된 이벤트 목록을 커서 `_readIndex`로 진행률 도달분만 발행
- 정적 지표 대비(참고): `Update`/`FixedUpdate`/`LateUpdate` 3/1/0 → 7/1/1, `foreach` 3 → 16, `new Dictionary` 0 → 4

남은 개선 여지 (스스로 확인한 것)
- `StateRunner.Enter`의 `StateKey.ToString()` 2건이 상태 진입마다 문자열 할당. 해시 캐시로 제거 가능

## 4. 그 외 미확인
- 리팩토링에 든 기간·커밋 수를 플레이어 시스템 한정으로 분리 가능한지
- 리팩토링으로 새로 생긴 문제(회귀 버그)가 있었는지, 있었다면 어떻게 잡았는지
- 플레이어 외 시스템(카메라·입력·오디오·UI·저장) 레거시 대비 미조사
