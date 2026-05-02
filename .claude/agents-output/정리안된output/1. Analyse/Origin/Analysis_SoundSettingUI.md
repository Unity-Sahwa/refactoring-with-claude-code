# SoundSettingUI 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | SoundSettingUI |
| 현재 역할 | 음성 설정 UI 관리<br>- 마스터, BGM, 적 SFX, 플레이어 SFX 볼륨 제어<br>- 슬라이더 입력에 따른 실시간 음성 조절<br>- 설정 저장 및 로드 |
| 구현 디자인 패턴 | MonoBehaviour (UI 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 슬라이더 이벤트 등록 | 1. masterAudioSlider.onValueChanged.AddListener(SetMasterVolume) 등록<br>2. BGMAudioSlider.onValueChanged.AddListener(SetBGM) 등록<br>3. enemySFXSlider.onValueChanged.AddListener(SetEnemySFX) 등록<br>4. playerSFXSlider.onValueChanged.AddListener(SetPlayerSFX) 등록<br>5. 슬라이더 값 변경 시 자동으로 대응 메서드 호출 | Slider |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| SaveManager 참조 및 저장 데이터 로드 | 1. SaveManager.instance를 saveManager에 할당<br>2. LoadVolumeData() 호출로 이전 저장된 음성 설정 복원 | SaveManager <br>LoadVolumeData() |

### SetMasterVolume(float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 마스터 볼륨 모든 카테고리에 적용 | 1. enemyAudioMixer.SetFloat("Master", value) 호출<br>2. playerAudioMixer.SetFloat("Master", value) 호출<br>3. envAudioMixer.SetFloat("Master", value) 호출<br>4. 세 개 믹서의 "Master" 파라미터 동시 설정으로 전체 음성 볼륨 제어 | AudioMixer |

### SetBGM(float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 환경음 BGM 및 일반 환경음 SFX 설정 | 1. envAudioMixer.SetFloat("BGM", value) 호출<br>2. envAudioMixer.SetFloat("SFX", value) 호출<br>3. BGM과 환경음 SFX를 동일하게 제어 | AudioMixer |

### SetEnemySFX(float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 적 음성 효과 볼륨 설정 | 1. enemyAudioMixer.SetFloat("SFX", value) 호출<br>2. 모든 적의 음성 효과(공격음, 피격음 등) 볼륨 조절 | AudioMixer |

### SetPlayerSFX(float value)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 음성 효과 볼륨 설정 | 1. playerAudioMixer.SetFloat("SFX", value) 호출<br>2. 플레이어의 모든 음성 효과(공격음, 피격음, 스킬음 등) 볼륨 조절 | AudioMixer |

### MuteAudio()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 전체 음성 음소거 | 1. SetMasterVolume(0) 호출<br>2. 마스터 볼륨을 0으로 설정하여 모든 음성 음소거 | SetMasterVolume() |

### SaveVolumeData()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 현재 음성 설정 저장 | 1. masterAudioSlider.value 획득<br>2. BGMAudioSlider.value 획득<br>3. enemySFXSlider.value 획득<br>4. playerSFXSlider.value 획득<br>5. saveManager.ChangeVolumeSetting(master, bgm, enemySFX, playerSFX) 호출로 설정 변경<br>6. saveManager.SaveSoundData() 호출로 파일에 저장 | SaveManager |

### LoadVolumeData()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 저장된 음성 설정 로드 및 적용 | **AudioMixer에 설정 적용:**<br>1. SetMasterVolume(saveManager.masterVolume) 호출<br>2. SetBGM(saveManager.BGMVolume) 호출<br>3. SetEnemySFX(saveManager.enemySFXVolume) 호출<br>4. SetPlayerSFX(saveManager.playerSFXVolume) 호출<br><br>**UI 슬라이더 동기화:**<br>5. masterAudioSlider.value = saveManager.masterVolume<br>6. BGMAudioSlider.value = saveManager.BGMVolume<br>7. enemySFXSlider.value = saveManager.enemySFXVolume<br>8. playerSFXSlider.value = saveManager.playerSFXVolume<br><br>**목적:** 게임 시작 시 이전 설정을 완벽히 복원 | SaveManager <br>SetMasterVolume() <br>SetBGM() <br>SetEnemySFX() <br>SetPlayerSFX() |
