using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;

    private PlayerController playerController;
    private MaskChange maskChange;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerSensor playerSensor;

    private SaveManager saveManager;
    [SerializeField] private VariableJoystick joystick;
    [SerializeField] private MobileInput mobileInput;

    private PlayerCommonData commonData;

    private float horizontalMove;
    private float verticalMove;
     

    private enum SignType
    {
        NONE,
        POSITIVE,
        NEGATIVE
    }
    private SignType[] horizontalArray = new SignType[2];
    private SignType[] verticalArray = new SignType[2];

    private float moveAmount;

    private Vector3 movement;
    public Vector3 Movement { get { return movement; } }

    private Quaternion targetRotation;
    public Quaternion TargetRotation
    {
        set { targetRotation = value; }
        get { return targetRotation; }
    }
    private float moveSpeed;


    private void Awake()
    {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion

        playerController = GetComponent<PlayerController>();
    }
    private void Start()
    {
        saveManager = SaveManager.instance;
        maskChange = playerController.maskChange;
        commonData = PlayerCommonData.Instance;
    }

    public float InputMovementValue(float moveValue, bool isHorizontal)
    {
        //문제: negative 버튼을 계속누르고 있을 때, positive 버튼을 누르면 바로 전환 가능. 하지만 positive 누른 상태에서 negative 누르면 X (if문 순서차이)

        float value = moveValue;
        KeyAction positiveValue;
        KeyAction negativeValue;
        SignType[] array;

        if (isHorizontal)
        {
            positiveValue = KeyAction.RIGHT;
            negativeValue = KeyAction.LEFT;
            array = horizontalArray;
        }
        else
        {
            positiveValue = KeyAction.UP;
            negativeValue = KeyAction.DOWN;
            array = verticalArray;
        }

        #region change SignType
        if (Input.GetKey(saveManager.InputKeys[positiveValue]))
        {
            //SignType.POSITIVE가 저장되어 있지 않은 경우에만 SignType.NONE에 저장
            bool hasValue = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == SignType.POSITIVE)
                {
                    hasValue = true;
                    break;
                }
            }
            if (!hasValue)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == SignType.NONE)
                    {
                        array[i] = SignType.POSITIVE;
                        break;
                    }
                }
            }
        }
        else
        {//SignType.POSITIVE가 있다면 지우고 SignType.POSITIVE, SignType.NEGATIVE 모두 앞으로 당김
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == SignType.POSITIVE)
                {
                    array[i] = SignType.NONE;
                    break;
                }
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == SignType.NONE)
                {
                    if (i + 1 < array.Length)//불안요소
                    {
                        array[i] = array[i + 1];
                        array[i + 1] = SignType.NONE;
                    }
                }
            }
        }

        if (Input.GetKey(saveManager.InputKeys[negativeValue]))
        {
            //SignType.NEGATIVE 저장되어 있지 않은 경우에만 SignType.NONE에 저장
            bool hasValue = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == SignType.NEGATIVE)
                {
                    hasValue = true;
                    break;
                }
            }
            if (!hasValue)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i] == SignType.NONE)
                    {
                        array[i] = SignType.NEGATIVE;
                        break;
                    }
                }
            }
        }
        else
        {//SignType.NEGATIVE 있다면 지우고 SignType.POSITIVE, SignType.NEGATIVE 모두 앞으로 당김
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == SignType.NEGATIVE)
                {
                    array[i] = SignType.NONE;
                    break;
                }
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == SignType.NONE)
                {
                    if (i + 1 < array.Length)
                    {
                        array[i] = array[i + 1];
                        array[i + 1] = SignType.NONE;
                    }
                }
            }
        }
        #endregion

        #region Select SignType
        SignType currentSignType = SignType.NONE;
        for (int i = array.Length - 1; 0 <= i; i--)
        {
            if (array[i] != SignType.NONE)
            {
                currentSignType = array[i];
                break;
            }
        }

        if (currentSignType == SignType.POSITIVE)
        {
            if (value < 0)
            {
                value = 0;
            }

            value += commonData.inputIncreaseSpeed * Time.deltaTime;
            return value >= 1 ? 1 : value;
        }
        else if (currentSignType == SignType.NEGATIVE)
        {
            if (value > 0)
            {
                value = 0;
            }

            value -= commonData.inputIncreaseSpeed * Time.deltaTime;
            return value <= -1 ? -1 : value;
        }
        #endregion

        #region No Input
        if (value < -0.1f)
        {
            value += commonData.inputIncreaseSpeed * Time.deltaTime;

            if (value > -0.1f)
            {
                return 0;
            }
            else
            {
                return value;
            }
        }

        if (value > 0.1f)
        {
            value -= commonData.inputIncreaseSpeed * Time.deltaTime;

            if (value < 0.1f)
            {
                return 0;
            }
            else
            {
                return value;
            }
        }

        return 0;
        #endregion
    }

    public void InputMovement() 
    {
        //방향키 입력
        if (PlatformSwitcher.instance.IsPCPlatform == false) //모바일
        {
            horizontalMove = joystick.Horizontal;
            verticalMove = joystick.Vertical;
        } 
        else //PC
        {
            horizontalMove = InputMovementValue(horizontalMove, true);
            verticalMove = InputMovementValue(verticalMove, false);
        }

        //recentering
        if (!PlatformSwitcher.instance.IsPCPlatform)
        {
            if (horizontalMove <= -0.5f || 0.5f <= horizontalMove)
            {
                CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_enabled = true;
            }
            else
            {
                CameraController.instance.DefaultCamera.m_RecenterToTargetHeading.m_enabled = false;
            }
        }
        

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalMove) + Mathf.Abs(verticalMove));

        movement = new Vector3(horizontalMove, 0, verticalMove);
        movement.Normalize();

        //반대방향이동시 입력값은 0인데 방향키가 움직이는 경우
        //if (horizontalMove == 0 && verticalMove == 0)
        //{
        //    if (Input.GetKey(saveManager.InputKeys[KeyAction.UP]) || 
        //        Input.GetKey(saveManager.InputKeys[KeyAction.DOWN]) ||
        //        Input.GetKey(saveManager.InputKeys[KeyAction.LEFT]) || 
        //        Input.GetKey(saveManager.InputKeys[KeyAction.RIGHT]))
        //    {
        //        moveAmount = .2f;
        //    }
        //}

        maskChange.CurrentAnimator.SetFloat("horizontal", horizontalMove);
        maskChange.CurrentAnimator.SetFloat("vertical", verticalMove);

        if (playerState.doNotMove)
        {
            maskChange.CurrentAnimator.SetFloat("moveAmount", 0);
        }
        else
        {
            maskChange.CurrentAnimator.SetFloat("moveAmount", moveAmount);
        }

        //targetRotation, movement 값을 한번에 처리한 다음 물리 적용
        if (moveAmount > 0f) //움직일 때 카메라 기준으로 방향 입력됨.
        {
            Vector3 cam = CameraController.instance.MainCamera.gameObject.transform.forward;
            movement = Quaternion.LookRotation(new Vector3(cam.x, 0, cam.z)) * movement;
            //movement를 Quaternion.LookRotation(new Vector3(cam.x, 0, cam.z)) 만큼 회전한 벡터 값

            //가끔 Look rotation viewing vector is zero 문구 날리면서 카메라 바라봄
            //TODO: Look rotation viewing vector is zero 해결하기
            targetRotation = Quaternion.LookRotation(movement);
        }

        //타겟락온시 회전값
        if (CameraController.instance.CurrentTarget)
        {
            Vector3 turnToTargetDirection = CameraController.instance.CurrentTarget.transform.position - maskChange.CurrentMask.transform.position;
            turnToTargetDirection.y = 0;
            targetRotation = Quaternion.LookRotation(turnToTargetDirection);
        }

        //사람탈, 동물탈 속도 변경
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            moveSpeed = PlayerHumanMaskData.Instance.moveSpeed;
        }
        else
        {
            moveSpeed = PlayerAnimalMaskData.Instance.moveSpeed;
        }
    }

    //참고: https://www.youtube.com/watch?v=4HpC--2iowE
    public void CharacterRotate() 
    {
        if (CameraController.instance.CurrentTarget)
        {
            maskChange.CurrentMask.transform.rotation 
                = Quaternion.Slerp(maskChange.CurrentMask.transform.rotation, targetRotation, .5f);
        }
        else
        {
            //목표(정면)를 향해 바라봄
            //문제가 있다면 특정이벤트로 다른 방향을 바라보고 있다가도 이벤트가 끝나면 targetRotation 방향으로 볼 수 있다는 것. 이벤트때 targetRotation을 직접 변경하는 방식이 필요할듯
            maskChange.CurrentMask.transform.rotation
                = Quaternion.RotateTowards(maskChange.CurrentMask.transform.rotation, targetRotation, commonData.playerRotateSpeed);
        }
    }
    public void CharacterMove()
    {
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            moveSpeed = PlayerHumanMaskData.Instance.moveSpeed;
            if (CheatMode.instance.isCheatMode)
            {
                if (CheatMode.instance.isMoveSpeedUp)
                {
                    moveSpeed = CheatData.Instance.moveSpeed;
                }
            }
        }
        else
        {
            moveSpeed = PlayerAnimalMaskData.Instance.moveSpeed;
            if (CheatMode.instance.isCheatMode)
            {
                if (CheatMode.instance.isMoveSpeedUp)
                {
                    moveSpeed = CheatData.Instance.moveSpeed;
                }
            }
        }

        if (playerSensor.canNotMoveforward)
        {
            moveSpeed = 0;
        }

        Vector3 flySpeed = new Vector3(0,0,0);
        if (CheatMode.instance.isFlying)
        {
            flySpeed = Vector3.up * CheatData.Instance.flySpeed * Time.deltaTime;
        }

        //물리시스템 충돌부분. 기존 움직임과 스킬 움직임 충돌 -> 어떻게 움직일지 모르는 상황
        //발생하는 이동량을 전부 더하는 공간을 만들어준다면 충돌이 일어나진 않을듯
        //움직임 애니에서만 작동하도록

        maskChange.CurrentRigidbody.MovePosition(maskChange.CurrentMask.transform.position + movement * moveSpeed * Time.deltaTime + flySpeed);
    }

    public void AddGravity()
    {
        maskChange.CurrentRigidbody.AddForce(new Vector3(0, PlayerCommonData.Instance.additionalGravity, 0));
    }

}
