using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAttackState : BaseState<PlayerStateEnum>, IPlayerInputAction
{
    public List<InputActionEnum> InputActionKey
    {
        get
        {
            return inputActionKey;
        }
    }
    private List<InputActionEnum> inputActionKey = new List<InputActionEnum>() { InputActionEnum.SpecialAttack };

    public SpecialAttackState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager) {}
    public override void EnterState()
    {
        Debug.Log("SpecialAttackState EnterState");
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        Debug.Log("SpecialAttackState ExitState");
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
