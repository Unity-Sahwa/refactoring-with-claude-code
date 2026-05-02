using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : BaseState<PlayerStateEnum>
{
    public FallState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager)
    {
    }

    public override void EnterState()
    {
        Debug.Log("FallState EnterState");
    }
    public override void UpdateState()
    {
        Debug.Log("FallState UpdateState");
    }
    public override void ExitState()
    {
        Debug.Log("FallState ExitState");
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
