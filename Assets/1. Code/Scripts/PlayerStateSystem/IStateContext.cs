using System;
using UnityEngine;

namespace Refactoring
{
    //TODO: SO 이벤트 채널 방식 자세히 알아보기
    //addressable에서 직접 당겨오려면 비동기 시점이 awake와 맞지 않는 문제가 있나
    //이벤트 SO 관리자 클래스 하나 만들어서 할까
    //씬 이동 메커니즘이 갑자기 궁금함. 씬 옮길 때 start를 반복하던가
    //resources 폴더는 왜 쓰지말라는건가

    public interface IStateContext
    {
        public PlayerStateEventChannel EventChannel {get;}
        public IStateDataProvider StateDataManager { get;}
        public Animator HAnimator { get;}
        public Animator AAnimator { get;}

        // 상태 클래스가 이벤트 호출을 할 경우 관련된 연출 클래스들에게 알림할 기능 필요
    }
}
