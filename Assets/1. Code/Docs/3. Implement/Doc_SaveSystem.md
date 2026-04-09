# 코드 설명 문서: SaveSystem

> 작성일: 2026-04-09

---

## 전체 흐름

```
외부 시스템 (플레이어, 설정UI, 씬전환 등)
    │
    ▼
ISaveService ◄── SaveManagerInstaller (Unity 씬에 붙이는 컴포넌트)
    │
    ▼
SaveManager (저장/로드 핵심 처리)
    ├──► IEncryptionService  →  데이터 암호화/복호화
    ├──► ISaveRepository<GameSaveData>  →  인게임 슬롯 파일 읽기/쓰기
    └──► ISaveRepository<SettingsData>  →  설정 파일 읽기/쓰기
```

---

## 인터페이스

### ISaveService
외부 시스템이 저장/로드를 요청할 때 사용하는 창구.
SaveManager가 이 인터페이스를 구현하며, 외부는 이것만 알면 된다.

| 메서드 | 설명 |
|--------|------|
| `SaveGame(data)` | 인게임 데이터를 저장한다 |
| `LoadGame(slotId)` | slotId에 해당하는 인게임 데이터를 가져온다 |
| `GetGameSlots()` | 저장된 슬롯 목록 전체를 가져온다 |
| `DeleteGame(slotId)` | slotId에 해당하는 슬롯을 삭제한다 |
| `SaveSettings(data)` | 설정 데이터를 저장한다 |
| `LoadSettings()` | 저장된 설정 데이터를 가져온다 |

---

### ISaveRepository\<T\>
파일을 실제로 읽고 쓰는 역할을 담당하는 창구.
T는 어떤 종류의 데이터를 다루는 저장소인지 구분하는 표시다.

| 메서드 | 설명 |
|--------|------|
| `Save(id, data)` | id 이름으로 데이터를 저장한다 |
| `Load(id)` | id에 해당하는 데이터를 읽어온다. 없으면 null |
| `Delete(id)` | id에 해당하는 데이터를 삭제한다 |
| `GetAllIds()` | 저장된 모든 id 목록을 가져온다 |

---

### IEncryptionService
데이터를 암호화하고 복호화하는 역할을 담당하는 창구.
실제 암호화 방식은 구현체가 결정하므로 나중에 바꾸기 쉽다.

| 메서드 | 설명 |
|--------|------|
| `Encrypt(data)` | 데이터를 암호화해서 반환한다 |
| `Decrypt(data)` | 암호화된 데이터를 원래대로 되돌려 반환한다 |

---

## 핵심 클래스

### SaveManager
저장/로드 요청을 받아 실제로 처리하는 핵심 클래스.
파일 처리와 암호화를 직접 하지 않고 각 담당에게 맡긴다.

```
SaveGame(data) 호출 시
    ├─ SlotId(고유번호), SavedAt(저장시각) 부여
    ├─ 슬롯이 10개 이상이면 → 가장 오래된 슬롯 찾아서 교체
    ├─ 데이터 → JSON 문자열 → 암호화 → 파일 저장
    └─ 메모리 슬롯 목록 갱신

LoadGame(slotId) 호출 시
    └─ 파일 읽기 → 복호화 → JSON 역변환 → 데이터 반환

GetGameSlots() 호출 시
    └─ 메모리에 올라온 슬롯 목록 복사본 반환
       (시작 시 파일에서 한 번 읽어 메모리에 유지)
```

| 메서드 | 설명 |
|--------|------|
| `SaveGame` | 인게임 데이터를 슬롯으로 만들어 저장한다 |
| `LoadGame` | slotId로 해당 슬롯 데이터를 찾아 반환한다 |
| `GetGameSlots` | 현재 슬롯 목록 전체를 반환한다 |
| `DeleteGame` | 해당 슬롯을 파일과 메모리에서 모두 지운다 |
| `SaveSettings` | 설정 데이터를 저장한다 |
| `LoadSettings` | 저장된 설정 데이터를 반환한다. 없으면 null |
| `LoadAllSlots` | 시작 시 파일에서 슬롯 전체를 읽어 메모리에 올린다 |
| `FindOldestSlot` | 슬롯 목록 중 저장 시각이 가장 오래된 것을 찾는다 |
| `Serialize` | 데이터를 JSON으로 바꾼 뒤 암호화한다 |
| `Deserialize` | 암호화된 데이터를 복호화한 뒤 원래 데이터로 되돌린다 |

---

### SaveManagerInstaller
Unity 씬에 붙이는 컴포넌트. SaveManager를 만들고 필요한 것들을 연결해준다.
에디터에서는 암호화 없이, 실제 빌드에서는 AES 암호화를 사용한다.

| 메서드 | 설명 |
|--------|------|
| `Awake` | SaveManager와 저장소, 암호화 서비스를 만들어 연결한다 |
| `CreateEncryptionService` | 에디터면 암호화 없음, 빌드면 AES 암호화를 반환한다 |
| `LoadOrCreateKey` | 저장된 암호화 키가 있으면 불러오고, 없으면 새로 만들어 저장한다 |

---

## 구현체

### AesEncryptionService
AES 방식으로 데이터를 암호화/복호화한다.
키와 IV(초기화 벡터)를 생성자로 받아 사용한다.

### NoEncryptionService
암호화 없이 데이터를 그대로 반환한다.
에디터에서 저장 파일 내용을 직접 확인할 때 사용한다.

### FileSaveRepository\<T\>
`Application.persistentDataPath` 아래 폴더에 파일로 저장한다.
쓰기 실패에 대비해 임시 파일로 먼저 쓴 뒤 정식 파일로 교체한다.

### FakeSaveRepository\<T\>
파일 없이 메모리(Dictionary)에만 저장한다.
테스트할 때 실제 파일 없이 저장/로드 흐름을 검증하는 용도다.

---

## 데이터 구조

### GameSaveData
인게임 저장 슬롯 하나의 데이터.
SlotId와 SavedAt이 슬롯을 구분하고 정렬하는 기준이 된다.

| 필드 | 설명 |
|------|------|
| `SlotId` | 슬롯 고유 번호 (자동 생성) |
| `SavedAt` | 저장한 시각 (문자열) |
| `SceneIndex` | 현재 씬 번호 |
| `ZoneIndex` | 현재 구역 번호 |
| `MaskType` | 마스크 종류 |
| `PlayerPosition` | 캐릭터 위치 |
| `Hp` | 현재 체력 |
| `LightingState` | 조명 켜짐 여부 |
| `PostProcessState` | 후처리 효과 켜짐 여부 |

### SettingsData
볼륨, 키 설정 등 게임 설정 데이터.
슬롯 개념 없이 파일 하나에 덮어써서 저장한다.

### SerializableVector3
Unity의 Vector3은 파일로 저장할 수 없어서 만든 대체 구조체.
Vector3과 자동으로 서로 변환된다.

### KeyBindingEntry
키 바인딩 하나를 나타내는 구조체.
`Dictionary`는 파일 저장이 안 되므로 목록(List) 형태로 보관한다.
