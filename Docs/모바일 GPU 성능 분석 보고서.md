# 모바일 GPU/CPU 병목 분석 보고서

- 작성일: 2026-08-28
- 대상 기기: Galaxy S10 (SM-G973N, Exynos 9820 / Mali-G76 MP12)
- Unity: URP
- 목표: 40 FPS 유지

---

## 1. 문제

모바일 기기에서 40 FPS 목표를 유지하지 못함. 특히 아래 두 상황에서 급격한 프레임 하락 관찰됨.

- 씬5: 보스 스킬의 불기둥 이펙트가 9개 동시 생성될 때 10 FPS대까지 하락
- 씬1/4/5: 조명 intensity가 0으로 전환되는 순간 순간적으로 프레임 급락
- 그 외 씬(배경 위주): 40 FPS 근처에서 애매하게 못 미치는 수준의 저하

원인이 GPU인지 CPU인지, 어느 렌더링 단계인지 특정이 안 된 상태에서 시작.

---

## 2. 분석 환경 구축

Unity 자체 Profiler는 대부분의 모바일 기기에서 GPU 타이밍을 지원하지 않아, 아래 툴 조합으로 분석 진행.

| 툴 | 용도 |
| --- | --- |
| Unity Frame Debugger | 프레임 내 draw call 구조, 배칭 여부, 사용 셰이더/키워드 확인 |
| Unity Profiler (CPU 모듈) | 메인 스레드 시간 분포, 프레임 hitch 지점 특정 |
| Arm Performance Studio (Streamline) | Mali GPU 하드웨어 카운터 실측 (Fragment/Non-fragment 사용률, Overdraw, Core Unit Utilization 등) |
| URP Rendering Debugger | Overdraw Mode로 화면상 오버드로우 분포 시각화 |

**Streamline 연동 조건**: 비루팅 기기 기준 Development Build(=`debuggable=true`)로만 캡처 가능. 일반 릴리즈 APK는 프로파일링 데몬을 붙일 수 없음.

**Frame Debugger 성격**: 화면 캡처가 아니라 프레임의 draw call 목록/렌더 상태를 캡처하는 도구. GPU/CPU 소요 시간(ms)은 안 나옴 — 그건 Profiler/Streamline 몫.

---

## 3. 분석 결과

### 3.1 Draw Call 구조 (Frame Debugger)

메인 카메라 기준 프레임당 190개 draw call. `DrawOpaqueObjects`(127개) 세부 분해:

| 그룹 | 개수 | 셰이더 |
| --- | --- | --- |
| Terrain 기본 패스 | ~40 | `URP/Terrain/Lit` |
| Terrain Add Pass | ~20 | `Hidden/URP/Terrain/Lit (Add Pass)` (레이어 4개 초과분) |
| 풀(Detail Billboard) | 36 | `Hidden/TerrainEngine/Details/.../BillboardWavingDoubleSided` |
| SRP Batcher 처리됨 | 27 | 캐릭터/나무/돌기둥 등, 문제 아님 |

**결론**: 터레인 6개 타일 분할 + 레이어 5개(4개 초과로 Add Pass 발생) + 풀 디테일이 draw call 수의 대부분을 차지. SRP Batcher가 묶은 부분은 정상.

### 3.2 씬별 GPU 카운터 비교 (Streamline CSV, Mali 하드웨어 카운터)

| 지표 | 씬1 | 씬2 | 씬3 | 씬4 | 씬5 |
| --- | --- | --- | --- | --- | --- |
| Fragment queue 사용률 | 87% | 87% | 84% | 83% | 80% |
| Overdraw (fragments/pixel) | 1.84 | 1.86 | 1.91 | 1.79 | **2.50** |
| Arithmetic 유닛 사용률 | 43% | 44% | 47% | 46% | **53%** |
| GPU 실행 코어 사용률 | 85% | 85% | 85% | 86% | 82% |
| CPU 빅코어(Exynos-M4) 사용률 | 99% | 1% | 23% | 50% | 99% |

**결론**:
- 모든 씬에서 GPU Fragment(픽셀) 단계 사용률이 80~87%로 지배적 → 병목이 정점/타일링이 아니라 픽셀 처리에 있음
- 씬5가 오버드로우·연산량 모두 최고치 → 불기둥 이펙트 구간이 GPU 부하 최댓값
- 씬1/4/5에서 CPU 특정 코어가 99%로 고정되는 현상 별도 발견 (3.4에서 원인 규명)

### 3.3 오버드로우 실측 (Rendering Debugger > Overdraw Mode)

URP는 Scene 뷰 기본 Draw Mode의 Overdraw를 지원하지 않음(빌트인 파이프라인 전용) → Rendering Debugger의 Overdraw Mode로 대체.

- 평지 터레인: 1~2단계(정상)
- 풀(Detail) 밀집 구역: 5~8단계로 국소적으로 매우 높음

