# 리팩토링 근거 — 에디터 툴 · 다국어 (레거시 대비 실측)

**주제: 손으로 반복하던 작업을 도구로 옮기기.** 데이터화만으로는 부족하고, 그 데이터를 사람이 다룰 수단까지 만들어야 함.

여기 적힌 것은 양쪽 코드 실측임. 추측은 적지 않음.

## 총량 지표 (실측)
| 지표 | before | after |
|---|---|---|
| 에디터 확장 파일 | **0** | **15파일 1,256줄** |
| `CustomEditor`/`EditorWindow`/`PropertyDrawer` | **0** | 15 |
| `UnityEditor` 참조 | 20파일 (전부 `using` 1회, 디버그 블록용) | — |
| 다국어 관련 | `LanguageManager` 68줄 + `TextManager` 97줄 + `MenuUI` 안에 혼재 | `LanguageSystem` 11파일 644줄 |

before에는 에디터 확장이 하나도 없었음. `UnityEditor`를 참조한 20개 파일은 전부 `using` 1회짜리
디버그용 블록이며 커스텀 인스펙터·창·드로어는 0개임.

## 만든 도구 (전부 신규)

### 최적화·정리 도구
| 도구 | 줄 | 하는 일 |
|---|---|---|
| `SceneComponentBatchEditor` | 202 | 씬 내 컴포넌트 값을 일괄 수정 |
| `MaterialDuplicateFinder` | 141 | 값이 같은 복제 머티리얼을 그룹으로 검출 (SRP Batcher 배칭 저해 원인) |
| `SceneUsageFinder` | 121 | 씬 전체에서 대상 사용처 추적 |
| `TextureCompressTool` | 70 | Android 텍스처 압축·Max Size 일괄 적용 |
| `MeshCombinerTool` | 66 | 같은 머티리얼 오브젝트를 메시 하나로 병합 |
| `RemoveMissingScriptsTool` | 39 | Missing 스크립트 일괄 제거 |

### 데이터 편집 도구
| 도구 | 줄 | 하는 일 |
|---|---|---|
| `TextTableCsv` | 178 | 다국어 텍스트 CSV 임포트 |
| `UIOrderViewer` | 148 | 씬의 Canvas 렌더 우선순위 확인 |
| `TextKeyDrawer` | 114 | 인스펙터에서 텍스트 키를 드롭다운 선택 |
| `StateDataEditor` | 54 | 상태 데이터 편집 UI (안 쓰는 항목을 가림) |
| `TextTableEntryDrawer` | 38 | 텍스트 테이블 항목 표시 |
| `ObjectActiveEntryDrawer` | 25 | 기믹 오브젝트 활성 항목 |
| `ObjectToggleDataEntryDrawer` | 20 | 플레이어 오브젝트 토글 항목 |
| `ShakeEntryDrawer` | 20 | 카메라 흔들림 항목 |
| `AudioCatalogEntryDrawer` | 20 | 오디오 카탈로그 ID를 이름으로 표시 |

PropertyDrawer 6종의 공통 목적: **SO에 넣은 데이터가 인스펙터에서 숫자·ID로만 보이는 문제를 없애는 것**.
데이터로 빼면 편집성이 떨어지므로 드로어를 같이 만들어야 데이터화가 완성됨.

## 다국어 — 자체 구현으로 돌아온 경로
### before
- `LanguageManager`(68줄) 싱글톤이 `ChangeLanguage(int index)`로 인덱스 기반 전환
- `TextManager`(97줄) 싱글톤은 타이핑 연출용 큐. 지역화와 역할이 섞임
- 실제 문구는 `MenuUI`(815줄) 안에 흩어짐

### after
| 구성 | 줄 | 책임 |
|---|---|---|
| `TextTableData` | 116 | 키-언어별 문구 테이블 (SO) |
| `LocalizedText` | 69 | 키로 문구를 가져와 표시 |
| `TextKeyAttribute` | 20 | 인스펙터에서 키를 드롭다운으로 고르게 하는 표시 |
| `LanguageSettings` / `ILanguageSettings` / `LanguageSettingsData` | 67 | 언어 설정과 저장 연동 |
| `LanguageButton` / `LanguageType` | 42 | 언어 전환 UI |
| `TextTableCsv` (에디터) | 178 | CSV로 번역 데이터 일괄 갱신 |

- Unity Localization + Addressable 패키지를 도입했다가 필요 없다고 판단해 제거하고 자체 구현으로 전환함
- 문자열 키를 손으로 타이핑하지 않도록 `TextKeyAttribute` + `TextKeyDrawer`로 드롭다운 제공.
  키 오타로 인한 조용한 실패를 구조적으로 막음
- 번역 갱신은 CSV 임포트로 처리. 번역자가 스프레드시트로 작업한 결과를 그대로 반영 가능

## 정리
- before: 에디터 확장 0개. 반복 작업은 전부 수작업이었음
- after: 도구 15개 1,256줄. 그중 6개는 **데이터화의 부작용(편집성 저하)을 되돌리기 위한 것**
- 다국어는 외부 패키지를 도입했다가 제거하고 자체 구현으로 돌아온 사례임.
  판단 근거는 `Evidence_AI.md`의 "AI가 만든 결과를 버린 기록"과 같은 맥락
