# 코드 설명 문서: SaveSystem3

> 작성일: 2026-04-09

---

## 전체 흐름

```
외부 시스템 (플레이어, 설정UI 등)
    │  Save<T>(data) / Load<T>()
    ▼
ISaveService
    │
    ▼
SaveManager (MonoBehaviour)
    ├─ JSON 직렬화 / 역직렬화
    └─→ ISaveFileHandler
              └─→ LocalSaveFileHandler → 로컬 파일
```

---

## 인터페이스

### ISaveData
저장 데이터임을 표시하는 빈 인터페이스.
모든 저장 데이터 클래스가 이것을 구현해야 SaveManager에 전달할 수 있다.

### ISaveService
외부 시스템이 저장/로드를 요청할 때 사용하는 창구.

| 메서드 | 설명 |
|--------|------|
| `Save<T>(data)` | 데이터를 저장한다. 성공하면 true, 실패하면 false |
| `Load<T>()` | 저장된 데이터를 가져온다. 없으면 null |

### ISaveFileHandler
파일을 실제로 읽고 쓰는 역할을 담당하는 창구.
SaveManager는 이 인터페이스만 알고, 파일이 어디에 저장되는지 알지 못한다.

| 메서드 | 설명 |
|--------|------|
| `Write(fileName, data)` | 해당 이름으로 파일에 쓴다. 성공하면 true |
| `Read(fileName)` | 해당 이름의 파일을 읽는다. 없으면 null |
| `Delete(fileName)` | 해당 이름의 파일을 삭제한다. 성공하면 true |
| `Exists(fileName)` | 해당 이름의 파일이 있는지 확인한다 |

---

## 핵심 클래스

### SaveManager
저장/로드 요청을 받아 처리하는 핵심 클래스.
JSON으로 변환 후 ISaveFileHandler에 파일 처리를 맡긴다.

```
Save<T>(data) 호출 시
    ├─ JSON 변환 → 비어있으면 경고 후 false 반환
    └─ ISaveFileHandler.Write(파일명, json) → 결과 반환

Load<T>() 호출 시
    ├─ ISaveFileHandler.Read(파일명) → 파일 없으면 null 반환
    └─ JSON → T 로 변환 후 반환
```

| 메서드 | 설명 |
|--------|------|
| `Save<T>` | 데이터를 JSON으로 바꾼 뒤 파일에 저장한다 |
| `Load<T>` | 파일을 읽어 원래 데이터로 복원해 반환한다 |
| `GetFileName<T>` | 데이터 클래스의 FileName 상수가 있으면 그것을, 없으면 클래스 이름을 파일명으로 사용한다 |

---

### LocalSaveFileHandler
실제 파일을 읽고 쓰는 클래스.
쓰기 실패에 대비해 임시 파일로 먼저 쓴 뒤 정식 파일로 교체한다.

| 메서드 | 설명 |
|--------|------|
| `Write` | 임시 파일에 먼저 쓴 뒤 성공하면 정식 파일로 교체한다. 실패하면 임시 파일을 지우고 false 반환 |
| `Read` | 파일이 있으면 내용을 읽어 반환한다. 없으면 null |
| `Delete` | 파일이 있으면 삭제하고 true, 없으면 false |
| `Exists` | 파일 존재 여부를 확인한다 |

---

## 데이터 구조

### GameSaveData
인게임 진행 상태를 저장하는 데이터.

| 필드 | 설명 |
|------|------|
| `SceneIndex` | 현재 씬 번호 |
| `ZoneIndex` | 현재 구역 번호 |
| `MaskType` | 마스크 종류 (숫자로 저장) |
| `PlayerPosition` | 캐릭터 위치 |
| `Hp` | 현재 체력 |
| `LightingState` | 조명 켜짐 여부 |
| `PostProcessState` | 후처리 효과 켜짐 여부 |

### SoundData
볼륨 설정을 저장하는 데이터.

| 필드 | 설명 |
|------|------|
| `MasterVolume` | 전체 볼륨 |
| `BgmVolume` | 배경음 볼륨 |
| `EnemySfxVolume` | 적 효과음 볼륨 |
| `PlayerSfxVolume` | 플레이어 효과음 볼륨 |

### InputData
조작 설정을 저장하는 데이터.

| 필드 | 설명 |
|------|------|
| `KeyBindings` | 키 바인딩 목록 (액션 이름 + 키 코드) |
| `MouseSensitivity` | 마우스 감도 |

### SerializableVector3
Unity의 Vector3은 파일로 저장할 수 없어서 만든 대체 구조체.
Vector3과 자동으로 서로 변환된다.

### KeyBindingEntry
키 바인딩 하나를 저장하는 구조체.
Dictionary는 파일 저장이 안 되므로 목록(List) 형태로 보관하기 위해 사용한다.
