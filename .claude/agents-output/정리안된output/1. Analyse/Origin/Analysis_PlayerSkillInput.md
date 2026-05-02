# PlayerSkillInput 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerSkillInput |
| 현재 역할 | 플레이어 스킬 입력 큐 및 실행 관리<br>- 입력 저장 및 버퍼링<br>- 입력 시간 윈도우 관리<br>- 입력 실행 |
| 구현 디자인 패턴 | MonoBehaviour (입력 시스템) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 버퍼 초기화 | 1. canStoreInputValue = false<br>2. 입력 스택 초기화<br>3. stopCoroutine = true | - |

### StoreInput(PlayerStateType inputValue)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 스택에 저장 | 1. canStoreInputValue 확인<br>2. playerSkillInputValue.Push(inputValue) | - |

### ProcessInputDirectly(PlayerSkillInputStruct skillInput, float startTime)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 직접 입력 처리 (버퍼 없음) | 1. CoProcessInputDirectly() 코루틴 시작 <br>2. 입력 버퍼를 거치지 않고 직접 처리 | - |

### ProcessInput(PlayerSkillInputStruct skillInput, float startTime)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 처리 시작 | 1. CoProcessInput() 코루틴 시작 | - |

### CoProcessInput()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 저장/실행 시간 윈도우 관리 코루틴 | 1. storeWaitTime부터 storeDuration 동안: canStoreInputValue = true<br>2. executeWaitTime부터 executeDuration 동안: 저장된 입력 실행<br>3. PopExecuteStoredInput() 호출<br>4. Initialize() 호출 | - |

### CoProcessInputDirectly()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 직접 입력 처리 시간 윈도우 관리 코루틴 | 1. storeWaitTime부터 storeDuration 동안: canStoreInputValue = true <br>2. executeWaitTime부터 executeDuration 동안: 저장된 입력 직접 실행 <br>3. ExecuteStoredInputDirectly() 호출 <br>4. Initialize() 호출 | - |

### ExecuteStoredInput(PlayerStateType inputValue)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 입력 실행 | 1. 입력 타입별 처리:<br>   - HUMAN_NORMALATTACK: 다음 단계 공격<br>   - HUMAN_INKSHAPE: 잉크 쉐이프<br>   - DASH: 대시<br>   - 등등 | PlayerState <br>HumanMaskSkill <br>AnimalMaskSkill <br>MaskChange <br>CameraController |

### ExecuteStoredInputDirectly(PlayerStateType inputValue)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 저장된 입력 직접 실행 (버퍼링 스킵) | 1. 입력 큐(playerSkillInputValue)에서 저장된 입력 추출<br>2. Pop() 호출로 입력 큐에서 제거<br>3. 추출된 PlayerStateType 값 판정<br>4. HUMAN_NORMALATTACK: humanMaskSkill.NormalAttack() 즉시 실행<br>5. HUMAN_INKSHAPE: humanMaskSkill.InkShape() 즉시 실행<br>6. DASH: 현재 마스크에 따라 대시 실행<br>7. 기타 스킬: 대응하는 스킬 메서드 즉시 실행<br>8. 예약 없이 순간적 입력 처리<br>9. 콤보 입력이나 빠른 연타 상황에서 활용<br>10. 입력 버퍼 후 최종 실행 단계 | PlayerStateType <br>HumanMaskSkill <br>AnimalMaskSkill <br>MaskChange <br>GhostMaskSkill |
