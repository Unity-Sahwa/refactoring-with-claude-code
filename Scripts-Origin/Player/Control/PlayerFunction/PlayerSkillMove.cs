using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerSkillMove : MonoBehaviour
{
    [SerializeField] private HumanMaskSkill humanSkill;
    [SerializeField] private MaskChange maskChange;
    [SerializeField] private PlayerSensor playerSensor;

    private PlayerHumanMaskData humanData;
    private CameraData cameraData;


    private bool stopMoveCoroutine = false;

    private bool isMovingForward;
    private bool isMovingBackward;
    private bool isMovingUpward;
    private bool isMovingDownward;

    public bool IsMovingForward { get { return isMovingForward; } }
    public bool IsMovingBackward { get { return isMovingBackward; } }
    public bool IsMovingUpward { get { return isMovingUpward; } }
    public bool IsMovingDownward { get { return isMovingDownward; } }

    private Vector3 forwardVector3;
    private Vector3 backwardVector3;
    private Vector3 upwardVector3;
    private Vector3 downwardVector3;

    private float moveForwardSpeed;
    private float moveBackwardSpeed;
    private float moveUpwardSpeed;
    private float moveDownwardSpeed;

    private float originHeight; //원래 캐릭터 높이



    private void Start()
    {
        humanData = PlayerHumanMaskData.Instance;
        cameraData = CameraData.Instance;
    }

    public void Initialize()
    {
        stopMoveCoroutine = true;

        isMovingForward = false;
        isMovingBackward = false;
        isMovingUpward = false;
        isMovingDownward = false;

        moveForwardSpeed = 0;
        moveBackwardSpeed = 0;
        moveUpwardSpeed = 0;
        moveDownwardSpeed = 0;
    }

    //배열로 전달받도록하기
    public IEnumerator SkillMove(SkillMoveStruct moveStruct)
    {
        float coroutineStarTime = Time.time;

        stopMoveCoroutine = false;

        #region while 변수
        bool activeSwitch = false;
        #endregion

        while (!stopMoveCoroutine)
        {
            if (Time.time >= coroutineStarTime + moveStruct.waitTime + moveStruct.duration)
            {
                MoveSwitch(moveStruct.direction, false);

                yield break;
            }

            else if (!activeSwitch && (Time.time >= coroutineStarTime + moveStruct.waitTime))
            {
                MoveSwitch(moveStruct.direction, true);

                switch (moveStruct.direction)
                {
                    case MoveDirection.FRONT:
                        moveForwardSpeed = moveStruct.moveSpeed;
                        break;
                    case MoveDirection.BACK:
                        moveBackwardSpeed = moveStruct.moveSpeed;
                        break;
                    case MoveDirection.UP:
                        moveUpwardSpeed = moveStruct.moveSpeed;
                        break;
                    case MoveDirection.DOWN:
                        moveDownwardSpeed = moveStruct.moveSpeed;
                        break;
                        //case MoveDirection.RIGHT:
                        //    break;
                        //case MoveDirection.LEFT:
                        //    break;
                }

                activeSwitch =true;
            }

            yield return null;
        }
    }

    public void RayCastForward()
    {
        Vector3 characterPosition = maskChange.CurrentMask.transform.position;
        characterPosition.y = originHeight;
        characterPosition += maskChange.CurrentMask.transform.up;

        Vector3 characterPositionUp = characterPosition + maskChange.CurrentMask.transform.up;

        float distance = 2;

        #region 전방에 적감지
        RaycastHit hit;
        if (Physics.Raycast(characterPosition, maskChange.CurrentMask.transform.forward, distance, cameraData.enemyLayer))
        {
            isMovingForward = false;
        }
        if (Physics.Raycast(characterPosition + maskChange.CurrentMask.transform.right, maskChange.CurrentMask.transform.forward, distance, cameraData.enemyLayer))
        {
            isMovingForward = false;
        }
        if (Physics.Raycast(characterPosition - maskChange.CurrentMask.transform.right, maskChange.CurrentMask.transform.forward, distance, cameraData.enemyLayer))
        {
            isMovingForward = false;
        }

        if (Physics.Raycast(characterPositionUp, maskChange.CurrentMask.transform.forward, distance, cameraData.enemyLayer))
        {
            isMovingForward = false;
        }
        if (Physics.Raycast(characterPositionUp + maskChange.CurrentMask.transform.right, maskChange.CurrentMask.transform.forward, distance, cameraData.enemyLayer))
        {
            isMovingForward = false;
        }
        if (Physics.Raycast(characterPositionUp - maskChange.CurrentMask.transform.right, maskChange.CurrentMask.transform.forward, distance, cameraData.enemyLayer))
        {
            isMovingForward = false;
        }
        #endregion

        //원래 높이에서 상대가 앞에 있는지 탐지
    }

    public void GetOriginHeight()
    {
        originHeight = maskChange.CurrentMask.transform.position.y;
    }

    public void MoveSwitch(MoveDirection dir, bool moveSwitch)
    {
        switch (dir)
        {
            case MoveDirection.FRONT:
                isMovingForward = moveSwitch;
                break;
            case MoveDirection.BACK:
                isMovingBackward = moveSwitch;
                break;
            case MoveDirection.UP:
                isMovingUpward = moveSwitch;
                break;
            case MoveDirection.DOWN:
                isMovingDownward = moveSwitch;
                break;
        }
    }

    public void UpdateSkillMovement()
    {
        if (!isMovingForward && !isMovingBackward && !isMovingUpward && !isMovingDownward)
        {
            return;
        }

        Vector3 totalMovement = Vector3.zero;

        if (isMovingForward)
        {
            RayCastForward();
            forwardVector3 = maskChange.CurrentMask.transform.forward * moveForwardSpeed * Time.deltaTime;
        }
        else
        {
            forwardVector3 = Vector3.zero;
        }

        if (isMovingBackward)
        {
            backwardVector3 = -maskChange.CurrentMask.transform.forward * moveBackwardSpeed * Time.deltaTime;
        }
        else
        {
            backwardVector3 = Vector3.zero;
        }

        if (isMovingUpward)
        {
            upwardVector3 = maskChange.CurrentMask.transform.up * moveUpwardSpeed * Time.deltaTime;
        }
        else
        {
            upwardVector3 = Vector3.zero;
        }

        if (isMovingDownward)
        {
            downwardVector3 = -maskChange.CurrentMask.transform.up * moveDownwardSpeed * Time.deltaTime;
        }
        else
        {
            downwardVector3 = Vector3.zero;
        }

        if (playerSensor.canNotMoveforward)
        {
            forwardVector3 = Vector3.zero;
        }
        if (playerSensor.canNotMoveBackward)
        {
            backwardVector3 = Vector3.zero;
        }

        totalMovement = forwardVector3 + backwardVector3 + upwardVector3 + downwardVector3;



            maskChange.CurrentRigidbody.MovePosition
            (maskChange.CurrentMask.transform.position + totalMovement);
    }

}

