# 규칙 변경 기록

CLAUDE.md, `.claude/` 아래 커맨드·설정·훅이 바뀔 때마다 한 행씩 위에 쌓는다.
`왜`는 대화에서 확인된 이유만 적는다. 추측은 적지 않는다.

| 날짜 | 파일 | 무엇을 | 왜 |
|---|---|---|---|
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
