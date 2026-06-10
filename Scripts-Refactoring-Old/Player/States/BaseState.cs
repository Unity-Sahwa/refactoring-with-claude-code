using UnityEngine;
using System;
using System.Collections.Generic;

//0904
//Estate에 열거형만 올 수 있음.
public abstract class BaseState<Estate>  where Estate : Enum
{
    public BaseState(Estate key, StateManager<Estate> stateManager)
    {
        StateKey = key;
        CanReenter = false;
    }

    public Estate StateKey { get; protected set; }
    
    //true일 경우 같은 상태에 다시 진입할 때, Start()함수부터 실행됨.
    //false일 경우 Update() 함수가 계속 반복됨.
    public bool CanReenter { get; protected set; }    

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void UpdateState();
    public abstract void OnTriggerEnter(Collider other);
    public abstract void OnTriggerStay(Collider other);
    public abstract void OnTriggerExit(Collider other);
}
