<!-- 피드백
[제목]
- '카메라 기준 플레이어 움직임 시스템 구현'

[본문]
- playerMovement 코드를 분석해서 무엇을 구현했는가 좀 더 잘 적어주기
    - 키 입력을 카메라 기준으로 계산하여 플레이어 이동 및 회전 로직 구현
    - '캐릭터 교체 시 현재 캐릭터 타입을 갱신하여 교체된 캐릭터의 Rigidbody로 이동 처리.' > 뭔말인지 모르겠어
    - 본문에 '변경 내용' 파트로 왜 분리한거야? 이유를 말해줘
    - 의존성 주입은 왜 말하는거야? 이유를 말해줘
    - 원래 메서드 하나하나 본문에서 소개해야하는거야?? 웹 검색해볼래?
-->

feat(PlayerMovement_cs): 플레이어 이동 시스템 구현

카메라 기준 방향 계산을 통한 플레이어 이동 및 회전 로직 추가.
캐릭터 교체 시 현재 캐릭터 타입을 갱신하여 교체된 캐릭터의 Rigidbody로 이동 처리.

변경 내용:
- IInterfaceInjectable 구현 및 IInputEventProvider, ICharacterSwapNotifier, BaseCharacter 의존성 주입
- FixedUpdate에서 이동 벡터가 존재할 때만 UpdateMoveDirection, Move, Rotate 실행
- UpdateMoveDirection: 카메라 forward/right 벡터에서 Y축 제거 후 정규화하여 이동 방향 계산
- Move: 현재 캐릭터 Rigidbody의 MovePosition으로 이동 처리
- Rotate: Quaternion.Slerp 기반 부드러운 회전 처리
