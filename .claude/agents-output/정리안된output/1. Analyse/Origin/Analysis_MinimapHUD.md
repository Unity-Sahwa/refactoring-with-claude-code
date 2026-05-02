# MinimapHUD 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | MinimapHUD |
| 현재 역할 | 미니맵 카메라 제어<br>- 미니맵 카메라 위치 플레이어 따라가기<br>- 플레이어 현재 마스크 상태 확인 |
| 구현 디자인 패턴 | MonoBehaviour (카메라 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### FixedUpdate()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 매 물리 프레임 미니맵 업데이트 | 1. ShowMinimap() 메서드 호출 | ShowMinimap() |

### ShowMinimap()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 미니맵 화면 표시 및 플레이어 추적 | 1. PlayerController.instance에서 플레이어 참조 획득<br>2. maskChange.CurrentMask의 위치 확인<br>3. CurrentMask가 null이면 조기 반환<br>4. minimap 게임오브젝트 활성화<br>5. CanvasGroup.alpha = 1 (완전 불투명)<br>6. minimapCamera의 위치를 플레이어 현재 마스크 위치로 동기화<br>   - X, Z좌표: 플레이어 위치와 동일<br>   - Y좌표: 고정값(카메라 높이 유지)<br>7. 매 FixedUpdate()마다 호출로 실시간 추적<br>8. 화면 하단 코너에 미니맵 표시<br>9. 적/아이템 마커도 함께 업데이트 | PlayerController <br>MaskChange <br>Camera <br>CanvasGroup <br>RectTransform |
