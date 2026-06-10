using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public partial class PlayerAnimalMaskData : ScriptableObject
{
    [Tooltip("산곡탈 이동속도")]
    public float moveSpeed = 8; 
}

#region NormalAttack_First
public partial class PlayerAnimalMaskData : ScriptableObject
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
    
    [Space(10f)]
    [Tooltip("무기형상 지연시간")]
    public float firstNormalAttackWeaponWaitTime = 1f;
    [Tooltip("무기형상 지속시간")]
    public float firstNormalAttackWeaponDuration = 0;
    [Space(10f)]
    [Tooltip("스킬 히트박스")]
    public HitBoxStruct firstNormalAttackHitBox;
    [Space(10f)]
    [Tooltip("스킬 사운드")]
    public SoundStruct firstNormalAttackSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct firstNormalAttackCameraShake;
    
    [Space(10f)]
    [Header("평타 1 - 타격판정")]
    [Tooltip("이펙트")]
    public EffectStruct firstNormalAttackHitEffect;
    [Tooltip("사운드")]
    public SoundStruct firstNormalAttackHitSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct firstNormalAttackHitCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct firstNormalAttackHitTimeScale;
}
#endregion

#region NormalAttack_Second
public partial class PlayerAnimalMaskData : ScriptableObject
{
    [Space(20f)]
    [Header("평타 2")]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct secondNormalAttackInput;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] secondNormalAttackMove;
    [Tooltip("행동 제한")]
    public RestrictStruct secondNormalAttackRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] secondNormalAttackAnimationSpeed;
    [Tooltip("스킬 이펙트")]
    public EffectStruct secondNormalAttackSkillEffect;

    [Space(10f)]
    [Tooltip("무기형상 지연시간")]
    public float secondNormalAttackWeaponWaitTime = 1f;
    [Tooltip("무기형상 지속시간")]
    public float secondNormalAttackWeaponDuration = 0;
    [Space(10f)]
    [Tooltip("스킬 히트박스")]
    public HitBoxStruct secondNormalAttackHitBox;
    [Space(10f)]
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
public partial class PlayerAnimalMaskData : ScriptableObject
{
    [Space(20f)]
    [Header("평타 3")]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct thirdNormalAttackInput;
    
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] thirdNormalAttackMove;
    [Tooltip("행동 제한")]
    public RestrictStruct thirdNormalAttackRestrict;
    [Tooltip("평타3 애니메이션 속도")]
    public animationSpeedStruct[] thirdNormalAttackAnimationSpeed;
    [Tooltip("스킬 이펙트")]
    public EffectStruct thirdNormalAttackSkillEffect;
    [Tooltip("트레일 이펙트")]
    public EffectStruct thirdNormalAttackTrailEffect;
    [Space(10f)]
    [Tooltip("무기형상 지연시간")]
    public float thirdNormalAttackWeaponWaitTime = 1f;
    [Tooltip("무기형상 지속시간")]
    public float thirdNormalAttackWeaponDuration = 0;
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

#region
public partial class PlayerAnimalMaskData : ScriptableObject
{
    [Space(10f)]
    [Header("A스킬")]
    [Tooltip("A스킬 기본")]
    public PlayerBasicStatStruct leapStrikeStat;
    