**원인**: 풀 하나가 겹친 십자형 평면(Cross Quad) 구조인 데다 Detail Density/Distance 설정으로 밀도까지 높아 누적. CSV의 평균 오버드로우(1.8~1.9)의 실제 진원지로 확인.

### 3.4 조명 전환 시 프레임 hitch (Unity Profiler CPU, 748ms 프레임)

씬4에서 조명 intensity가 0으로 바뀌는 프레임을 Profiler로 포착.

| 항목 | 비중 | 시간 |
| --- | --- | --- |
| 전체 프레임 CPU | - | 748ms |
| DrawOpaqueObjects | 82.5% | 617ms |
| └ Shader.CreateGPUProgram (SRPBatcher) | 42.4% | 318ms |
| └ Shader.CreateGPUProgram (StdRender) | 39.9% | 299ms |
| └└ Semaphore.WaitForSignal | 대부분 | GPU 드라이버 컴파일 완료 대기 |

Frame Debugger로 동일 프레임의 Terrain 셰이더 Keywords를 조명 켜짐/꺼짐 상태로 비교:

- 켜짐: `..._MAIN_LIGHT_SHADOWS_CASCADE`, `_SHADOWS_SOFT` 포함
- 꺼짐(intensity 0): 위 두 키워드 빠지고 `_MAIN_LIGHT_SHADOWS`(Cascade 없음)로 전환

**원인**: 조명 intensity 0 전환 시 URP가 그림자 키워드 조합을 바꿈 → 해당 조합의 셰이더 variant가 런타임에 처음 쓰이며 즉석 컴파일 발생 → 대형 hitch.

**적용 시도**: Shader Variant Collection(`Assets/5.Data/ShaderVariants/ShaderVariants.shadervariants`)을 Play 세션(조명 2분 이상 켜짐 유지 후 끔) 기반으로 Save to Asset하여 Preloaded Shaders에 등록.
**검증 결과**: 파일 안에 `Terrain` 문자열이 포함된 셰이더가 **전혀 없음** → Unity의 Shader Variant 트래킹이 Terrain 셰이더(런타임 레이어 조합 방식)를 캡처하지 못하는 알려진 한계로 확인. 이 부분은 **미해결 상태**로 남음(6장 참고).

### 3.5 보스 이펙트 GPU 분석 (불기둥 9개 동시 생성)

동일 프레임을 CPU/GPU 양쪽에서 확인.

| 단계 | GPU 비중 | GPU 시간 |
| --- | --- | --- |
| 전체 프레임 | 99.1% | 25.401ms |
| Setup Camera | **74.9%** | **19.215ms** |
| DrawTransparentObjects (실제 이펙트) | 20.4% | 5.239ms |

Setup Camera가 비정상적으로 큰 비중을 차지 → 프리팹/머티리얼 추적.

**추적 경로**: `Assets/3.Content/Enemy/BossWisu/Effect/` 프리팹(불기둥 계열 6종) → 공통으로 `Noise18bd1/2/3.mat` 머티리얼 참조 → 셰이더 `HS_BlendDistort.shadergraph`(Hovl Studio 에셋) → 그래프 안에 **Scene Color 노드**(카메라 컬러 텍스처 샘플링, 왜곡 효과용) 존재.

**결정적 확인**: 세 머티리얼 모두 `_Distortionpower: 0` — 즉 **왜곡 효과가 시각적으로 전혀 적용되지 않는 상태**인데도, 셰이더가 매 프레임 화면 전체를 복사(Scene Color)하는 비용은 그대로 지불하고 있었음. 100% 낭비 비용.

그래프 추가 분석 결과, Scene Color 출력은 Branch 노드를 거쳐 있었고 그 Predicate는 `#ifdef HAVE_DECALS`로 HDRP 여부만 판별하는 Custom Function이었음. 이 프로젝트는 URP이므로 Predicate는 항상 false → Branch는 항상 Scene Color 경로(True측 HDRP 폴백은 죽은 코드)로 귀결. 즉 그래프에 조건부 스위치가 있었지만 URP에서는 사실상 무의미했음.

---

## 4. 원인 요약

| 문제 | 원인 | 심각도 |
| --- | --- | --- |
| 보스 이펙트 9개 생성 시 프레임 급락 | 오버드로우(반투명 다중 겹침) + 불필요한 Scene Color 카메라 복사 | 높음 |
| 조명 intensity 0 전환 시 hitch | 그림자 키워드 조합 변경 → 런타임 셰이더 컴파일 | 높음 |
| 배경 렌더링 전반 저하 | 터레인 타일 분할(6개)·레이어 5개(Add Pass)·풀 오버드로우 | 중간 |
| CPU 특정 코어 99% 고정(씬1/4/5) | 미확정 (GPU 대기 vs 실연산, 3.4 hitch와 동일 구간일 가능성) | 확인 필요 |

---

## 5. 검토한 개선 방법과 선택

### 5.1 보스 이펙트 Scene Color 문제

