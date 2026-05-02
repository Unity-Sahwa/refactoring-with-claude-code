# PlayerHUD 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | PlayerHUD |
| 현재 역할 | 플레이어 HUD 이미지 관리<br>- 기본 이미지 배열 및 중첩 이미지 배열 관리<br>- 플레이어 마스크 아이콘 표시 및 업데이트 |
| 구현 디자인 패턴 | MonoBehaviour (UI 컴포넌트) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### UpdateMask(int maskIndex)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 마스크 인덱스 검증 | 1. maskIndex가 0 이상이고 maskSprites.Length 미만인지 확인 | - |
| 마스크 이미지 업데이트 | 1. maskIndex가 유효하면:<br>   - MaskHUD.sprite = maskSprites[maskIndex] 할당<br>   - MaskHUD.enabled = true로 활성화<br>2. Debug.Log(maskIndex)로 마스크 변경 로그 출력 | Image <br>Sprite |
