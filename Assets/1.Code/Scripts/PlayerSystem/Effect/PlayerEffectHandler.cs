using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // =============================================
    // PlayerEffectHandler 구현 목록
    // =============================================
    //
    // [초기화 / 로딩]
    // 1. 게임 시작 시 이펙트 프리팹을 "비동기"로 로딩 -> 스킬 사용 시 로딩 병목 제거
    // 2. 로딩한 프리팹으로 인스턴스 미리 생성 (스킬당 3개), 비활성 상태로 보관용 부모 밑에 캐시
    //    - 풀 시스템(클래스)은 과함. List<인스턴스> 캐시 수준이면 충분
    // 3. 부착 지점들을 DI로 주입받기 (마커 스크립트 + key(enum)로 여러 개 구분)
    //    - injectedImplements에 부착지점 타입 추가
    //    - this.transform 은 부착 후보 아님 (핸들러가 캐릭터에 안 붙음)
    //
    // [이벤트 구독] - Start / OnDestroy (현재 주석처리된 것 복구)
    // 4. Subscribe(Effect, HandleEffect) / SubscribeReset(HandleReset)
    // 5. OnDestroy에서 전부 Unsubscribe + 진행 중 코루틴 정리
    //
    // [HandleEffect] - 이펙트 켜기
    // 6. 들어온 데이터(SkillEffectDataEntry)로 캐시에서 비활성 인스턴스 하나 꺼냄
    //    - 같은 이펙트 겹칠 수 있으니(콤보) 3개 중 노는 것 선택
    // 7. 데이터의 부착지점 key로 해당 Transform을 부모 SetParent -> 따라다님
    // 8. position/rotation/scale 을 로컬 기준으로 적용
    // 9. SetActive(true) + 코루틴 시작
    //
    // [이펙트 수명 코루틴]
    // 10. duration(초)초 동안 활성 유지
    // 11. untilFinish == true  -> reset 와도 무시, duration 끝날 때까지 실행
    // 12. untilFinish == false -> reset 오면 즉시 종료
    // 13. "멈추는 시점"에 SetParent(null, worldPositionStays:true)로 부모 분리 -> 그 자리 정지
    //     ** 멈추는 시점 기준 = 아직 미정 (duration 끝 / 별도 필드 / 등) -> 데이터 확정 후 결정
    //
    // [HandleReset] - 상태 변경 시
    // 14. 진행 중인 이펙트 중 untilFinish==false 인 것만 즉시 종료(코루틴 Stop)
    //
    // [재활용]
    // 15. 끝난 이펙트는 SetActive(false) + 보관용 부모로 SetParent 복귀 -> 다음에 재사용
    //
    // =============================================
    // 선행 작업 (데이터 쪽)
    // - SkillEffectDataEntry 에 "부착 지점 key" 필드 추가
    // - "멈추는 시점" 기준 필드 추가 여부 결정
    // =============================================
    public class PlayerEffectHandler : MonoBehaviour, IInterfaceInjectable
    {
        public Dictionary<Type, List<object>> injectedImplements { get; } = new Dictionary<Type, List<object>>()
        {
            { typeof(IPlayerStateEventSubscriber), new List<object>() }
        };

        private IPlayerStateEventSubscriber _eventSubscriber;

        private void Start()
        {
            _eventSubscriber = (IPlayerStateEventSubscriber)injectedImplements[typeof(IPlayerStateEventSubscriber)][0];
            //_eventSubscriber.Subscribe(StateEventCategory.Effect, HandleEffect);
            //_eventSubscriber.SubscribeReset(HandleReset);
        }

        private void OnDestroy()
        {
            //_eventSubscriber.Unsubscribe(StateEventCategory.Effect, HandleEffect);
            //_eventSubscriber.UnsubscribeReset(HandleReset);
        }
    }
}
