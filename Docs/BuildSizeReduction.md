# 빌드 용량 초과 문제 해결 보고서

- 작성일: 2026-08-28
- 대상: Android (Google Play Console 업로드)
- Unity: 2022.3.62f2 / URP

---

## 1. 문제

Play Console 업로드 시 경고 발생.

> App Bundle의 일부 기능 모듈이 최대 압축 다운로드 크기(500MB)를 초과합니다. base 모듈의 크기를 줄이세요.

초기 빌드 수치:

| 항목 | 값 |
| --- | --- |
| Total Size | 550.73 MB |
| Download Size (arm64-v8a) | 522.18 MB |
| Download Size (armeabi-v7a) | 520.06 MB |

---

## 2. 원인 분석

### 2.1 계측 도구 도입

추측 대신 실제 용량 분포를 보기 위해 `com.unity.build-report-inspector` 패키지를 설치함.
`Packages/manifest.json`에 `"com.unity.build-report-inspector": "0.3.0-preview"` 추가.

확인 경로: **Window > Open Last Build Report** → `SourceAssets` 탭.

### 2.2 계측 결과

상위 용량 에셋이 거의 전부 텍스처였음.

| 에셋 | 크기 |
| --- | --- |
| t_minotaur1_MetallicSmoothness.png | 21.33 MB |
| t_minotaur1_Normal.png | 21.33 MB |
| blck_ink.png (2개) | 17.85 MB ×2 |
| Solmoe_KDG_Medium SDF.asset | 16.00 MB |
| ON Wolinsokbo_L SDF.asset | 16.00 MB |
| white one.png | 15.71 MB |
| t_minotaur1_AO.png | 10.67 MB |
| (이하 5.33 MB 급 텍스처 수십 개) | |

**진단**: 5.33 MB / 21.33 MB 같은 수치는 비압축 상태의 크기임.
Player Settings의 Texture Compression 기본값은 개별 텍스처가 `Automatic`일 때만 적용되고,
플랫폼 오버라이드가 걸려 있거나 `Compression = None`인 텍스처는 무시됨.
또한 Max Size는 전역 설정이 없어 텍스처마다 개별 지정이 필요함.

오디오는 상위 목록에 존재하지 않아 우선순위에서 제외함.

---

## 3. 조치

### 3.1 텍스처 일괄 압축 도구 작성

`Assets/1.Code/Scripts/Editor/TextureCompressTool.cs` 신규 작성.

메뉴: **Tools > Texture > Android 압축 일괄 적용 (ASTC 6x6, 1024)**

동작:

- 프로젝트 전체 `t:Texture2D` 순회
- 각 텍스처의 **Android 플랫폼 오버라이드** 설정
  - `Override for Android` = 체크
  - `Max Size` = `Mathf.Min(기존값, 1024)` — 기존이 512면 512 유지, 작은 텍스처를 키우지 않음
  - `Format` = `ASTC_6x6`
  - `Compression` = `Compressed`
- 이미 동일 설정이면 건너뜀 (불필요한 재임포트 방지)
- 원본 파일은 수정하지 않음 (임포트 설정만 변경)

### 3.2 포맷 선정 근거

| 포맷 | 비트레이트 | 비고 |
| --- | --- | --- |
| ETC2 RGBA | 8 bpp | 고정, 구형 호환 |
| ASTC 4x4 | 8 bpp | 고품질 |
| **ASTC 6x6** | **3.56 bpp** | 품질/용량 절충 (채택) |
| ASTC 8x8 | 2 bpp | 뭉개짐 심함 |

ASTC 요구사항은 GLES 3.1 / Vulkan. Galaxy S10(Adreno 640, GLES 3.2)은 완전 지원.
실질 하한선은 Android 5.0~6.0대 저가 기기 수준.

Player Settings > Android > Texture Compression도 ETC2 → ASTC로 변경함.
(단 이는 `Automatic` 텍스처에만 적용되며, 스크립트가 건 오버라이드와는 독립적으로 동작함)

### 3.3 결과

| 항목 | 이전 | 이후 | 감소 |
| --- | --- | --- | --- |
| Total Size | 550.73 MB | 389.69 MB | -161 MB |
| Download (arm64-v8a) | 522.18 MB | **361.15 MB** | -161 MB |
| Download (armeabi-v7a) | 520.06 MB | 359.03 MB | -161 MB |

500MB 기준 통과. 텍스처 상위 항목이 2~5 MB 수준으로 평탄해짐.

---

## 4. 부수 문제: 터레인 번들거림

### 4.1 증상

압축 적용 후 터레인의 빛 반사가 과도하게 강해짐.

### 4.2 검증 과정

| 가설 | 검증 결과 |
| --- | --- |
| ASTC 압축 손상 | Format을 4x4로 올려도 재현 → 기각 |
| Max Size 축소 | 해상도를 올려도 재현 → 기각 |
| Normal Map Encoding (DXT5nm/XYZ) | 인코딩은 노멀 정밀도 문제이지 반사 강도와 무관 → 기각 |
| **Smoothness Source** | **Diffuse Alpha → Constant 변경 시 해소 → 확정** |

### 4.3 원인

터레인 머티리얼이 **Diffuse 텍스처의 알파 채널을 smoothness 소스로 사용**하고 있었음.
해당 알파 채널에 의미 있는 값이 들어있지 않은 상태에서 압축까지 거치며 값이 튐.

### 4.4 조치

Smoothness Source를 `Constant`로 변경. 번들거림 해소.

