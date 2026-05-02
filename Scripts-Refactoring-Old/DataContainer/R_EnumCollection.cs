using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputActionEnum
{
    Up,
    Down,
    Left,
    Right,

    Jump,

    NormalAttack,
    SpecialAttack,
    FinishAttack,

    Dash,

    LockOnTarget,
    Interact,

    Menu,

    Count
}
public enum InputStateEnum
{
    Down,
    Up
}
public enum PlayerStateEnum
{
    Move,
    
    Jump,
    Fall,
    Land,
    Dash,

    NormalAttack,
    HNormalAttack3,
    ANormalAttack1,
    ANormalAttack2,
    ANormalAttack3,

    HSpecialAttack,
    ASpecialAttack,

    HFinishAttack,
    AFinishAttack,
    
    Damaged,
    Die,
    Count
}
public enum PlayerInputHandleEnum
{
    EnableInput,
    EnableSaveInput,
    EnableAct,
    EnableMove,
    EnableRotate
}
public enum PlayerComponentEnum
{
    PlayerBasicMove,
    SkillMove,
    AnimalAnimation,
    HumanAnimation
}
public enum PlayerMoveActionEnum
{
    Move,
    Rotate,
    Impulse
}
public enum DataPropertyNameEnum
{
    //Data ScriptableObject의 프로퍼티 이름과 동일해야함
    EnvSoundSliderValue,
    EnvSoundLabelText
}
public enum PopupTypeEnum
{

}