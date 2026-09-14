# 태그 목록

커밋·이슈에 `#태그` 형식으로 붙인다. AI가 검색·필터링하기 위한 것이라 사람이 보기 좋은 문장이 아니라 짧은 단어로 쓴다.
여기 없는 태그를 새로 쓰려면 먼저 이 표에 한 행 추가한 뒤 커밋·이슈에 쓴다. 같은 뜻의 태그를 두 번 안 만든다(추가 전에 이 표에서 검색).

## 시스템 (`Assets/1.Code/Scripts/` 폴더명 소문자)
| 태그 | 대상 |
|---|---|
| #disystem | DISystem |
| #audiosystem | AudioSystem |
| #camerasystem | CameraSystem |
| #combatsystem | CombatSystem |
| #enemysystem | EnemySystem |
| #gamestatesystem | GameStateSystem |
| #gimmicksystem | GimmickSystem |
| #inputsystem | InputSystem |
| #languagesystem | LanguageSystem |
| #platformsystem | PlatformSystem |
| #playersystem | PlayerSystem |
| #savesystem | SaveSystem |
| #settingssystem | SettingsSystem |
| #uisystem | UISystem |

## 커맨드·스킬 (`.claude/commands/` 파일명)
| 태그 | 대상 |
|---|---|
| #system-evaluation | 시스템 설계 적합성 평가 |
| #code-evaluation | 코드 품질 평가 |
| #code-convention | 코드 컨벤션 검사 |
| #comment-convention | 주석 컨벤션 검사 |
| #analysis-origin | 기존 코드 분석·설계 계획 |
| #evidence-research | 근거 조사 |
| #drawio-convention | drawio 다이어그램 규칙 |
| #diagram-html-convention | HTML 관계도 규칙 |
| #git-commit | 커밋 작성·승인 절차 |
| #git-issue | 이슈 작성·등록 절차 |

## 개념 (변경의 성격)
| 태그 | 뜻 |
|---|---|
| #interface | 인터페이스 도입·변경 |
| #refactor | 동작 안 바꾸고 구조만 바꿈 |
| #folder-reorg | 폴더 재배치 |
| #bugfix | 원래 되어야 할 게 안 되던 것을 고침 |
| #process | 커밋·이슈 등 작업 절차 자체를 바꿈 |
| #ai | AI 사용 편의를 위한 변경 |

## 상태
| 태그 | 뜻 |
|---|---|
| #problemsolved | 평가·리뷰로 찾은 문제를 그 커밋/이슈에서 해결함 |
| #wip | 진행 중, 후속 커밋 필요 |
