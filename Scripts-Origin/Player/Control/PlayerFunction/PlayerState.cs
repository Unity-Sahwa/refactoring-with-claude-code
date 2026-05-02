using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    #region 외부
    [SerializeField] private PlayerMovement playerMovement;
    #endregion

    #region 행동 제한

    public bool doNotAct {  get; private set; }
    public bool doNotMove { get; private set; }
    public bool doNotRotate { get; private set; }
    
    private bool stopRestrictCoroutine;
    #endregion

    #region 플레이어 상태
    [field: SerializeField]
    public PlayerStateType playerCurrentState { get; private set; }
    [field: SerializeField]
    public PlayerSubStateType playerCurrentSubState { get; private set; }
    [field: SerializeField]
    public bool isPerfomingSklill { get; private set; }
    #endregion

    #region 피격 판정 관련
    public bool isSuperArmor { get; private set; }
    public bool isInvincible { get; private set; }
    #endregion

    private void Start()
    {
        isPerfomingSklill = false;

        doNotAct = false;
        doNotMove = false;
        doNotRotate = false;
        stopRestrictCoroutine = true;

        isInvincible = false;
        isSuperArmor = false;
    }
    public void Initialize()
    {
        stopRestrictCoroutine = true;

        playerCurrentState = PlayerStateType.IDLE;
        playerCurrentSubState = PlayerSubStateType.NONE;
        isPerfomingSklill = false;

        ToggleSuperArmorState(false);
        ToggleInvincibleState(false);

        doNotAct = false;
        doNotMove = false;
        doNotRotate = false;
    }

    #region 플레이어 상태 변경
    public void ChangePlayerState(PlayerStateType state)
    {
        this.playerCurrentState = state;
        IsPerformingSKill();
    }
    public void ChangePlayerSubState(PlayerSubStateType subState)
    {
        this.playerCurrentSubState = subState;
        IsPerformingSKill();
    }

    //스킬 사용중인지
    public void IsPerformingSKill()
    {
        isPerfomingSklill = playerCurrentState switch
        {
            PlayerStateType.NONE => false,
            PlayerStateType.IDLE => false,
            PlayerStateType.WALK => false,
            PlayerStateType.HIT => false,
            PlayerStateType.DEAD => false,
            _ => true
        };
    }

    #endregion

    #region 플레이어 피격 판정 관련
    //슈퍼아머
    public void ToggleSuperArmorState(bool toggleSwitch)
    {
        isSuperArmor = toggleSwitch;
    }

    //무적
    public void ToggleInvincibleState(bool toggleSwitch)
    {
        isInvincible = toggleSwitch;
    }
    #endregion

    #region 제한
    public void RestrictPlayer(RestrictStruct restrictStruct, float skillStartTime)
    {
        if (restrictStruct.actRestrictDuration != 0)
        {
            if (Time.time >= skillStartTime + restrictStruct.actRestrictWaitTime + restrictStruct.actRestrictDuration)
            {
                doNotAct = false;

            }
            else if (Time.time >= skillStartTime + restrictStruct.actRestrictWaitTime)
            {
                doNotAct = true;
            }
        }

        if (restrictStruct.moveRestrictDuration != 0)
        {
            if (Time.time >= skillStartTime + restrictStruct.moveRestrictWaitTime + restrictStruct.moveRestrictDuration)
            {
                doNotMove = false;
            }
            else if (Time.time >= skillStartTime + restrictStruct.moveRestrictWaitTime)
            {
                doNotMove = true;
            }
        }

        if (restrictStruct.rotateRestrictDuration != 0)
        {
            if (Time.time >= skillStartTime + restrictStruct.rotateRestrictWaitTime + restrictStruct.rotateRestrictDuration)
            {
                doNotRotate = false;
            }
            else if (Time.time >= skillStartTime + restrictStruct.rotateRestrictWaitTime)
            {
                doNotRotate = true;
            }
        }
    }

    public void RestrictPlayer(PlayerRestrictionType restrictionType, bool doRestrict)
    {
        switch(restrictionType)
        {
            case PlayerRestrictionType.ACT:
                doNotAct = doRestrict;
                break;

            case PlayerRestrictionType.MOVE:
                doNotMove = doRestrict;
                break;

            case PlayerRestrictionType.ROTATE:
                doNotRotate = doRestrict;
                break;
        }
    }

    #endregion
}
