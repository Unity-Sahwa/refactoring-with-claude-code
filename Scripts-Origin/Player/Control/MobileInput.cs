using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobileInput : MonoBehaviour
{
    public static MobileInput instance;

    [SerializeField] private MaskChange maskChange;

    [SerializeField] private HumanMaskSkill humanMaskSkill;
    [SerializeField] private AnimalMaskSkill animalMaskSkill;
    [SerializeField] private GhostMaskSkill ghostMaskSkill;

    [SerializeField] private CameraController cameraController;
    
    [SerializeField] private MenuUI menuUI;

    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerSkillInput playerSkillInput;

    public bool interact {  get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
    }

    public void NormalAttack()
    {
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            playerSkillInput.StoreInput(PlayerStateType.HUMAN_NORMALATTACK);
            if (!playerState.isPerfomingSklill) humanMaskSkill.NormalAttack();
        }
        else
        {
            playerSkillInput.StoreInput(PlayerStateType.ANIMAL_NORMALATTACK);
            if (!playerState.isPerfomingSklill) animalMaskSkill.NormalAttack();
        }
    }

    public void SpecialAttack()
    {
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            playerSkillInput.StoreInput(PlayerStateType.HUMAN_INKSHAPE);
            if (!playerState.isPerfomingSklill) humanMaskSkill.InkShape();
        }
        else
        {
            playerSkillInput.StoreInput(PlayerStateType.ANIMAL_LEAPSTRIKE);
            if (!playerState.isPerfomingSklill) animalMaskSkill.LeapStrike();
        }
    }
    public void FinishAttack()
    {
        playerSkillInput.StoreInput(PlayerStateType.GHOST_FINISHSKILL);
        if (!playerState.isPerfomingSklill) ghostMaskSkill.Finish();
    }

    public void Dash()
    {
        if (maskChange.CurrentMask == maskChange.HumanMask)
        {
            playerSkillInput.StoreInput(PlayerStateType.DASH);
            if (!playerState.isPerfomingSklill) humanMaskSkill.Dash();
        }
        else
        {
            playerSkillInput.StoreInput(PlayerStateType.DASH);
            if (!playerState.isPerfomingSklill) animalMaskSkill.Dash();
        }
    }

    public void LockOnTarget()
    {
        cameraController.LockOnTarget();
    }

    public void OpenMenu()
    {
        menuUI.MenuSwitch();
    }
    public void EnableInteraction()
    {
        interact = true;
    }

    public void DisableInteraction()
    {
        interact = false;
    }
}
