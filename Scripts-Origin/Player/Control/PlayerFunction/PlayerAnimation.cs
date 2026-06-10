using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private MaskChange maskChange;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private HumanMaskSkill humanMaskSkill;
    [SerializeField] private AnimalMaskSkill animalMaskSkill;

    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerSound playerSound;

    private PlayerHumanMaskData humanData;
    private PlayerAnimalMaskData animalData;
    private PlayerGhostMaskData ghostData;
    private PlayerCommonData commonData;
                
    #region Animation Hash
    public readonly int Human_Movement = Animator.StringToHash("Human_Movement");
    public readonly int Human_LockOnMovement = Animator.StringToHash("Human_LockOnMovement");
    public readonly int Human_Hit = Animator.StringToHash("Human_Hit");
    public readonly int Human_Die = Animator.StringToHash("Human_Die");
    public readonly int Human_FirstNormalAttack = Animator.StringToHash("Human_NormalAttackFirst");
    public readonly int Human_SecondNormalAttack = Animator.StringToHash("Human_NormalAttackSecond");
    public readonly int Human_ThirdNormalAttack = Animator.StringToHash("Human_NormalAttackThird");
    public readonly int Human_InkShape = Animator.StringToHash("Human_InkShape");
    public readonly int Human_InkFloor = Animator.StringToHash("Human_InkFloor");
    public readonly int Human_FrontDash = Animator.StringToHash("Human_FrontDash");
    public readonly int Human_BackDash = Animator.StringToHash("Human_BackDash");

    public readonly int Animal_Movement = Animator.StringToHash("Animal_Movement");
    public readonly int Animal_LockOnMovement = Animator.StringToHash("Animal_LockOnMovement");
    public readonly int Animal_Hit = Animator.StringToHash("Animal_Hit");
    public readonly int Animal_Die = Animator.StringToHash("Animal_Die");
    public readonly int Animal_FirstNormalAttack = Animator.StringToHash("Animal_NormalAttackFirst");
    public readonly int Animal_SecondNormalAttack = Animator.StringToHash("Animal_NormalAttackSecond");
    public readonly int Animal_ThirdNormalAttack = Animator.StringToHash("Animal_NormalAttackThird");
    public readonly int Animal_LeapStrike = Animator.StringToHash("Animal_LeapStrike");
    public readonly int Animal_Roar = Animator.StringToHash("Animal_Roar");
    public readonly int Animal_FrontDash = Animator.StringToHash("Animal_FrontDash");
    public readonly int Animal_BackDash = Animator.StringToHash("Animal_BackDash");
    
    public readonly int Ghost_HumanHitGround = Animator.StringToHash("Ghost_HumanHitGround");
    public readonly int Ghost_HumanSwing = Animator.StringToHash("Ghost_HumanSwing");

    public readonly int Ghost_AnimalSweap = Animator.StringToHash("Ghost_AnimalSweap");
    public readonly int Ghost_AnimalSwing = Animator.StringToHash("Ghost_AnimalSwing");
    #endregion

    private void Start()
    {
        humanData = PlayerHumanMaskData.Instance;
        animalData = PlayerAnimalMaskData.Instance;
        ghostData = PlayerGhostMaskData.Instance;
        commonData = PlayerCommonData.Instance;

        #region 애니메이션 속도
        maskChange.HumanAnimator.SetFloat("rate_NormalAttackFirst", 1);
        maskChange.HumanAnimator.SetFloat("rate_NormalAttackSecond", 1);
        maskChange.HumanAnimator.SetFloat("rate_NormalAttackThird", 1);
        maskChange.HumanAnimator.SetFloat("rate_InkShape", 1);
        maskChange.HumanAnimator.SetFloat("rate_InkFloor", 1);
        maskChange.HumanAnimator.SetFloat("rate_BackDash", 1);
        maskChange.HumanAnimator.SetFloat("rate_FrontDash", 1);
        maskChange.HumanAnimator.SetFloat("rate_Hit", 1);
        maskChange.HumanAnimator.SetFloat("rate_Die", 1);

        //현재 동물탈은 안통하고있음
        maskChange.AnimalAnimator.SetFloat("rate_NormalAttackFirst", 1);
        maskChange.AnimalAnimator.SetFloat("rate_NormalAttackSecond", 1);
        maskChange.AnimalAnimator.SetFloat("rate_NormalAttackThird", 1);
        maskChange.AnimalAnimator.SetFloat("rate_LeapStrike", 1);
        maskChange.AnimalAnimator.SetFloat("rate_Roar", 1);
        maskChange.AnimalAnimator.SetFloat("rate_BackDash", 1);
        maskChange.AnimalAnimator.SetFloat("rate_FrontDash", 1);
        maskChange.AnimalAnimator.SetFloat("rate_Hit", 1);
        maskChange.AnimalAnimator.SetFloat("rate_Die", 1);

        maskChange.HumanAnimator.SetFloat("rate_GhostHumanHitGround", 1);
        maskChange.HumanAnimator.SetFloat("rate_GhostHumanSwing", 1);
        maskChange.AnimalAnimator.SetFloat("rate_GhostAnimalSweap", 1);
        maskChange.AnimalAnimator.SetFloat("rate_GhostAnimalSwing", 1);
        #endregion
    }
    public void UpdateAnimationState()
    {

        #region 상태 / 애니메이션 속도 제어
        var animatorStateInfo = maskChange.CurrentAnimator.GetCurrentAnimatorStateInfo(0);
        var normalizedTime = animatorStateInfo.normalizedTime;
        var animationHash = animatorStateInfo.shortNameHash;

        if (animationHash == Human_FirstNormalAttack)
        {
            SetAnimationSpeed(humanData.firstNormalAttackAnimationSpeed, normalizedTime, "rate_NormalAttackFirst");
        }
        else if (animationHash == Human_SecondNormalAttack)
        {
            SetAnimationSpeed(humanData.secondNormalAttackAnimationSpeed, normalizedTime, "rate_NormalAttackSecond");
        }
        else if (animationHash == Human_ThirdNormalAttack)
        {
            SetAnimationSpeed(humanData.thirdNormalAttackAnimationSpeed, normalizedTime, "rate_NormalAttackThird");
        }
        else if (animationHash == Human_InkShape)
        {
            SetAnimationSpeed(humanData.inkShapeAnimationSpeed, normalizedTime, "rate_InkShape");
        }
        else if (animationHash == Human_InkFloor)
        {
            SetAnimationSpeed(humanData.inkFloorAnimationSpeed, normalizedTime, "rate_InkFloor");
        }
        else if (animationHash == Human_FrontDash)
        {
            SetAnimationSpeed(commonData.frontDashAnimationSpeed, normalizedTime, "rate_FrontDash");
        }
        else if (animationHash == Human_BackDash)
        {
            SetAnimationSpeed(commonData.backDashAnimationSpeed, normalizedTime, "rate_BackDash");
        }
        else if (animationHash == Human_Hit)
        {
            SetAnimationSpeed(commonData.hitAnimationSpeed, normalizedTime, "rate_Hit");
        }
        else if (animationHash == Human_Die)
        {
            playerSound.StopLoopingAudio();
            SetAnimationSpeed(commonData.dieAnimationSpeed, normalizedTime, "rate_Die");
        }
            
        else if (animationHash == Animal_FirstNormalAttack)
        {
            SetAnimationSpeed(animalData.firstNormalAttackAnimationSpeed, normalizedTime, "rate_NormalAttackFirst");
        }
        else if (animationHash == Animal_SecondNormalAttack)
        {
            SetAnimationSpeed(animalData.secondNormalAttackAnimationSpeed, normalizedTime, "rate_NormalAttackSecond");
        }
        else if (animationHash == Animal_ThirdNormalAttack)
        {
            SetAnimationSpeed(animalData.thirdNormalAttackAnimationSpeed, normalizedTime, "rate_NormalAttackThird");
        }
        else if (animationHash == Animal_LeapStrike)
        {
            SetAnimationSpeed(animalData.leapStrikeAnimationSpeed, normalizedTime, "rate_LeapStrike");
        }
        else if (animationHash == Animal_Roar)
        {
            SetAnimationSpeed(animalData.roarAnimationSpeed, normalizedTime, "rate_Roar");
        }
        else if (animationHash == Animal_FrontDash)
        {
            SetAnimationSpeed(commonData.frontDashAnimationSpeed, normalizedTime, "rate_FrontDash");
        }
        else if (animationHash == Animal_BackDash)
        {
            SetAnimationSpeed(commonData.backDashAnimationSpeed, normalizedTime, "rate_BackDash");
        }
        else if (animationHash == Animal_Hit)
        {
            SetAnimationSpeed(commonData.hitAnimationSpeed, normalizedTime, "rate_Hit");
        }
        else if (animationHash == Animal_Die)
        {
            playerSound.StopLoopingAudio();
            SetAnimationSpeed(commonData.dieAnimationSpeed, normalizedTime, "rate_Die");
        }

        else if (animationHash == Ghost_HumanHitGround)
        {
            SetAnimationSpeed(ghostData.huamnHitGroundAnimationSpeed, normalizedTime, "rate_GhostHumanHitGround");
        }
        else if (animationHash == Ghost_HumanSwing)
        {
            SetAnimationSpeed(ghostData.humanSwingAnimationSpeed, normalizedTime, "rate_GhostHumanSwing");
        }
        else if (animationHash == Ghost_AnimalSweap)
        {
            SetAnimationSpeed(ghostData.animalSweapAnimationSpeed, normalizedTime, "rate_GhostAnimalSweap");
        }
        else if (animationHash == Ghost_AnimalSwing)
        {
            SetAnimationSpeed(ghostData.animalSwingAnimationSpeed, normalizedTime, "rate_GhostAnimalSwing");
        }


        //사람,동물 움직임 함수간소화하자
        else if (animationHash == Human_Movement || animationHash == Human_LockOnMovement)
        {
            if (!playerState.isPerfomingSklill)
            {
                if (maskChange.CurrentAnimator.GetFloat("moveAmount") >= 0.5f)
                {
                    //중복방지
                    if (playerState.playerCurrentState == PlayerStateType.WALK)
                    {
                        return;
                    }

                    playerSound.SetPlayerSound(commonData.humanRunSound, Player.instance.transform.position,Time.time);
                    playerState.ChangePlayerState(PlayerStateType.WALK);
                    playerState.ChangePlayerSubState(PlayerSubStateType.NONE);
                }

                else if (maskChange.CurrentAnimator.GetFloat("moveAmount") == 0)
                {
                    //중복방지
                    if (playerState.playerCurrentState == PlayerStateType.IDLE)
                    {
                        return;
                    }

                    playerSound.StopLoopingAudio();
                    playerState.ChangePlayerState(PlayerStateType.IDLE);
                    playerState.ChangePlayerSubState(PlayerSubStateType.NONE);
                }
            }
        }
        else if (animationHash == Animal_Movement || animationHash == Animal_LockOnMovement)
        {
            if (!playerState.isPerfomingSklill)
            {
                if (maskChange.CurrentAnimator.GetFloat("moveAmount") >= 0.5f)
                {
                    //중복방지
                    if (playerState.playerCurrentState == PlayerStateType.WALK)
                    {
                        return;
                    }

                    playerSound.SetPlayerSound(commonData.animalRunSound, Player.instance.transform.position, Time.time);
                    playerState.ChangePlayerState(PlayerStateType.WALK);
                    playerState.ChangePlayerSubState(PlayerSubStateType.NONE);
                }

                else if (maskChange.CurrentAnimator.GetFloat("moveAmount") == 0)
                {
                    //중복방지
                    if (playerState.playerCurrentState == PlayerStateType.IDLE)
                    {
                        return;
                    }

                    playerSound.StopLoopingAudio();
                    playerState.ChangePlayerState(PlayerStateType.IDLE);
                    playerState.ChangePlayerSubState(PlayerSubStateType.NONE);
                }
            }
        }
        #endregion
    }

    public void SetAnimationSpeed(animationSpeedStruct[] animationStruct, float normalizedTime, string speedRate)
    {
        for (int i = 0; i < animationStruct.Length; i++)
        {
            if ((animationStruct[i].startTime <= normalizedTime) && (normalizedTime < animationStruct[i].endTime))
            {
                maskChange.CurrentAnimator.SetFloat(speedRate, animationStruct[i].animationSpeed);
                break;
            }
        }
    }
}