검토안:
1. 머티리얼 3개의 셰이더를 Distort 없는 버전(`HS_Blend_CG`)으로 교체
2. **HS_BlendDistort.shadergraph 자체를 수정해 Scene Color 관련 노드 제거** ← 선택
3. 그대로 두고 동시 생성 개수만 제한

**선택 이유**: 이 Distort 셰이더를 쓰는 머티리얼이 프로젝트 전체에 3개뿐이라 영향 범위가 한정적이었고, `_Distortionpower`가 이미 0이라 시각적 손실 없이 제거 가능함을 확인했기 때문. 1번(머티리얼 교체)도 동일 효과지만 그래프를 직접 고치는 쪽이 원본 에셋 구조를 이해하는 데 더 정확했음.

### 5.2 조명 전환 hitch (셰이더 variant)

검토안:
1. Shader Variant Collection + Preloaded Shaders ← 시도했으나 Terrain 셰이더가 트래킹에서 누락되어 **미해결**
2. 로딩/Fade 구간에 더미 오브젝트로 실제 렌더링 1프레임 강제 실행(워밍업) — 트래킹 버그와 무관하게 동작
3. 수동으로 ShaderVariantCollection에 키워드 조합을 직접 입력(Add Shader 버튼) — Terrain 셰이더는 Shader 피커에 안 뜨는(Hidden 셰이더) 문제로 UI로는 불가, 스크립트(`ShaderVariantCollection.Add()`)로만 가능

**미정**: 2번(더미 워밍업)이 트래킹 버그에 의존하지 않는 가장 확실한 방법으로 결론 내렸으나, 아직 구현 전 단계.

### 5.3 배경 렌더링(터레인/풀) 저하

검토안:
1. Terrain Detail Density/Distance 하향 ← 근본 해결 아님(그리는 범위/양을 줄이는 임시방편이라는 지적 있었음), 우선순위 보류
2. 풀 셰이더 Alpha Blend → Alpha Test(Cutout) 전환 (Early-Z 활용 가능) — 셰이더 확인 필요, 미착수
3. 터레인 6타일 → 1개로 통합 (씬이 더 이상 변경될 일 없어 통합 가능하다고 판단) — 방법은 확인함(에디터 스크립트로 Heightmap/Alphamap/Tree/Detail 데이터 병합), 미착수

### 5.4 CPU 코어 99% 고정

원인 후보 두 가지를 구분하지 못한 상태:
1. 실제 연산(Physics/Script/Animator 등)이 그 코어를 점유
2. GPU 완료를 기다리는 busy-wait/스핀락이 "활동"으로 잡힘 (GPU 병목의 결과일 뿐일 가능성)

Profiler에서 `Gfx.WaitForPresent`/`WaitForTargetFPS` 비중을 봐야 구분 가능 — **미확인**.

---

## 6. 적용된 조치

- `HS_BlendDistort.shadergraph`에서 Scene Color 노드, Branch 노드, HDPR Custom Function 노드 삭제 → 해당 자리에 Vector2(0,0) 상수 노드로 대체
- 적용 대상 머티리얼: `Noise18bd1.mat`, `Noise18bd2.mat`, `Noise18bd3.mat` (Fire thrower 계열 프리팹 6종에서 참조)
- 효과: 이 머티리얼이 활성화될 때마다 발생하던 카메라 컬러 텍스처 전체 복사(GPU 19.2ms, 프레임의 74.9%) 제거. 시각적 변화 없음(원래 왜곡 강도 0이었음)

---

## 7. 남은 작업

- [ ] 터레인 셰이더 variant 워밍업: 로딩/Fade 구간에서 조명 intensity 0 상태 포함 더미 렌더링으로 강제 컴파일 (3.4, 5.2)
- [ ] CPU 코어 99% 고정 원인 특정: `Gfx.WaitForPresent` 비중 확인 (5.4)
- [ ] 풀(Detail) 오버드로우 개선: 셰이더 Blend/Cutout 여부 확인 후 전환 검토 (5.3)
- [ ] 터레인 6타일 → 1타일 병합 (에디터 스크립트로 Heightmap/Alphamap/Tree/Detail 병합) (5.3)
- [ ] 보스 이펙트 동시 생성 개수 제한(풀링 캡) — Scene Color 제거로 완화됐으나 오버드로우 자체는 별개 문제로 남음
- [ ] 조치 이후 씬1~5 재측정으로 개선폭 정량 확인 (Streamline CSV 재비교)

---

## 8. 참고: 사용 툴/명령

- Frame Debugger: Window > Analysis > Frame Debugger
- Rendering Debugger: Window > Analysis > Rendering Debugger, Rendering 탭 > Fullscreen Debug Mode > Overdraw
- Arm Performance Studio (Streamline): https://developer.arm.com/Tools%20and%20Software/Arm%20Mobile%20Studio (ADB 경로 수동 설정 필요, Preferences)
- ShaderVariantCollection: Project Settings > Graphics > Shader Loading > Preloaded Shaders
