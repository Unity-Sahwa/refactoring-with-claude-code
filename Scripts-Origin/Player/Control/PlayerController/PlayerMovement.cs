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
        #region �̱���
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
        //����: negative ��ư�� ��Ӵ����� ���� ��, positive ��ư�� ������ �ٷ� ��ȯ ����. ������ positive ���� ���¿��� negative ������ X (if�� ��������)

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
            //SignType.POSITIVE�� ����Ǿ� ���� ���� ��쿡�� SignType.NONE�� ����
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
        {//SignType.POSITIVE�� �ִٸ� ����� SignType.POSITIVE, SignType.NEGATIVE ��� ������ ���
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
                    if (i + 1 < array.Length)//�Ҿȿ��
                    {
                        array[i] = array[i + 1];
                        array[i + 1] = SignType.NONE;
                    }
                }
            }
        }

        if (Input.GetKey(saveManager.InputKeys[negativeValue]))
        {
            //SignType.NEGATIVE ����Ǿ� ���� ���� ��쿡�� SignType.NONE�� ����
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
        {//SignType.NEGATIVE �ִٸ� ����� SignType.POSITIVE, SignType.NEGATIVE ��� ������ ���
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
        //����Ű �Է�
        if (PlatformSwitcher.instance.IsPCPlatform == false) //�����
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

        //targetRotation, movement ���� �ѹ��� ó���� ���� ���� ����
        if (moveAmount > 0f) //������ �� ī�޶� �������� ���� �Էµ�.
        {
            Vector3 cam = CameraController.instance.MainCamera.gameObject.transform.forward;
            movement = Quaternion.LookRotation(new Vector3(cam.x, 0, cam.z)) * movement;
            //movement�� Quaternion.LookRotation(new Vector3(cam.x, 0, cam.z)) ��ŭ ȸ���� ���� ��

            //���� Look rotation viewing vector is zero ���� �����鼭 ī�޶� �ٶ�
            //TODO: Look rotation viewing vector is zero �ذ��ϱ�
            targetRotation = Quaternion.LookRotation(movement);
        }

        //Ÿ�ٶ��½� ȸ����
        if (CameraController.instance.CurrentTarget)
        {
            Vector3 turnToTargetDirection = CameraController.instance.CurrentTarget.transform.position - maskChange.CurrentMask.transform.position;
            turnToTargetDirection.y = 0;
            targetRotation = Quaternion.LookRotation(turnToTargetDirection);
        }

        //���Ż, ����Ż �ӵ� ����
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            moveSpeed = PlayerHumanMaskData.Instance.moveSpeed;
        }
        else
        {
            moveSpeed = PlayerAnimalMaskData.Instance.moveSpeed;
        }
    }

    //����: https://www.youtube.com/watch?v=4HpC--2iowE
    public void CharacterRotate() 
    {
        if (CameraController.instance.CurrentTarget)
        {
            maskChange.CurrentMask.transform.rotation 
                = Quaternion.Slerp(maskChange.CurrentMask.transform.rotation, targetRotation, .5f);
        }
        else
        {
            //��ǥ(����)�� ���� �ٶ�
            //������ �ִٸ� Ư���̺�Ʈ�� �ٸ� ������ �ٶ󺸰� �ִٰ��� �̺�Ʈ�� ������ targetRotation �������� �� �� �ִٴ� ��. �̺�Ʈ�� targetRotation�� ���� �����ϴ� ����� �ʿ��ҵ�
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

        //�����ý��� �浹�κ�. ���� �����Ӱ� ��ų ������ �浹 -> ��� �������� �𸣴� ��Ȳ
        //�߻��ϴ� �̵����� ���� ���ϴ� ������ ������شٸ� �浹�� �Ͼ�� ������
        //������ �ִϿ����� �۵��ϵ���

        maskChange.CurrentRigidbody.MovePosition(maskChange.CurrentMask.transform.position + movement * moveSpeed * Time.deltaTime + flySpeed);
    }

    public void AddGravity()
    {
        maskChange.CurrentRigidbody.AddForce(new Vector3(0, PlayerCommonData.Instance.additionalGravity, 0));
    }

}
