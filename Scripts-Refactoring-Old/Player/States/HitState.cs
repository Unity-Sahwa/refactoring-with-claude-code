using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitState : BaseState<PlayerStateEnum>
{
    private R_PlayerStateMachine playerStateMachine;
    public HitState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager) 
    {
        key = PlayerStateEnum.Damaged;
        playerStateMachine = (R_PlayerStateMachine)stateManager;
    }

    public override void EnterState()
    {
        Debug.Log("HitState EnterState");
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        Debug.Log("HitState EnterState");
    }
    public override void OnTriggerEnter(Collider other)
    {
        throw new System.NotImplementedException();
    }
    public override void OnTriggerExit(Collider other)
    {
        throw new System.NotImplementedException();
    }
    public override void OnTriggerStay(Collider other)
    {
        throw new System.NotImplementedException();
    }
}
