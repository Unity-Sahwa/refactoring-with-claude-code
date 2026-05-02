using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : BaseState<PlayerStateEnum>, IPlayerInputAction
{
    public List<InputActionEnum> InputActionKey
    {
        get
        {
            return inputActionKey;
        }
    }
    private List<InputActionEnum> inputActionKey = new List<InputActionEnum>() { InputActionEnum.Jump };

    public JumpState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager)
    {
    }

    public override void EnterState()
    {
        Debug.Log("JumpState EnterState");
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        Debug.Log("JumpState ExitState");
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
