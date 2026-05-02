using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ---[Enum]---------------------------------------------------------------------------------------------------------------------------------
#endregion

#region SkillCooldown

public enum SkillCooldown
{
    INKSHAPE,
    //INKFLOOR,
    LEAPSTRIKE,
    //ROAR,
    FINISH,
    DASH
}
#endregion

#region MaskType
public enum MaskType
{
    HUMAN,
    ANIMAL,
    GHOST
}
#endregion

#region FunctionTarget
public enum FunctionTarget
{
    PLAYER,
    HUMAN_WEAPON,
    ANIMAL_LEFTHAND,
    ANIMAL_RIGHTHAND,
    ANIMAL_LEFTARM,
    ANIMAL_RIGHTARM,
    CURRENTENEMY,
}
#endregion

#region HitBox Type

public enum HitBoxType
{
    HUMAN_NORMALATTACK_FIRST,
    HUMAN_NORMALATTACK_SECOND,
    HUMAN_NORMALATTACK_THIRD,
    HUMAN_INKSHAPE,

    ANIMAL_NORMALATTACK_FIRST,
    ANIMAL_NORMALATTACK_SECOND,
    ANIMAL_NORMALATTACK_THIRD,
    ANIMAL_LEAPSTRIKE
}

#endregion

#region CameraType
//public enum CameraType
//{
//    PLAYER_DEFAULT,
//    HUMAN_FIRSTFINISH,
//    HUMAN_FINISH,
//    ANIMAL_FINISH,
//}
#endregion

#region CameraShakeType
public enum CameraShakeType
{
    IMPULSE_RECOIL,
    IMPULSE_BUMP,
    IMPULSE_EXPOLOSION,
    IMPULSE_RUMBLE
}
#endregion

#region CameraReactionType
public enum CameraReactionType
{
    NOISE_6DSHAKE,
    NOISE_6DWOBBLE,
    HANDHELD_NORMAL_EXTREME,
    HANDHELD_NORMAL_MILD,
    HANDHELD_NORMAL_STRONG,
    HANDHELD_TELE_MILD,
    HANDHELD_TELE_STRONG,
    HANDHELD_WIDEANGLE_MILD,
    HANDHELD_WIDEANGLE_STRONG,
}
#endregion

#region TimeScaleApplyTarget
public enum TimeScaleApplyTarget
{
    GAME,
    VFX
}
#endregion

#region PlayerStateType
public enum PlayerStateType
{
    NONE,
    IDLE,
    WALK,
    HUMAN_NORMALATTACK,
    HUMAN_INKSHAPE,
    HUMAN_INKFLOOR,
    ANIMAL_NORMALATTACK,
    ANIMAL_LEAPSTRIKE,
    ANIMAL_ROAR,
    GHOST_FINISHSKILL,
    DASH,
    HIT,
    DEAD
}
#endregion

#region PlayerSubStateType
public enum PlayerSubStateType
{
    NONE,
    WALK_GROUND,
    WALK_WATER,
    DEAD_HPZERO,
    DEAD_FALL,
    HIT_DEFAULT,

    //사람탈
    HUMAN_FIRSTNORMALATTACK,
    HUMAN_SECONDNORMALATTACK,
    HUMAN_THIRDNORMALATTACK,

    //동물탈
    ANIMAL_FIRSTNORMALATTACK,
    ANIMAL_SECONDNORMALATTACK,
    ANIMAL_THIRDNORMALATTACK,
}
#endregion

#region RestrictionType
public enum PlayerRestrictionType
{
    ACT,
    MOVE,
    ROTATE
}
#endregion

#region MoveDirection
public enum MoveDirection
{
    FRONT,
    BACK,
    UP,
    DOWN,
    //LEFT,
    //RIGHT,
}
#endregion

#region ---[Struct]------------------------------------------------------------------------------------------------------------------------------------
#endregion