> 참고: 알파를 쓰지 않게 되었으므로 해당 텍스처를 RGB 포맷으로 전환하면 용량을 더 줄일 수 있음.
> 이미 목표치를 달성해 미적용.

---

## 5. 방안 비교 및 선택 근거

### 5.1 후보 목록

| 방안 | 예상 감소폭 | 작업량 | 부작용 |
| --- | --- | --- | --- |
| **텍스처 압축/해상도 조정** | 100~200 MB | 수 시간 | 화질 저하 (튜닝 가능) |
| Play Asset Delivery + Addressables | 사실상 무제한 | 수일 | 로딩 구조 전면 변경, 런타임 다운로드 실패 처리 필요 |
| SDF 폰트 Dynamic 전환 | 약 30 MB | 수 시간 | 런타임 글리프 생성 부하 |
| 오디오 압축 조정 | 미미 | - | - |

### 5.2 텍스처 압축을 택한 이유

**초과분이 22 MB에 불과했음.** 522 MB → 500 MB 미만이 목표였으므로
구조 변경 없이 임포트 설정만으로 도달 가능한 범위였고, 실제로 161 MB를 줄여 여유까지 확보함.

**용량의 원인이 텍스처에 집중되어 있었음.** Build Report 상위 항목이 거의 전부 텍스처였고,
그중 다수가 비압축 상태였음. 즉 "에셋이 너무 많은" 문제가 아니라 "설정이 안 된" 문제였음.
설정 누락이 원인인데 배포 구조를 바꾸는 것은 원인과 조치가 어긋남.

### 5.3 Play Asset Delivery를 보류한 이유

레거시 프로젝트에서는 PAD를 사용했으나, 이번에는 채택하지 않음.

- 현 프로젝트에 **Addressables 패키지 자체가 미설치**. PAD를 쓰려면 Addressables 도입부터 필요함
- 사용하지도 않던 로딩 시스템을 용량 문제 하나 때문에 도입하면
  에셋 참조 방식, 로딩 타이밍, 다운로드 실패 처리까지 전부 새로 설계해야 함
- 이는 22 MB 초과를 해결하기 위한 비용으로는 과함

**PAD가 정당해지는 조건**은 따로 있음.

- 압축을 최대한 적용하고도 500 MB를 넘는 경우
- 특정 챕터/DLC처럼 **전원이 받을 필요가 없는** 컨텐츠 덩어리가 존재하는 경우
- 이미 Addressables로 에셋을 로딩하고 있는 경우

현재는 셋 다 해당하지 않음. 향후 컨텐츠 추가로 다시 500 MB에 근접하면 재검토 대상.

### 5.4 정리

이번 대응의 순서는 **계측 → 원인 특정 → 원인에 대응하는 최소 조치**였음.
PAD를 먼저 잡았다면 수일을 쓰고도, 압축 안 된 텍스처는 그대로 남아
PAD 팩 자체가 비대해지는 결과가 됐을 것임.

다만 이는 "PAD가 나쁘다"는 뜻이 아니라 **이번 문제의 원인과 규모에 맞지 않았다**는 뜻임.
레거시 프로젝트의 선택이 틀렸는지는 그쪽의 초과 규모와 컨텐츠 구조를 따로 봐야 판단 가능함.

### 폰트 / 오디오

- SDF 폰트 아틀라스 2개 = 32 MB (전체의 약 6%). Dynamic 전환 시 감소하나 런타임 부하 발생 → 미적용
- 오디오는 상위 용량 목록에 미등장 → 조치 불필요

---

## 6. 남은 작업 / 향후 개선

### 6.1 즉시

- [ ] APK 실기기 테스트: UI 스프라이트 깨짐, 캐릭터 노멀맵 아티팩트 확인
- [ ] 문제 발견 시 해당 텍스처만 Max Size 2048 또는 ASTC 4x4로 상향

### 6.2 신규 프로젝트 대비

수백 장을 수동 관리하는 방식은 재발함. 두 가지 병행 필요.

**AssetPostprocessor** (필수)

임포트 시점에 폴더 규칙(`/UI/`, `/Character/`, `/Prop/`)별 Max Size 상한과 포맷을 자동 적용.
툴 실행을 잊어도 신규 에셋이 자동으로 규칙을 따름.
Max Size는 상한이므로 64px 원본을 키우지 않음.

**텍스처 관리 EditorWindow** (검수용, 선택)

`TextureCompressTool.cs` 하단에 TODO로 기록해둠.

- 수집 버튼: 전체 텍스처(또는 씬에서 실제 참조되는 텍스처만) 목록화
- 표 형태: 행 = 텍스처, 열 = 플랫폼별 MaxSize / Format / Compression
- 일괄 적용 버튼
- 예외 체크박스 — **경로가 아닌 GUID로 ScriptableObject에 저장해야 파일 이동 시 깨지지 않음**

단, 일괄 적용 기능 자체는 Project 창 `t:Texture2D` 검색 + 다중선택 인스펙터로도 가능하므로
툴의 실질 가치는 "플랫폼별 설정을 한눈에 보는 검수 뷰"에 있음.

---

## 7. 변경 파일

| 파일 | 변경 |
| --- | --- |
| `Packages/manifest.json` | `com.unity.build-report-inspector` 추가 |
| `Assets/1.Code/Scripts/Editor/TextureCompressTool.cs` | 신규 |
| Player Settings (Android) | Texture Compression: ETC2 → ASTC |
| 터레인 머티리얼 | Smoothness Source: Diffuse Alpha → Constant |
