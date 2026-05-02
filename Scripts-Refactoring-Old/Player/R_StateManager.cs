using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//모든 상태를 참조
//활성화된 상태를 확인
//상태에 속해있는 메소드를 호출
//상태 전환을 관리

public abstract class R_StateManager<EState> : MonoBehaviour where EState : Enum //제네릭 타입. Estate에는 열거형만 올 수 있음.
{
    //BaseState 클래스 또한 같은 열거형을 
    protected Dictionary<EState, BaseState<EState>> states = new Dictionary<EState, BaseState<EState>>();

    public EState PreviousState {get; private set;}
    public BaseState<EState> CurrentState {get; private set;}
    private EState nextState;

    protected bool IsTransitioningState = false;
     
    private void Start()
    {
        CurrentState.EnterState();
    }

    private void Update()
    {
        EState nextStateKey = nextState;
        

        if (!IsTransitioningState && nextStateKey.Equals(CurrentState.StateKey))
        {//상태전환중이 아니고 다음 상태가 현재상태랑 같을 때 업데이트 진행
            CurrentState.UpdateState();
        }
        else if (!IsTransitioningState)
        {
            TransitionToState(nextStateKey);
        }
    }

    public void GetNextState(EState estate)
    {//외부에서 상태를 전환해줌. (입력을 플레이어가 하기 때문에) (적AI라면 필요없음. 단, 플레이어 반응형이라면 사용할수도?)

        //현재상태와 매개변수 상태가 같을 때 + CanReenter가 true일 때 -> Start부터 재진입 가능하도록(원래는 Update 계속 진행)
        if (estate.Equals(currentState.StateKey))
        {
            if (currentState.CanReenter)
            {
                TransitionToState(estate);
                return;
            }
        }
        
        nextState = estate;
    }

    public void TransitionToState(EState statekey)
    {//다른 상태로 전환되는 과정
        IsTransitioningState = true; //TransitionToState 중복실행 방지
        currentState.ExitState();
        
        previousState = currentState.StateKey;

        currentState = states[statekey];
        currentState.EnterState();
        IsTransitioningState = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
    private void OnTriggerStay(Collider other)
    {
        
    }
    private void OnTriggerExit(Collider other)
    {
        
    }
}
