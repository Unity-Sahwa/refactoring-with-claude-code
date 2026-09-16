# 규칙 변경 기록

CLAUDE.md, `.claude/` 아래 커맨드·설정·훅이 바뀔 때마다 한 행씩 위에 쌓는다.
`왜`는 대화에서 확인된 이유만 적는다. 추측은 적지 않는다.

| 날짜 | 파일 | 무엇을 | 왜 |
|---|---|---|---|
| 2026-09-16 | .claude/commands/system-evaluation.md | 0-0-1 절차 추가(참조·의존·흐름·연결 확인은 LSP 사용, 코드 밖 참조는 Grep 병행), 2절 활용 표시의 "외부 시스템 참조" 항목에 "(LSP findReferences 기준)" 명시 | 사용자가 커맨드의 grep 기반 참조 확인을 LSP로 바꾸라고 요청함 |
| 2026-09-16 | .claude/commands/code-evaluation.md | 0절 절차 추가(참조·의존·흐름·연결 확인은 LSP 사용, 코드 밖 참조는 Grep 병행), 1-4 항목의 "호출처 검색"·"검색하면"을 "LSP로 찾은 호출처"·"LSP로 찾아지나"로 수정 | 사용자가 커맨드의 grep 기반 참조 확인을 LSP로 바꾸라고 요청함 |
| 2026-09-16 | .claude/settings.json | env에 ENABLE_LSP_TOOL=1 추가, enabledPlugins에 csharp-lsp@claude-plugins-official 추가 | 사용자가 C# LSP를 클로드 코드에 붙여 참조·정의 이동을 테스트해보자고 요청함 |
| 2026-09-15 | .claude/commands/code-evaluation.md | 판정표 아래에 system-evaluation.md와 동일한 FAIL 순번(n/총개수)·파일명·줄·문제 표 형식 추가 | 사용자가 code-evaluation에도 같은 형식을 적용해달라고 요청함 |
| 2026-09-15 | .claude/commands/system-evaluation.md | 1-4에 FAIL 하나씩 순번(n/총개수)과 파일명·줄·문제 표로 제시하는 형식 추가 | 사용자가 한 번에 하나씩 봐야 집중된다고 요청함. 파일명은 확장자 빼고, 줄 번호는 정확한 위치를 표시하라고 지시함 |
| 2026-09-14 | .claude/commands/code-evaluation.md | 1-3 첫 질문의 예외 범위에 "유니티 공식 에셋(Cinemachine 등 Unity Technologies 배포 패키지) 제공 타입" 추가 | CameraRole.cs 평가 중 CinemachineCamera 구체 타입 노출이 FAIL로 잡힘. 사용자가 유니티 공식 에셋도 예외 처리하자고 함 |
| 2026-09-14 | .claude/commands/code-evaluation.md | 1-1 첫 질문에 "게임 엔진·언어 기본 제공 타입, 유니티 공식 에셋 제공 타입 대상은 제외" 예외 추가 | CameraRole.cs:15 GetComponent<CinemachineCamera>() 호출이 FAIL로 잡힘. 1-1은 타입이 아니라 주입 방향을 보는 축이라 1-3과 같은 근거는 아니라고 AI가 이의 제기함. 사용자가 그래도 예외 처리하자고 지시함 |
| 2026-09-14 | .claude/commands/code-evaluation.md | 1-4 첫 질문에 "인스펙터에서 리스트 항목을 구분하는 라벨용 `[SerializeField] string` 필드는 제외" 예외 추가 | CameraShakeDataEntry.cs:10 `_name` 필드가 코드에서 안 읽혀 FAIL로 잡힘. 사용자가 SO 리스트 항목을 인스펙터에서 구분하는 용도라고 설명함 |
| 2026-09-14 | .claude/commands/code-evaluation.md | 1-1 둘째 질문에 "게임 엔진·언어 기본 제공 타입, 유니티 공식 에셋 제공 타입의 공개 API 접근은 제외" 예외 추가 | CameraSwitcher.cs:80,95,96 `entry.Value.Priority.Value`,`entry.Value.Target.TrackingTarget` 체이닝이 FAIL로 잡힘. 사용자가 시네머신 내부 구조는 우리가 못 건드리는 외부 API라서 그렇게 접근하게 만들어진 거라고 함 |
| 2026-09-15 | .claude/commands/code-evaluation.md | 1-3 필수 의존 방어 코드 질문의 좋음/나쁨 기준을 "null이면 메시지와 함께 예외를 던져 즉시 멈춤 / 로그만 찍고 실행이 계속되는 경우 포함"으로 구체화 | Evidence_DI.md:48은 필수 의존 없으면 LogError라 적혀있고 HitStopHandler.cs:27-31은 LogError 뒤 return으로 멈추는데, InputHub.cs:32-35는 throw로 멈춤. 셋이 형식이 달라 판정 기준이 불명확했음. 사용자가 에러 메시지를 던지고 멈추는 방식으로 통일하자고 함 |
| 2026-09-14 | .claude/commands/git-issue.md | 본문 절차의 태그 채우기 6번 단계, 초안 형식의 `태그:` 줄, 금지 목록의 Docs/Tags.md 미등록 태그 사용 항목 제거 | 사용자가 git-issue에는 태그를 달지 말자고 함 |
| 2026-09-14 | .claude/git/work_issuetemplate.md | `## 태그` 섹션 제거 | 사용자가 git-issue에는 태그를 달지 말자고 함 |
| 2026-09-14 | .claude/git/bug_issuetemplate.md | `## 태그` 섹션 제거 | 사용자가 git-issue에는 태그를 달지 말자고 함 |
| 2026-09-14 | .claude/commands/git-commit.md | 3-1 태그 판정을 "AI가 골라서 확정"에서 "후보를 뽑아 커밋안에 적고 사용자가 확인"으로 변경 | 시스템·커맨드/스킬·개념·상태 4개 범주 중 일부를 커밋마다 빠뜨림 |
| 2026-09-15 | .claude/commands/git-commit.md | 3번 이슈 매칭에서 열린 이슈를 보여주고 어느 이슈에 넣을지·닫을지 사용자에게 묻도록 변경, 닫기로 하면 6번에서 Project Status를 Done으로 바꾸고 `/git-issue`를 호출하도록 추가 | 사용자가 커밋마다 관련 이슈의 활동을 남기고, 닫을 때 이슈 본문에 그동안 한 일을 적어야 한다고 함 |
| 2026-09-15 | .claude/commands/git-issue.md | 이슈 생성 시 본문을 비워두도록 변경하고, 종료 시 `Refs #N` 커밋을 모아 본문을 채우는 6번 절차 추가 | 이슈를 만드는 시점엔 아직 한 일이 없어 내용을 쓸 수 없음. 사용자가 닫을 때 커밋 내역으로 본문을 채우자고 함 |
| 2026-09-14 | .claude/commands/git-commit.md | 5절에 `git add` 전 인덱스 상태 확인, 목록에 없는 파일은 `git restore --staged`로 내리기, 커밋을 pathspec으로 파일 못박기, 커밋 후 `git show` 파일 목록과 커밋안 대조 단계 추가. 금지 목록에 관련 항목 2개 추가 | AudioPlayer.cs 커밋 때 이미 스테이징돼 있던 무관한 DISystem 폴더 재배치 변경이 같은 커밋에 같이 들어감 |
| 2026-09-14 | .claude/commands/code-evaluation.md | 1-3 첫 질문을 "`[Inject]` 필드 타입" 한정에서 "필드·매개변수·반환 타입" 전체로 확장. 순수 데이터 클래스(필드를 그대로 반환하는 프로퍼티만 있고 로직 없음)·enum·struct는 검사 대상에서 제외 | SOLID DIP는 `[Inject]` 필드에 국한된 원칙이 아니라 모든 자리의 구체 의존에 적용됨. enum·구현체를 여러 개 만들 수 없는 순수 데이터는 추상화할 대상이 없음 |
| 2026-09-14 | .claude/commands/code-evaluation.md | 0절 절차에 "FAIL 나온 클래스는 재평가로 FAIL이 없어질 때까지 다음 클래스로 못 넘어간다" 규칙 추가 | AudioPlayer.cs FAIL을 처리 안 하고 다음 클래스로 넘어가려 함 |
| 2026-09-14 | .claude/commands/system-evaluation.md | 1-1에 enum·순수 데이터 타입(필드만 있고 동작 없는 struct/class)을 검사에서 제외하는 예외 추가 | CameraSystem 평가 중 InputActionType·GameStateType·StateEventCategory·CloseEventType처럼 같은 패턴의 FAIL이 반복됨. 사용자가 매번 좁은 인터페이스로 감싸는 대신 예외로 두자고 함 |
| 2026-09-14 | .claude/git/work_issuetemplate.md | 연관 항목 아래 `## 태그` 섹션 추가 | 커밋·이슈를 사람이 아니라 AI가 검색하기 쉽게, Docs/Tags.md 기반 해시태그를 이슈 본문에 남기자고 함. 주석은 다른 섹션과 톤을 맞춰 짧게 지시만 하게 줄임 |
| 2026-09-14 | .claude/git/bug_issuetemplate.md | 연관 항목 아래 `## 태그` 섹션 추가 | 커밋·이슈를 사람이 아니라 AI가 검색하기 쉽게, Docs/Tags.md 기반 해시태그를 이슈 본문에 남기자고 함. 주석은 다른 섹션과 톤을 맞춰 짧게 지시만 하게 줄임 |
| 2026-09-14 | .claude/commands/git-issue.md | 초안 제시 형식에 `태그:` 줄 추가, 본문 절차에 태그 채우는 6번 단계 추가, 금지 목록에 Docs/Tags.md 미등록 태그 사용 추가 | 커밋·이슈를 사람이 아니라 AI가 검색하기 쉽게, Docs/Tags.md 기반 해시태그를 이슈 본문에 남기자고 함 |
| 2026-09-14 | .claude/commands/git-commit.md | 3-1 태그 판정 단계 추가, 커밋안 형식에 `태그:` 줄과 메시지 꼬리말에 `#태그` 줄 추가, 금지 목록에 Docs/Tags.md 미등록 태그 사용 추가 | 커밋·이슈를 사람이 아니라 AI가 검색하기 쉽게, Docs/Tags.md 기반 해시태그를 커밋 메시지에 남기자고 함 |
| 2026-09-14 | .claude/commands/system-evaluation.md | 3절 SystemUsageGuide 항목 형식에 `- 확정: YYYY-MM-DD` 줄 추가. 재평가로 항목을 다시 채우면 그 날짜로 덮어씀 | 사용자가 시스템 활용 목록에 확정 날짜를 적자고 함 |
| 2026-09-14 | .claude/commands/git-issue.md | 본문 규칙을 "템플릿을 채운다"에서 파일 경로 판정→Read로 직접 열기→구조 유지→빈 섹션 삭제→주석 삭제 5단계 절차로 재작성 | DISystem 이슈 초안 작성 때 템플릿 파일을 안 읽고 임의 형식으로 본문을 씀. 사용자가 어떤 경우에 어떤 파일을 쓰는지 절차로 명시하자고 함 |
| 2026-09-14 | .claude/commands/system-evaluation.md | 0절 맨 앞에 0-0 사전조건 추가. 문서에 명시 안 된 상황·예외를 만나면 임의 판단 대신 멈추고 사용자에게 확인 | DISystem 평가 중 기능 폴더가 없는 상황을 AI가 임의로 "해당 없음"으로 처리함. 사용자가 기능 폴더뿐 아니라 규칙에 없는 예외 상황 전반에서 임의 판단을 금지하고 순서대로만 행동하라고 함 |
| 2026-09-12 | .claude/commands/code-evaluation.md | 1-1~1-5 질문을 줄 수·인자 수·체이닝 단계·grep 결과 같은 셀 수 있는 기준으로 재작성. 복붙·이름에 없는 일·더 짧게·요구만큼만 질문 삭제. 1-3 비핵심 질문을 필수/선택 의존 두 줄로 교체. 1-5에 참조만 보는 문법 목록·인터페이스 null 검사·static 중복/삭제 처리·캐싱·프레임 안 무거운 호출·문자열 이름 호출 추가 | 사용자가 평가 항목 중 수치로 못 재는 것을 골라내자고 함. "짧다·깊다·군더더기" 같은 기준은 판정이 갈리고, 다른 클래스와 비교해야 하는 질문은 이 파일만 읽는 절차에서 근거를 못 냄. `?.`·인터페이스 `== null`이 파괴된 유니티 오브젝트를 못 잡는 건 유니티 공식 글로 확인함 |
| 2026-09-12 | .claude/commands/system-evaluation.md | 1-1·1-2 두 번째 질문을 "참조한 인터페이스에서 실제 호출한 멤버의 반환타입·매개변수"로 바꿈 | CameraSwitcher 평가 때 ICurrentCharacterProvider.CurrentType(외부 enum 반환)을 호출하지 않는데도 인터페이스 전체 기준으로 보류 판정을 냄. 사용자가 클래스에서 실제로 쓰였는지가 기준이라고 함 |
| 2026-09-12 | .claude/commands/system-evaluation.md | 0절 1-2에 외부 연결·에디터 작업 여부 표시와 개수 세기 추가, 2절 판정표 아래에 활용 표시 표(외부 시스템 참조 여부·씬/SO/인스펙터 작업 여부와 내용) 추가, 0절 2번에 센 개수와 초안 나열 수를 맞추는 검사 추가. 3절 형식을 개발자용·비개발자용 모두 클래스별 나열 구조로 교체 | AudioSystem 초안 작성 중 SO 에셋 생성 경로·SerializeField 값·DataContainer 등록이 빠졌고, 기존 형식이 기능 단위라 외부 연결·에디터 작업이 필요한 클래스를 빠뜨려도 잡히지 않았음. 사용자가 평가 단계에서 개수를 세고 초안 단계에서 대조하자고 함. O/X 한 줄 표시는 나중에 다시 볼 때 무엇인지 알 수 없다고 해서 내용 칸이 있는 표로 바꿈 |
| 2026-09-11 | .claude/commands/system-evaluation.md | 1-1 축에 예외 문구 추가. DISystem의 `[Inject]`, `[Inject(true)]` 어트리뷰트 참조는 PASS로 보되, 어트리뷰트로 주입받는 필드 타입은 예외 없이 그대로 검사한다고 명시 | AudioPlayer.cs 평가 중 `[Preserve, Inject]` 표기가 DISystem의 InjectAttribute를 참조한다고 FAIL 판정됨. SystemUsageGuide.md DISystem 항목(84~108줄)에 이미 예외로 명시돼 있어 사용자가 평가 커맨드에도 반영하라고 함 |
| 2026-09-11 | .claude/commands/system-evaluation.md | 시스템 평가를 폴더 단위에서 클래스 단위로 전환. 평가 축을 1-1 시스템 간 연결, 1-2 기능 폴더 간 연결, 1-3 기능 배치 적합성 3개의 측정 가능한 PASS/FAIL 문항으로 재작성. 확장·트러블슈팅 축과 중복 배치적합성 축 삭제, 판정 용어를 좋음/나쁨에서 PASS/FAIL로 통일 | 사용자가 평가 기준이 추상적이라 판정이 갈리고 추측·생략이 가능하다고 함. 확장 축은 code-evaluation 1-3(새 종류 추가 시 기존 코드 안 고치나)과 중복, 트러블슈팅 축은 클래스 단위 품질이라 code-evaluation 담당으로 정리 |
| 2026-09-10 | CLAUDE.md | 표현 규칙 4개 추가(전문용어·함축어 금지, 조사·보조어 생략 금지, 비유 금지, 과장 수식어 금지) | 사용자가 에이전트 설명의 전문용어·함축어·비유·과장 수식어 때문에 이해가 어렵다고 함 |
| 2026-09-10 | .claude/hooks/rule-verify.ps1 | `$OutputEncoding`을 BOM 있는 UTF8에서 `New-Object System.Text.UTF8Encoding $false`(BOM 없음)로 교체 | BOM 바이트가 claude -p 파이프 입력 앞에 붙어 검증 세션이 인코딩 깨짐으로 응답, 편집이 계속 deny됐다 |
| 2026-09-07 | .claude/hooks/rule-change-log.ps1 | 규칙 파일 수정 시 이 기록을 남기라고 AI에게 지시하는 훅 신규 | 훅은 대화 맥락을 모르므로 이유는 AI가 써야 한다 |
| 2026-09-07 | .claude/settings.json | PostToolUse 훅(matcher `Edit|Write`) 등록 | 이유가 안 남으면 왜 바꿨는지 모른 채 이전 수정으로 되돌아간다 |
| 2026-09-07 | .claude/commands/git-issue.md | 제목 규칙을 `요약` 한 단어에서 /git-commit 제목 규칙 전문으로 교체 | `요약`만 적혀 있어 필드명 직역 제목(`픽셀 예산으로 제한`)이 그대로 통과했다 |