#region PlayerBasicStat
[System.Serializable]
public struct PlayerBasicStatStruct
{
    [Header("스킬 스탯")]
    public float damage;
    public float cooldown;
    public float inkStack;
}


#endregion

#region FunctionStop
[System.Serializable]
public struct FunctionStopStruct
{
    public float waitTime;
    public float duration;
}
#endregion

#region Restrict
[System.Serializable]
public struct RestrictStruct
{
    [Header("스킬 행동 제한")]

    public float actRestrictWaitTime;
    public float actRestrictDuration;

    public float moveRestrictWaitTime;
    public float moveRestrictDuration;

    public float rotateRestrictWaitTime;
    public float rotateRestrictDuration;
}
#endregion

#region animationSpeed
[System.Serializable]
public struct animationSpeedStruct
{
    [Range(0, 1)] public float startTime;
    [Range(0, 1)] public float endTime;
    public float animationSpeed;
    public animationSpeedStruct(float startTime = 0, float endTime = 1, float animationSpeed = 1)
    {
        this.startTime = 0;
        this.endTime = 1;
        this.animationSpeed = 1;
    }
}
#endregion

#region SkillMove
[System.Serializable]
public struct SkillMoveStruct
{
    public MoveDirection direction;
    public float moveSpeed;
    public float waitTime;
    public float duration;
}

#endregion

#region TimeScale
[System.Serializable]
public struct TimeScaleStruct
{
    public bool useFunction;
    [Range(0, 1)] public float timeScale;
    
    [Header("useFrame 체크: 단위를 프레임으로 진행, frame 값만 설정하기")]
    public bool useFrame;
    public float waitTimeFrames;
    public float durationFrames;

    [Space(10)]
    public float waitTimeSeconds;
    public float durationSeconds;

    public void Initialize(float tixxxmeScale, float waitTime, float duration)
    {

    }
}
#endregion

#region PlayerEffect
[System.Serializable]
public struct EffectStruct
{
    public bool useFunction;
    public float waitTime;
    public float duration;

    [Header("untilFinish: 이펙트 끝날때까지 계속 \nfollowPosition: 포지션을 계속 따라다님")]
    public bool untilFinish;

    [Space(10)]
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public bool followPosition;
    public float followWaitTime;
    public float followDuration;

    //public void Initialize()
}
#endregion

#region PlayerHitBox
[System.Serializable]
public struct HitBoxStruct
{
    public bool useFunction;
    public bool showHitbox;
    public bool untilFinish;
    //public bool followPosition;

    [Space(10)]
    public float waitTime;
    public float duration;

    [Space(10)]
    public HitBoxType hitBoxType;

    //[Space(10)]
    //public int hitCount;
    //public float hitInterval;

    //public void Initialize()
}
#endregion

#region PlayerSound
[System.Serializable]
public struct SoundStruct
{
    //"오디오 랜텀 컨테이너" 에셋있긴함
    public bool useFunction;
    public AudioClip[] audioClip;
    public bool loop;
    public float volume;
    public float pitch;
    public float waitTime;

    [Range(0, 1)] public float spatialBlend;
    public float MinDistance;
    public float MaxDistance;

    public bool untilFinish;

    //public void Initialize(AudioClip[] audioClip, bool loop, float volume, float pitch, float waitTime)
    //{
    //    this.audioClip = audioClip;
    //    this.loop = loop;
    //    this.volume = volume;
    //    this.pitch = pitch;
    //    this.waitTime = waitTime;
    //}
}

#endregion

#region CameraShake
[System.Serializable]
public struct CameraShakeStruct
{
    public bool useFunction;

    [Tooltip("cameraType: 기능이 적용되는 카메라")]
    public CameraType cameraType;
    [Tooltip("waitTime: waitTime 이후에 기능 시작")]
    public float waitTime;
    
