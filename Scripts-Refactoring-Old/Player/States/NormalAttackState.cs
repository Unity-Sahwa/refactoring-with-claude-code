using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://memmaeranger.tistory.com/46 애니메이션 프레임에 관한 자료
public class NormalAttackState : BaseState<PlayerStateEnum> , IPlayerInputAction
{
    public List<InputActionEnum> InputActionKey
    {
        get
        {
            return inputActionKey;
        }
    }
    private List<InputActionEnum> inputActionKey = new List<InputActionEnum>() { InputActionEnum.NormalAttack };
    public NormalAttackState(PlayerStateEnum key, StateManager<PlayerStateEnum> stateManager) : base(key, stateManager) 
    {
        CanReenter = true;
    }
    public override void EnterState()
    {
        Debug.Log("NormalAttackState EnterState");
    }
    public override void UpdateState()
    {
    }
    public override void ExitState()
    {
        Debug.Log("NormalAttackState ExitState");
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
