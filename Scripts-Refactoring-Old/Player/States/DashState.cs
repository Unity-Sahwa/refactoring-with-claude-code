using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashState : BaseState<PlayerStateEnum>, IPlayerInputAction
{
    public List<InputActionEnum> InputActionKey
    {
        get
        {
            return inputActionKey;
        }
    }
    private List<InputActionEnum> inputActionKey = new List<InputActionEnum>() { InputActionEnum.Dash };

    public DashState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager)
    {
    }

    public override void EnterState()
    {
        Debug.Log("DashState EnterState");
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        Debug.Log("DashState ExitState");
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
