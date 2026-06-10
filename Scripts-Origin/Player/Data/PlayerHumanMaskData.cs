using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public partial class PlayerHumanMaskData: ScriptableObject
{
    [Tooltip("도화탈 이동속도")]
    public float moveSpeed = 5;
}

#region NormalAttack_First
public partial class PlayerHumanMaskData : ScriptableObject
{
    [Space(10f)]
    [Header("평타 스텟")]
    public PlayerBasicStatStruct normalAttactStat;
    
    [Space(20f)]
    [Header("평타 1")]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct firstNormalAttackInput;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct firstNormalAttackStop;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] firstNormalAttackMove;
    [Tooltip("행동 제한")]
    public RestrictStruct firstNormalAttackRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] firstNormalAttackAnimationSpeed;
    [Tooltip("스킬 이펙트")]
    public EffectStruct firstNormalAttackSkillEffect;
    [Tooltip("스킬 히트박스")]
    public HitBoxStruct firstNormalAttackHitBox;
    [Space(10f)]
    [Tooltip("스킬 사운드")]
    public SoundStruct firstNormalAttackSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct firstNormalAttackCameraShake;
    

    [Space(10f)]
    [Header("평타 1 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct firstNormalAttackHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct firstNormalAttackHitSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct firstNormalAttackHitCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct firstNormalAttackHitTimeScale;
}

#endregion

#region NormalAttack_Second
public partial class PlayerHumanMaskData : ScriptableObject
{
    [Space(20f)]
    [Header("평타 2")]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct secondNormalAttackInput;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct secondNormalAttackStop;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] secondNormalAttackMove;
    [Tooltip("행동 제한")]
    public RestrictStruct secondNormalAttackRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] secondNormalAttackAnimationSpeed;
    [Tooltip("스킬 이펙트")]
    public EffectStruct secondNormalAttackSkillEffect;
    [Tooltip("스킬 히트박스")]
    public HitBoxStruct secondNormalAttackHitBox;
    [Tooltip("스킬 사운드")]
    public SoundStruct secondNormalAttackSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct secondNormalAttackCameraShake;

    [Space(10f)]
    [Header("평타 2 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct secondNormalAttackHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct secondNormalAttackHitSound;
    [Tooltip("카메라 히트 쉐이크")]
    public CameraShakeStruct secondNormalAttackHitCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct secondNormalAttackHitTimeScale;
}

#endregion

#region NormalAttack_Third
public partial class PlayerHumanMaskData : ScriptableObject
{
    [Space(20f)]
    [Header("평타 3")]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct thirdNormalAttackInput;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct thirdNormalAttackStop;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] thirdNormalAttackMove;
    [Tooltip("제한")]
    public RestrictStruct thirdNormalAttackRestrict;
    [Tooltip("평타3 애니메이션 속도")]
    public animationSpeedStruct[] thirdNormalAttackAnimationSpeed;
    [Tooltip("스킬 이펙트")]
    public EffectStruct thirdNormalAttackSkillEffect;
    [Tooltip("스킬 히트박스")]
    public HitBoxStruct thirdNormalAttackHitBox;
    [Tooltip("스킬 사운드")]
    public SoundStruct thirdNormalAttackSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct thirdNormalAttackCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct thirdNormalAttackGameTimeScale;

    [Space(10f)]
    [Header("평타 3 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct thirdNormalAttackHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct thirdNormalAttackHitSound;
    [Tooltip("카메라 히트 쉐이크")]
    public CameraShakeStruct thirdNormalAttackHitCameraShake;
    [Tooltip("히트스톱")]
    public TimeScaleStruct thirdNormalAttackHitTimeScale;
}
#endregion

#region InkShape
public partial class PlayerHumanMaskData : ScriptableObject
{
    [Space(20f)]
    [Header("A스킬")]
    [Tooltip("A스킬 기본")]
    public PlayerBasicStatStruct inkShapeStat;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct inkShapeStop;
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct inkShapeInput;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] inkShapeMove;
    [Tooltip("행동 제한")]
    public RestrictStruct inkShapeRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] inkShapeAnimationSpeed;
    [Tooltip("스핀 트레일 이펙트")]
    public EffectStruct inkShapeSpinTrailEffect;
    [Tooltip("스플레쉬 이펙트")]
    public EffectStruct inkShapeSplashEffect;
    [Space(10f)]
    [Tooltip("히트박스 지연시간")]
    public float inkShapeHitBoxWaitTime = 1f;
    [Tooltip("히트박스 지속시간")]
    public float inkShapeHitBoxDuration = 0;
    [Tooltip("히트박스 타격 횟수")]
    public int inkShapeHitCount = 2;
    [Tooltip("히트박스 타격 간격")]
    public float inkShapeHitInterval = .4f;
    [Space(10f)]
    [Tooltip("스핀 사운드")]
    public SoundStruct inkShapeSpinSound;
    [Tooltip("스플래쉬 사운드")]
    public SoundStruct inkShapeSplashSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct inkShapeCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct inkShapeGameTimeScale;

    [Space(10f)]
    [Header("A스킬 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct inkShapeHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct inkShapeHitSound;
    [Tooltip("카메라 히트 쉐이크")]
    public CameraShakeStruct inkShapeHitCameraShake;
    [Tooltip("히트스탑")]
    public TimeScaleStruct inkShapeHitTimeScale;
}
#endregion

#region InkFloor
public partial class PlayerHumanMaskData : ScriptableObject
{
    [Space(20f)]
    [Header("S스킬")]
    [Tooltip("S스킬 기본")] 
    public PlayerBasicStatStruct inkFloorStat;
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct inkFloorInput;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct inkFloorStop;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] inkFloorMove;
    [Tooltip("행동 제한")]
    public RestrictStruct inkFloorRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] inkFloorAnimationSpeed;
    [Tooltip("발사체 이펙트")]
    public EffectStruct inkFloorProjectileEffect;
    [Space(10f)]
    [Tooltip("히트박스 지연시간")]
    public float inkFloorHitBoxWaitTime = .6f;
    [Tooltip("히트박스 지속시간")]
    public float inkFloorHitBoxDuration = 3;
    [Tooltip("히트박스 타격 횟수")]
    public int inkFloorHitCount = 5;
    [Tooltip("히트박스 타격 간격")]
    public float inkFloorHitInterval = .4f;
    [Tooltip("히트박스 영역의 크기 ")]
    public Vector3 inkFloorScale = new Vector3(2, 2, 2);
    [Space(10f)]
    [Tooltip("스윙 사운드")]
    public SoundStruct inkFloorSwingSound;
    [Tooltip("발사체 사운드")]
    public SoundStruct inkFloorProjectileSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct inkFloorCameraShake;
    [Tooltip("타임 스케일")]
    public TimeScaleStruct inkFloorGameTimeScale;

    [Space(10f)]
    [Header("S스킬 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct inkFloorHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct inkFloorHitSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct inkFloorHitCameraShake;

}
#endregion

#region Set
public partial class PlayerHumanMaskData : ScriptableObject
{
    #region 싱글톤
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/PlayerHumanMaskData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static PlayerHumanMaskData instance;
    public static PlayerHumanMaskData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<PlayerHumanMaskData>("PlayerHumanMaskData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<PlayerHumanMaskData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<PlayerHumanMaskData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
                    AssetDatabase.CreateAsset(instance, SettingFilePath); //방금 생성된 오브젝트를 유니티에셋으로 생성, 저장할 수 있게
                }

            }
#endif
            return instance;
        }
    }
    #endregion

    
}
#endregion