    [Space(10f)]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct leapStrikeInput;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct leapStrikeStop;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] leapStrikeSkillMove;
    [Tooltip("행동 제한")]
    public RestrictStruct leapStrikeRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] leapStrikeAnimationSpeed;
    [Tooltip("트레일 이펙트")]
    public EffectStruct leapStrikeTrailEffect;
    [Tooltip("슬래쉬 이펙트")]
    public EffectStruct leapStrikeSlashEffect;
    [Tooltip("무기형상 지연시간")]
    public float leapStrikeWeaponWaitTime = 1f;
    [Tooltip("무기형상 지속시간")]
    public float leapStrikeWeaponDuration = 0;
    [Space(10f)]

    [Header("아직 추가 구현 필요")]
    [Tooltip("히트박스 지연시간")]
    public float leapStrikeHitBoxWaitTime = 1f;
    [Tooltip("히트박스 지속시간")]
    public float leapStrikeHitBoxDuration=0;
    [Tooltip("히트박스 타격 횟수")]
    public int leapStrikeHitCount = 2;
    [Tooltip("히트박스 타격 간격")]
    public float leapStrikeHitInterval = .4f;
    [Space(10f)]
    [Tooltip("점프 사운드")]
    public SoundStruct leapStrikeJumpSound;
    [Tooltip("공중 사운드")]
    public SoundStruct leapStrikeFloatSound;
    [Tooltip("슬래쉬 사운드")]
    public SoundStruct leapStrikeSlashSound;
    
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct leapStrikeCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct leapStrikeGameTimeScale;

    [Space(10f)]
    [Header("A스킬 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct leapStrikeHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct leapStrikeHitSound;
    [Tooltip("카메라 히트 쉐이크")]
    public CameraShakeStruct leapStrikeHitCameraShake;
    [Tooltip("히트스탑")]
    public TimeScaleStruct leapStrikeHitTimeScale;
}
#endregion

#region
public partial class PlayerAnimalMaskData : ScriptableObject
{
    [Space(10f)]
    [Header("S스킬")]
    [Tooltip("S스킬 기본")]
    public PlayerBasicStatStruct roarStat;


    [Space(10f)]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct roarInput;
    [Tooltip("스킬 정지")]
    public FunctionStopStruct roarStop;
    [Tooltip("스킬 물리 이동")]
    public SkillMoveStruct[] roarSkillMove;
    [Tooltip("행동 제한")]
    public RestrictStruct roarRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] roarAnimationSpeed;
    [Tooltip("트레일 이펙트")]
    public EffectStruct roarTrailEffect;
    [Tooltip("차징 이펙트")]
    public EffectStruct roarChargeEffect;
    [Tooltip("디스차징 이펙트")]
    public EffectStruct roardisChargeEffect;
    [Space(10f)]
    [Tooltip("무기형상 지연시간")]
    public float roarWeaponWaitTime = 1f;
    [Tooltip("무기형상 지속시간")]
    public float roarWeaponDuration = 0;
    [Space(10f)]
    [Tooltip("히트박스 지연시간")]
    public float roarHitBoxWaitTime = 1f;
    [Tooltip("히트박스 지속시간")]
    public float roarHitBoxDuration = 0;
    [Tooltip("히트박스 스케일(크기)")]
    public Vector3 roarHitBoxScale = new Vector3(9,9,9);
    [Tooltip("히트박스 타격 횟수")]
    public int roarHitCount = 2;
    [Tooltip("히트박스 타격 간격")]
    public float roarHitInterval = .4f;
    [Space(10f)]
    [Tooltip("차지 사운드")]
    public SoundStruct roarChargeSound;
    [Tooltip("디스차지 사운드")]
    public SoundStruct roarDischargeSound;
    [Tooltip("포효 사운드")]
    public SoundStruct roarSound;
    [Tooltip("카메라 쉐이크")]
    public CameraShakeStruct roarCameraShake;
    [Tooltip("타임스케일")]
    public TimeScaleStruct roarGameTimeScale;


    [Space(10f)]
    [Header("S스킬 - 타격판정")]
    [Tooltip("타격 이펙트")]
    public EffectStruct roarHitEffect;
    [Tooltip("타격 사운드")]
    public SoundStruct roarHitSound;
    [Tooltip("카메라 히트 쉐이크")]
    public CameraShakeStruct roarHitCameraShake;
    [Tooltip("히트스탑")]
    public TimeScaleStruct roarHitTimeScale;
}
#endregion

public partial class PlayerAnimalMaskData : ScriptableObject
{
    #region 싱글톤
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/PlayerAnimalMaskData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static PlayerAnimalMaskData instance;
    public static PlayerAnimalMaskData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<PlayerAnimalMaskData>("PlayerAnimalMaskData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<PlayerAnimalMaskData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<PlayerAnimalMaskData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
                    AssetDatabase.CreateAsset(instance, SettingFilePath); //방금 생성된 오브젝트를 유니티에셋으로 생성, 저장할 수 있게
                }

            }
#endif
            return instance;
        }
    }
    #endregion
}