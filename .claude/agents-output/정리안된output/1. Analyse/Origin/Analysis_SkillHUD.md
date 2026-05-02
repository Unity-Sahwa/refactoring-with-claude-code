# SkillHUD 분석

## 클래스 개요

| 항목 | 내용 |
|------|------|
| 클래스명 | SkillHUD |
| 현재 역할 | 플레이어 스킬 HUD 관리<br>- 스킬 쿨타임 표시<br>- 피니시 공격 HUD 활성/비활성화<br>- 마스크별 특수공격 아이콘 전환<br>- 스킬 상태 색상 관리 |
| 구현 디자인 패턴 | MonoBehaviour (UI 제어) |
| 분석날짜 | 2026-04-13 |

---

## 메서드 기능 상세분리

### ActivateFinishHUD(bool activate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 피니시 공격 HUD 표시/숨김 | 1. finishAttackImage.gameObject.SetActive(activate)로 HUD 활성/비활성화 | Image |

### SkillCooldown(PlayerStateType stateType, float flowTimeRate)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬별 이미지 선택 | 1. SetImage(stateType) 호출로 해당 스킬 이미지 획득 | SetImage() |
| 쿨타임 진행률 표시 | 1. flowTimeRate > 0.99f이면:<br>   - skillGuideImage.fillAmount = 1 (완전히 찬 상태)<br>2. 아니면:<br>   - skillGuideImage.fillAmount = flowTimeRate<br>   (진행률에 따라 이미지 채우기) | Image |

### ChangeGuideHUDColor(PlayerStateType stateType, Color color)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 스킬 가이드 색상 변경 | 1. SetImage(stateType) 호출로 해당 스킬 이미지 획득<br>2. skillGuideImage.color = color로 색상 설정 | SetImage() <br>Image |

### SetImage(PlayerStateType stateType)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 플레이어 상태별 이미지 매핑 | 1. switch/if문으로 stateType 판정:<br>   - HUMAN_INKSHAPE: humanSpecialAttackImage 반환<br>   - ANIMAL_LEAPSTRIKE: animalSpecialAttackImage 반환<br>   - DASH: dashImage 반환<br>   - GHOST_FINISHSKILL: finishAttackImage 반환<br>2. 매핑되는 Image 객체 반환 | Image |

### ChangeIcon(MaskType maskType)
| 기능 | 세부 기능 | 의존관계 |
|------|---------|--------|
| 마스크별 특수공격 아이콘 전환 | 1. maskType 파라미터 판정<br>2. MaskType.HUMAN 일 때:<br>   - humanSpecialAttackImage.transform.parent.gameObject.SetActive(true)<br>   - animalSpecialAttackImage.transform.parent.gameObject.SetActive(false)<br>   - specialAttackButton.targetGraphic을 humanSpecialAttackImage로 설정<br>3. MaskType.ANIMAL 일 때:<br>   - humanSpecialAttackImage.transform.parent.gameObject.SetActive(false)<br>   - animalSpecialAttackImage.transform.parent.gameObject.SetActive(true)<br>   - specialAttackButton.targetGraphic을 animalSpecialAttackImage로 설정<br>4. UI 버튼 상태 즉시 갱신<br>5. 마스크 변경 완료 시 자동 호출 | Image <br>Button <br>MaskType |
