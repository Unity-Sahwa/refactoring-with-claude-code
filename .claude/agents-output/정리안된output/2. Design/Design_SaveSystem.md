# 설계 문서: SaveSystem3

> 작성일: 2026-04-09
> 설계 담당: 설계담당 (Claude Sonnet 4.6)
> 참고 문서: Analysis_Origin_FEATURES.md (6. 저장 시스템)

---

## 핵심 요약

외부 시스템이 SaveManager에 "이 데이터 저장해줘" / "이 데이터 줘"만 요청하면 SaveManager가 직렬화·파일 읽기/쓰기를 처리한다.
데이터 종류가 늘어나도 ISaveData를 구현하기만 하면 SaveManager 코드 수정 없이 확장된다.
SaveManager는 MonoBehaviour로 Unity 씬에 직접 배치한다.

---

## 1. 전체 시스템 흐름 다이어그램

```
외부 시스템
    │  Save<T>(data) / Load<T>()
    ▼
ISaveService
    │
    ▼
SaveManager
    ├─ 직렬화 / 역직렬화 (JsonUtility)
    └─→ ISaveFileHandler  (파일 쓰기 / 읽기)
              └─→ 로컬 파일 (persistentDataPath)

데이터 구조
    ISaveData
    ├── GameSaveData    (인게임 저장)
    └── SettingsData    (설정 저장)
```

---

## 2. 핵심 기능

### 저장 / 로드

SaveManager는 데이터를 받으면 JSON으로 바꾸고 → 파일에 쓴다.
로드는 반대 순서로 파일 읽기 → 원래 데이터로 복원해서 돌려준다.

#### 저장 흐름

```
Save<T>(data) 호출
    → JSON 직렬화 → string
    → ISaveFileHandler.Write(파일명, string)
```

#### 로드 흐름

```
Load<T>() 호출
    → ISaveFileHandler.Read(파일명) → string
    → JSON 역직렬화 → T 반환 (없으면 null)
```

#### 파일명 결정 방식

`typeof(T).Name` 을 파일명으로 사용한다. (예: `GameSaveData.sav`, `SettingsData.sav`)

#### 참고 패턴: 제네릭 + DIP (의존성 역전 원칙)

- `Save<T>` / `Load<T>`로 데이터 종류에 관계없이 동일한 방식으로 처리한다.
- SaveManager는 ISaveFileHandler 인터페이스에만 의존하므로 구현체 교체가 자유롭다.

| 항목 | 내용 |
|------|------|
| 장점 | ISaveData를 구현하기만 하면 새 데이터 타입 추가 시 SaveManager 수정 불필요 |
| 장점 | ISaveFileHandler 교체만으로 저장 매체 변경 가능 |
| 단점 | 파일명이 `typeof(T).Name` 기반이므로 클래스 이름 변경 시 기존 저장 파일을 읽지 못함 |
| 단점 | `Save<T>` / `Load<T>`는 타입당 파일 하나 → 슬롯(여러 저장 파일) 개념이 없음. 슬롯이 필요하면 별도 확장 필요 |
| 단점 | T에 `[Serializable]` 어트리뷰트가 없어도 컴파일 오류가 나지 않아 런타임에서야 직렬화 실패를 알 수 있음 |
| 단점 | Load 실패(파일 없음·손상) 시 반환값이 null → 호출부에서 null 처리를 빠뜨리면 NullReferenceException 발생 |
| 특징 | ISaveData는 동작 없는 마커 인터페이스 — 타입 제약과 확장 지점 역할만 한다 |

---

### ISaveFileHandler

파일을 실제로 읽고 쓰는 역할만 담당한다.
SaveManager는 파일이 어디에 어떻게 저장되는지 알지 못한다.

| 메서드 | 설명 |
|--------|------|
| `Write(fileName, string)` | 해당 이름으로 데이터를 파일에 쓴다 |
| `Read(fileName)` | 해당 이름의 파일을 읽어 string으로 반환한다. 없으면 null |
| `Delete(fileName)` | 해당 이름의 파일을 삭제한다 |
| `Exists(fileName)` | 해당 이름의 파일이 있는지 확인한다 |

| 항목 | 내용 |
|------|------|
| 장점 | SaveManager가 파일 경로·형식에 의존하지 않으므로 저장 방식 교체가 쉬움 |
| 단점 | 쓰기 실패(디스크 꽉 참, 권한 없음) 시 예외를 던질지 bool을 반환할지 인터페이스 계약이 명확하지 않음 → 정책 결정 필요 |
| 단점 | 파일 쓰기 중 강제 종료되면 손상된 파일이 남을 수 있음 → 임시 파일로 먼저 쓴 뒤 교체하는 방식으로 구현체에서 처리 필요 |

---

## 3. 외부와의 상호작용

외부 시스템은 ISaveService 인터페이스를 통해서만 SaveManager에 접근한다.
SaveManager를 직접 참조하지 않는다.

```
외부 시스템 → ISaveService.Save<GameSaveData>(data)
외부 시스템 → ISaveService.Load<GameSaveData>()

외부 시스템 → ISaveService.Save<SettingsData>(data)
외부 시스템 → ISaveService.Load<SettingsData>()
```

| 항목 | 내용 |
|------|------|
| 장점 | 외부 시스템은 ISaveService만 알면 되므로 SaveManager 내부 변경에 영향받지 않음 |
| 단점 | 외부에서 데이터를 직접 채워서 넘겨야 하므로 외부 코드가 DTO 구조에 의존 — DTO 필드가 바뀌면 외부 코드도 수정 필요 |
| 단점 | 저장 성공/실패를 외부에 알릴 방법이 현재 없음 → 조용히 실패하면 사용자는 저장됐다고 생각할 수 있음 |

---

## 4. 구현 참고사항

### 인터페이스 목록

| 인터페이스 | 역할 | 구현체 예시 |
|-----------|------|------------|
| `ISaveService` | 외부 진입점 | `SaveManager` |
| `ISaveFileHandler` | 파일 읽기/쓰기 | `LocalSaveFileHandler`, `FakeSaveFileHandler` |
| `ISaveData` | 저장 데이터 마커 | `GameSaveData`, `SettingsData` |

### 클래스 의존 방향

```
외부 시스템
    │
    ▼
ISaveService ◄── SaveManager (MonoBehaviour) ──► ISaveFileHandler
```

### 데이터 직렬화 주의사항

- `Vector3`, `Dictionary` 는 JsonUtility로 직렬화 불가 → SerializableVector3 래퍼, List<KeyBindingEntry> 변환 필요
- 모든 저장 데이터 클래스에 `[Serializable]` 어트리뷰트 필수
- DTO 필드 이름 변경 시 기존 저장 파일과 역직렬화 불일치 발생 → 이전 저장 데이터가 기본값으로 채워짐

### Unity 적용 지침

- SaveManager는 MonoBehaviour로 작성, Unity 씬에 직접 배치
- DontDestroyOnLoad는 SaveManager 자신의 Awake에서 처리
- `Application.persistentDataPath`는 ISaveFileHandler 구현체 안에서만 사용
