using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

//�ִϸ��̼� �� ���� ����, ��� ���� ��ȣ ����
public class R_PlayerStateMachine : R_StateManager<PlayerStateEnum>, IUserInputReceiver
{
    private Dictionary<PlayerComponentEnum, IPlayerComponent> playerComponents = new Dictionary<PlayerComponentEnum, IPlayerComponent>();

    private IPlayerMovable playerMovement;
    private IPlayerAnimatable humanAnimator;
    private IPlayerAnimatable animalAnimator;

    //x�� �¿�(horizontal), y�� ���Ʒ�(jump), z�� �յ�(vertical)
    private Vector3 dirVector;

    //���� �׼� Ŭ������
    private List<IPlayerInputAction> playerInputActions = new List<IPlayerInputAction>();
    private Dictionary<InputActionEnum, PlayerStateEnum> dictPlayerInputActions = new Dictionary<InputActionEnum, PlayerStateEnum>();

    public Dictionary<PlayerInputHandleEnum, int> DictPlayerInputHandle
    {
        get
        {
            return dictPlayerInputHandle;
        }
    }
    private Dictionary<PlayerInputHandleEnum, int> dictPlayerInputHandle;
    //EStateKey Ȱ��
    private void Awake()
    {
        #region ���� �߰�
        states.Add(PlayerStateEnum.Move, new MoveState(PlayerStateEnum.Move, this));

        states.Add(PlayerStateEnum.Jump, new JumpState(PlayerStateEnum.Jump, this));
        //states.Add(PlayerStateEnum.Fall, new JumpState(PlayerStateEnum.Fall, this)); 
        states.Add(PlayerStateEnum.Dash, new DashState(PlayerStateEnum.Dash, this));

        states.Add(PlayerStateEnum.NormalAttack, new NormalAttackState(PlayerStateEnum.NormalAttack, this));
        states.Add(PlayerStateEnum.HSpecialAttack, new SpecialAttackState(PlayerStateEnum.HSpecialAttack, this));
        states.Add(PlayerStateEnum.HFinishAttack, new FinishAttackState(PlayerStateEnum.HFinishAttack, this));

        states.Add(PlayerStateEnum.Damaged, new HitState(PlayerStateEnum.Damaged, this));
        #endregion

        currentState = states[PlayerStateEnum.Move];

        foreach (var item in states)
        {
            //item�� IPlayerInputAction�� ������ Ŭ�������� ����
            if (item.Value is IPlayerInputAction playerAction)
            {
                //IPlayerInputAction�� ������ Ŭ������� playerInputActions�� �߰�
                playerInputActions.Add(playerAction);

                foreach (var key in playerAction.InputActionKey)
                {
                    if (!dictPlayerInputActions.ContainsKey(key))
                    {
                        //�Է�Ű �׼ǰ� �÷��̾� ���¸� ¦���� Dictionary�� ����.
                        dictPlayerInputActions.Add(key, item.Key);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {

    }


    //���� ����
    public void Initialize(List<IPlayerComponent> components)
    {
        foreach (var item in components)
        {
            if (!playerComponents.ContainsKey(item.ComponentID)) //�ߺ�����
            {
                playerComponents.Add(item.ComponentID, item);
            }
        }

        playerMovement = (IPlayerMovable)playerComponents[PlayerComponentEnum.PlayerBasicMove];
        humanAnimator = (IPlayerAnimatable)playerComponents[PlayerComponentEnum.HumanAnimation];
        animalAnimator = (IPlayerAnimatable)playerComponents[PlayerComponentEnum.AnimalAnimation];
    }

    #region �Է� ����
    //����� �Է½�, ����
    public void DeliverInput(InputActionEnum inputAction, InputStateEnum inputState)
    {
        //���Ⱚ ����
        if (inputAction == InputActionEnum.Up || inputAction == InputActionEnum.Left || inputAction == InputActionEnum.Down || inputAction == InputActionEnum.Right || inputAction == InputActionEnum.Jump)
        {
            ConvertToDirVector(inputAction, inputState);
        }

        //����� �Է¿� �´� ���·� ����.
        GetNextState(dictPlayerInputActions[inputAction]);
    }
    private void ConvertToDirVector(InputActionEnum inputAction, InputStateEnum inputState)
    {
        int stateBuffer;

        if (inputState == InputStateEnum.Down)
        {
            stateBuffer = 1;
        }
        else
        {
            stateBuffer = 0;
        }

        switch (inputAction)
        {
            case InputActionEnum.Up:
                dirVector.z = 1 * stateBuffer;
                break;
            case InputActionEnum.Down:
                dirVector.z = -1 * stateBuffer;
                break;
            case InputActionEnum.Right:
                dirVector.x = 1 * stateBuffer;
                break;
            case InputActionEnum.Left:
                dirVector.x = -1 * stateBuffer;
                break;
            case InputActionEnum.Jump:
                dirVector.y = 1 * stateBuffer;
                break;
        }
    }


    //�Է�Ű �׼��� ���� Ŭ������ ����
    public List<InputActionEnum> DeliverInputActions()
    {
        List<InputActionEnum> ListBuffer = new List<InputActionEnum>();

        //���� �׼� Ŭ�������� InputActionEnum ���� �з� �� ����
        foreach (var actionItem in playerInputActions)
        {
            ListBuffer.AddRange(actionItem.InputActionKey);
        }

        return ListBuffer;
    }
    #endregion
}
