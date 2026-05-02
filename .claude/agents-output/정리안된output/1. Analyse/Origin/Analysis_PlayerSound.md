# PlayerSound 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerSound |
| 현재 역할 | 플레이어 사운드 효과 관리<br>- 사운드 재생 및 쿨타임<br>- 루프 사운드 제어<br>- 음성 일시정지/재개 |
| 구현 디자인 패턴 | MonoBehaviour (사운드 매니저) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 초기화 | 1. playerCommonData 참조 획득 <br>2. stopSoundCoroutine = false로 초기화 <br>3. lastLoop = false로 초기화 | PlayerCommonData |

### Initialize()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 사운드 상태 초기화 | 1. stopSoundCoroutine = true<br>2. StopLoopingAudio() 호출 | - |

### StartSet()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 오디오 시스템 초기화 | 1. 오디오 소스 배열 생성<br>2. AudioMixerGroup 설정<br>3. 10개 사운드 오브젝트 생성 | PlayerHumanMaskData |

### SetPlayerSound(SoundStruct soundStruct, Vector3 soundTarget, float skillStartTime)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 사운드 큐 추가 | 1. useFunction 확인<br>2. 루프 사운드 중복 방지<br>3. 비어있는 AudioSource 찾기<br>4. 오디오 소스 설정<br>5. TogglePlayerSound() 코루틴 시작 | PlayerState |

### TogglePlayerSound()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 사운드 재생 시간 제어 코루틴 | 1. waitTime 도달 시 Play()<br>2. 재생 완료 시 종료 | - |

### StopLoopingAudio()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 루프 사운드 중지 | 1. loop == true인 오디오 모두 중지<br>2. lastLoop = false | - |

### TogglePlayingAudioPause(bool turnSwitch)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 재생 중인 사운드 일시정지/재개 | 1. turnSwitch = true일 때:<br>   - 현재 재생 중인 모든 AudioSource를 순회<br>   - isPlaying==true인 모든 사운드를 Pause() 호출로 일시정지<br>   - audioPause 플래그 = true<br>2. turnSwitch = false일 때:<br>   - 모든 AudioSource를 순회<br>   - 이전에 일시정지된 사운드들을 Play() 호출로 재개<br>   - audioPause 플래그 = false<br>3. 메뉴 진입/퇴출 시 게임 음성 제어<br>4. 페이드 효과 중 사운드 음소거에 사용 | AudioSource |
