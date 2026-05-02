using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class R_PlayerMovement : MonoBehaviour, IPlayerMovable
{
    public PlayerComponentEnum ComponentID
    {
        get
        {
            return componentID;
        }
    }
    private PlayerComponentEnum componentID = PlayerComponentEnum.PlayerBasicMove;

    public Rigidbody PlayerRigidbody
    {
        get
        {
            return playerRigidbody;
        }
    }
    private Rigidbody playerRigidbody;
    public CapsuleCollider PlayerCollider
    {
        get
        {
            return playerCollider;
        }
    }


    private CapsuleCollider playerCollider;

    private float moveAmount;
    
    public Vector3 DirVector
    {
        get
        {
            return dirVector;
        }
    }

    private Vector3 dirVector;
    private Quaternion playerRotationQuaternion;

    private void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
    }

    public void CharacterRotate(Vector3 movement)
    {
        if (movement.AlmostZero()) return;
        playerRotationQuaternion = Quaternion.LookRotation(movement);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, playerRotationQuaternion, 15);
    }
    //0903
    public void CharacterMove(Vector3 movement)
    {
         playerRigidbody.MovePosition(transform.position + movement * 5 * Time.deltaTime);
    }

    private void ConvertToPlayerDirection(Dictionary<InputActionEnum, int> dirValue)
    {
        //if (isInputBlockedByMenu || isInputBlockedByCutScene) return;

        //카메라 뷰 기준 방향 데이터
        int verticalValue = dirValue[InputActionEnum.Up] - dirValue[InputActionEnum.Down];
        int horizontalValue = dirValue[InputActionEnum.Right] - dirValue[InputActionEnum.Left];

        Vector3 camForward;
        Vector3 camRight;

        camForward = Camera.main.transform.forward;
        camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        Vector3 forwardRelative = verticalValue * camForward;
        Vector3 rightRelative = horizontalValue * camRight;

        dirVector = (forwardRelative + rightRelative).normalized;
    }

    //플레이어
    public void GetInput(Vector3 dirVector, PlayerMoveActionEnum actionEnum)
    {
        throw new System.NotImplementedException();
    }
}