    [Space(10)]
    [Tooltip("shakeType: 카메라 쉐이크 종류")]
    public CameraShakeType shakeType;
    [Tooltip("imulseVelocty: 쉐이크 속도")]
    public Vector3 impulseVelocty;
    [Tooltip("impulseDuration: 쉐이크 시간")]
    public float impulseDuration;
    
    [Space(10)]
    [Header("reaction은 쉐이크에 카메라가 반응하는 걸 말합니다")]
    [Tooltip("reactionType: 쉐이크에 반응하는 타입")]
    public CameraReactionType reactionType;
    //public float gain;
    [Tooltip("amplitudeGain: 값을 높이면 카메라 흔들림의 강도가 증가")]
    public float amplitudeGain;
    [Tooltip("frequencyGain: 값을 높이면 카메라 흔들림의 속도가 증가")]
    public float frequencyGain;
    [Tooltip("reactionDuration: 반응하는 시간")]
    public float reactionDuration; //카메라 기능 내에 duration이 있음

    //public void Initialize(float cameraShakeStrength, float cameraShakeWaitTime, float cameraShakeDuration)
}
#endregion

#region CameraRecomposer
[System.Serializable]
public struct CameraRecomposerStruct
{
    public bool useFunction;
    [Tooltip("waitTime: waitTime 이후에 기능 시작")]
    public float waitTime;
    [Tooltip("duration: waitTime이 끝나고 duratio 동안 기능 실행")]
    public float duration;
    [Tooltip("줌하는 정도")]
    public float zoomScale;
    [Tooltip("카메라가 타겟을 따라가는 정도")]
    public float followAttachment;
    [Tooltip("카메라가 타겟을 바라보는 정도")]
    public float lookAtAttachment;
}
#endregion

#region CameraZoom
[System.Serializable]
public struct CameraZoomStruct
{
    [Header("isZoomInFirst 체크: Zoom In 먼저시작")]
    public float isZoomInFirst;
    public float waitTime;
    public float firstDuration;
    public float secondDuration;

    [Header("카메라 타겟 설정하고 positionOffset으로 세부조정")]
    public FunctionTarget cameraTarget;
    public float positionOffset;

    public void Initialize(float isZoomInFirst, float waitTime, float firstDuration, float secondDuration, FunctionTarget cameraTarget, float positionOffset)
    {
        this.isZoomInFirst = isZoomInFirst;
        this.waitTime = waitTime;
        this.firstDuration = firstDuration;
        this.secondDuration = secondDuration;

        this.cameraTarget = cameraTarget;
        this.positionOffset = positionOffset;
    }
}
#endregion

#region CameraPosition
[System.Serializable]
public struct CameraPositionOffeset
{
    public float cameraZoomWaitTime;
    public float cameraZoomDuration;
    public GameObject cmaeraZoomTarget;

    public void Initialize(float cameraZoomWaitTime, float cameraZoomDuration, GameObject cmaeraZoomTarget)
    {
        this.cameraZoomWaitTime = cameraZoomWaitTime;
        this.cameraZoomDuration = cameraZoomDuration;
        this.cmaeraZoomTarget = cmaeraZoomTarget;
    }
}
#endregion

#region PlayerSkillInput
[System.Serializable]
public struct PlayerSkillInputStruct
{
    public float storeWaitTime;
    public float storeDuration;
    public float executeWaitTime;
    public float executeDuration;
}
#endregion

#region PostProcessing-Vignette
[System.Serializable]
public struct VignetteStruct
{
    public bool useFunction;
    public Color color;
    public Vector2 center;
    [Range(0, 1)] public float intensity;
    [Range(0,1)] public float smoothness;
    public bool rounded;

    //반복해서 사용할 것인가
    public bool isLooping;
    //몇회 반복할 것인가
    public int count;
    //한 주기 커브
    public AnimationCurve oneTimeCurve;
    //지연시간
    public float waitTime;
    //1회 수행시간
    public float oneTimeDuration;
}


#endregion