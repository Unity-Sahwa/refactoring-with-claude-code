# HpHUD 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | HpHUD |
| 현재 역할 | 플레이어 HP(체력) HUD 관리<br>- HP 스택(하트) 이미지 활성/비활성화<br>- HP 텍스트 표시 및 업데이트 |
| 구현 디자인 패턴 | 싱글톤 패턴 (Awake에서 인스턴스 관리) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Awake()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 싱글톤 초기화 | 1. instance가 null이면 현재 객체를 instance에 할당<br>2. instance가 이미 존재하면 현재 컴포넌트 제거 | - |

### Start()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 인스턴스 캐싱 | 1. Player.instance에서 player 객체 획득 | Player |

### ChangeHPStack(int currentHP)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| HP 값 검증 | 1. currentHP가 0 이하면 0으로 설정 | - |
| HP 스택 활성화 | 1. 0부터 currentHP-1까지 반복:<br>   - hpStack[i].SetActive(true)로 해당 인덱스 게임오브젝트 활성화 | GameObject |
| HP 스택 비활성화 | 1. currentHP부터 hpStack.Length-1까지 반복:<br>   - hpStack[i].SetActive(false)로 해당 인덱스 게임오브젝트 비활성화 | GameObject |
| HP 텍스트 업데이트 | 1. hpText.text = currentHP + "/" + hpStack.Length 형식으로 텍스트 설정<br>   (예: "3/5") | TextMeshProUGUI |