#region 스크립터블 오브젝트 업그레이드
// 어디 저장버튼없나? 저장버튼 누르면 수정되고있는값 다 디폴트값으로 저장시키는

//  https://discord.com/channels/1181871963170418688/1292704374941810780

//추가 질문: struct으로 만드는데 구성원 중에 원하는 것만 골라서 구성할 수는 없나?

//void Reset() 
//{
//    //리셋저장용
//    //리셋버튼으로 되는거 확인함
//    //animSpeed_NormalAttackThird[0].startTime = 0;
//    //    animSpeed_NormalAttackThird[0].endTime = 1;
//    //    animSpeed_NormalAttackThird[0].animationSpeed = 1.5f;


//    //    animSpeed_InkShape[0].startTime = 0;
//    //    animSpeed_InkShape[0].endTime = 0.05f;
//    //    animSpeed_InkShape[0].animationSpeed = 0.5f;

//    //    animSpeed_InkShape[0].startTime = 0.05f;
//    //    animSpeed_InkShape[0].endTime = 0.3f;
//    //    animSpeed_InkShape[0].animationSpeed = 1.5f;

//    //    animSpeed_InkShape[0].startTime = 0.3f;
//    //    animSpeed_InkShape[0].endTime = 0.5f;
//    //    animSpeed_InkShape[0].animationSpeed = 1f;


//    //    skill[0].restrictBehavior[0].waitTime = 0;
//} ===================================================

//[System.Serializable] 이중 struct
//public struct Skill
//{
//    public float outerfloat;
//    public int outertime;


//    public function[] restrictBehavior;

//    [System.Serializable]
//    public struct function
//    {
//        public float waitTime;
//        public int duration;
//    }
//} 

//public Skill[] skill; =====================================

//[System.Serializable]
//public struct www
//{
//    [Range(0, 1)] public float startTime;
//    [Range(0, 1)] public float endTime;
//    public float animationSpeed;
//}

//public www wwww;

//// 기본값을 상수로 정의
//private const float DEFAULT_START_TIME = 0f;
//private const float DEFAULT_END_TIME = .2f;
//private const float DEFAULT_ANIMATION_SPEED = 4f;

//private void Reset()
//{
//    // Reset 함수에서 구조체의 값들을 기본값으로 초기화
//    wwww = new www
//    {
//        startTime = DEFAULT_START_TIME,
//        endTime = DEFAULT_END_TIME,
//        animationSpeed = DEFAULT_ANIMATION_SPEED
//    };

//    // 값이 변경되었음을 Unity에 알림
//    // 없어도 작동은 하는데 필요성을 알아봐야할듯
//    //UnityEditor.EditorUtility.SetDirty(this);
//}
#endregion
