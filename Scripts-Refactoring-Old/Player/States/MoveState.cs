using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveState : BaseState<PlayerStateEnum>, IPlayerInputAction
{
    public List<InputActionEnum> InputActionKey
    {
        get
        {
            return inputActionKey;
        }
    }
    private List<InputActionEnum> inputActionKey = new List<InputActionEnum>() {InputActionEnum.Up, InputActionEnum.Down, InputActionEnum.Left, InputActionEnum.Right };

    private R_PlayerStateMachine playerStateMachine;
    public MoveState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager) 
    {
        key = PlayerStateEnum.Move;
        playerStateMachine = (R_PlayerStateMachine)stateManager;
    }

    public override void EnterState()
    {
        Debug.Log("MoveState EnterState");
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        Debug.Log("MoveState ExitState");
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
