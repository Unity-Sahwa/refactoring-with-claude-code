# 구현 검토 문서: SaveSystem3

> 작성일: 2026-04-09
> 참고 설계: Design_SaveSystem.md

---

## 1. 구현 클래스 목록

| 클래스 | 유형 | 역할 |
|--------|------|------|
| `ISaveData` | Interface | 저장 데이터 마커. 모든 데이터 클래스가 구현 |
| `ISaveService` | Interface | 외부 진입점. Save\<T\> / Load\<T\> |
| `ISaveFileHandler` | Interface | 파일 읽기/쓰기 추상화 |
| `SaveManager` | MonoBehaviour | ISaveService 구현. 직렬화 + ISaveFileHandler 위임 |
| `LocalSaveFileHandler` | Class | persistentDataPath 기반 실제 파일 읽기/쓰기 |
| `GameSaveData` | Class | 인게임 저장 데이터 |
| `SoundData` | Class | 사운드 설정 데이터 |
| `InputData` | Class | 조작 설정 데이터 |
| `SerializableVector3` | Struct | Vector3 직렬화 래퍼 |
| `KeyBindingEntry` | Struct | KeyBindings Dictionary 직렬화 래퍼 |

---

## 2. 동작 흐름 다이어그램

### 2-1. 저장 흐름

```
외부 시스템
    │ Save<T>(data)
    ▼
SaveManager
    ├─ JsonUtility.ToJson(data) → string
    └─→ ISaveFileHandler.Write(typeof(T).Name, string)
              └─ 파일 쓰기
```

### 2-2. 로드 흐름

```
외부 시스템
    │ Load<T>()
    ▼
SaveManager
    └─→ ISaveFileHandler.Read(typeof(T).Name) → string
              ├─ 파일 없음 → null 반환
              └─ JsonUtility.FromJson<T>(string) → T 반환
```

### 2-3. 클래스 의존 방향

```
외부 시스템
    │
    ▼
ISaveService ◄── SaveManager (MonoBehaviour) ──► ISaveFileHandler
                                                       ↓
                                              LocalSaveFileHandler
```

---

## 3. 기능 요약

- **Save\<T\>**: 데이터를 JSON으로 직렬화 후 파일명(`typeof(T).Name`)으로 저장
- **Load\<T\>**: 파일명으로 파일 읽기 후 역직렬화. 파일 없으면 null 반환
- **ISaveFileHandler**: 파일 경로·방식은 구현체가 결정. SaveManager는 모름
- **직렬화 보조**: Vector3 → SerializableVector3, Dictionary → List\<KeyBindingEntry\>

---

## 4. 설계 단점 개선 방안

| 설계 단점 | 개선 방안 |
|-----------|-----------|
| 클래스 이름 변경 시 기존 파일 못 읽음 | 각 데이터 클래스에 `public const string FileName` 상수 정의. typeof(T).Name 대신 이 값을 파일명으로 사용 |
| `[Serializable]` 누락 시 런타임 실패 | ISaveData에 제약을 걸 수 없으므로 문서에 필수 명시. LocalSaveFileHandler에서 저장 전 JsonUtility.ToJson 결과가 빈 문자열이면 경고 로그 출력 |
| Load 실패 시 null → NullReferenceException | ISaveFileHandler.Read가 null 반환 시 SaveManager가 null 그대로 반환. 호출부 책임임을 문서에 명시 |
| Vector3 직렬화 불가 | SerializableVector3 래퍼 구조체. Vector3과 암묵적 변환 연산자로 자동 변환 |
| Dictionary 직렬화 불가 | List\<KeyBindingEntry\>로 저장. KeyBindingEntry는 actionName(string) + keyCode(int) |
| 파일 쓰기 실패 계약 불명확 | ISaveFileHandler.Write 반환 타입을 bool로 정의. true = 성공, false = 실패. SaveManager는 실패 시 호출부에 false 전달 |
