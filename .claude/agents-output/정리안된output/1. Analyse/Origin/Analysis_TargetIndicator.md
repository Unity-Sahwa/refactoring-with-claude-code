# TargetIndicator 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | TargetIndicator |
| 현재 역할 | 화면 밖의 타겟 인디케이터 표시<br>- 타겟이 화면 밖에 있을 때 가장자리에 인디케이터 표시<br>- 타겟 깜빡임 효과(blink) 구현 |
| 구현 디자인 패턴 | MonoBehaviour (UI 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### Update()
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 타겟 화면좌표 변환 | 1. mainCamera.WorldToScreenPoint(target.position)로 월드좌표를 화면좌표로 변환 | Camera <br>Transform |
| 카메라 뒤의 타겟 처리 | 1. screenPosition.z < 0이면(카메라 뒤의 타겟):<br>   - screenPosition *= -1 로 반대쪽으로 뒤집기<br>   - screenPosition.z = 0 설정 | - |
| 화면 내 타겟 판정 | 1. screenPosition.z > 0 여부 확인<br>2. screenPosition.x가 0과 Screen.width 사이 여부 확인<br>3. screenPosition.y가 0과 Screen.height 사이 여부 확인<br>4. 모든 조건을 만족하면 isOnScreen = true | Screen |
| 화면 내 타겟 표시 | 1. isOnScreen이 true면:<br>   - indicator.transform.position = screenPosition 으로 정확한 위치에 표시 | Image |
| 화면 밖 타겟 가장자리 표시 | 1. isOnScreen이 false면:<br>   - screenPosition.x를 edgeOffset ~ (Screen.width - edgeOffset) 범위로 제한<br>   - screenPosition.y를 edgeOffset ~ (Screen.height - edgeOffset) 범위로 제한<br>   - 계산된 위치에 인디케이터 표시 | Screen <br>Mathf |
| 타겟 깜빡임 효과 | 1. Mathf.PingPong()으로 0~1 사이 반복하는 알파값 생성<br>2. Time.time * blinkSpeed로 깜빡임 속도 조절<br>3. indicator.color.a에 알파값 적용 | Image <br>Color |
