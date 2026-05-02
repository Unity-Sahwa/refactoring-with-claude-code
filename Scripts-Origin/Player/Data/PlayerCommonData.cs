using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class PlayerCommonData : ScriptableObject
{
    [Header("방향키 입력 속도")]
    public float inputIncreaseSpeed;
}

public partial class PlayerCommonData: ScriptableObject
{
    [Space(20)]
    [Header("플레이어")]
    [Header("최대 체력")]
    public float maxHp = 20;
    [Header("회전속도")]
    public float playerRotateSpeed = 40;
    [Header("추가 중력")]
    public float additionalGravity = -10;
    [Header("충돌 감지 거리(앞)")]
    public float playerForwardSensorRange = 2;
    [Header("충돌 감지 거리(뒤)")]
    public float playerBackwardSensorRange = 2;
    [Header("충돌 감지 레이어")]
    public LayerMask collisionLayer;
}

#region 이동
public partial class PlayerCommonData : ScriptableObject
{
    [Space(20)]
    [Header("이동")]
    [Header("애니메이션 속도")]
    public animationSpeedStruct[] runAnimationSpeed;
    [Header("도화탈 뛰는 소리")]
    public SoundStruct humanRunSound;
    [Header("산곡탈 뛰는 소리")]
    public SoundStruct animalRunSound;
}

#endregion

#region 피격
public partial class PlayerCommonData : ScriptableObject
{
    [Space(20)]
    [Header("피격")]
    [Header("쿨타임")]
    public float hitCooldown = 2;
    [Header("hitRestrict의 untilFinish 체크해주세요")]
    public RestrictStruct hitRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] hitAnimationSpeed;
    [Tooltip("피격소리")]
    public SoundStruct hitSound;
    [Tooltip("심장박동소리")]
    public SoundStruct heartBeatSound;
    [Tooltip("카메라이펙트")]
    public VignetteStruct hitVignette;
}
#endregion

#region 사망
public partial class PlayerCommonData : ScriptableObject
{
    [Space(20)]
    [Header("사망")]
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] dieAnimationSpeed;
    [Tooltip("사망 소리")]
    public SoundStruct dieSound;
    [Tooltip("사망 이후 소리")]
    public SoundStruct afterDeadSound;
    [Tooltip("낙사 카메라 보정")]
    public CameraRecomposerStruct fallDeathCameraRecomposer;
    [Tooltip("체력0 사망 카메라 보정")]
    public CameraRecomposerStruct zeroHealthDeathCameraRecomposer;
}
#endregion

#region 탈교체
public partial class PlayerCommonData : ScriptableObject
{
    [Space(20)]
    [Header("탈교체")]
    [Header("탈교체 쿨타임")]
    public float changeMaskCooldown = 2;
    [Header("탈교체 소리")]
    public SoundStruct maskChangeSound;
    
    [Header("사람탈 이펙트")]
    public EffectStruct humanMaskEffect;
    [Header("동물탈 이펙트")]
    public EffectStruct animalMaskEffect;
    [Header("귀신탈 이펙트")]
    public EffectStruct ghostMaskEffect;

    [Header("동반자 충돌 이펙트(수정금지)")]
    public EffectStruct partnerCollisionEffect;
}
#endregion

#region 대쉬
public partial class PlayerCommonData : ScriptableObject
{
    [Space(20)]
    [Header("대쉬")]
    [Header("대쉬 쿨타임")]
    public float dashCooldown = 2;


    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct dashInput;
    [Tooltip("물리 이동")]
    public SkillMoveStruct[] dashMove;
    [Tooltip("행동 제한")]
    public RestrictStruct dashRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] frontDashAnimationSpeed;
    [Tooltip("소리")]
    public SoundStruct dashSound;

    [Space(20)]
    [Header("백대쉬")]
    [Tooltip("입력 처리")]
    public PlayerSkillInputStruct backDashInput;
    [Tooltip("물리 이동")]
    public SkillMoveStruct[] backDashMove;
    [Tooltip("행동 제한")]
    public RestrictStruct backDashRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] backDashAnimationSpeed;
    [Tooltip("소리")]
    public SoundStruct backDashSound;
}
#endregion

#region 싱글톤
public partial class PlayerCommonData : ScriptableObject
{
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/PlayerCommonMaskData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static PlayerCommonData instance;
    public static PlayerCommonData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<PlayerCommonData>("PlayerCommonMaskData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<PlayerCommonData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<PlayerCommonData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
                    AssetDatabase.CreateAsset(instance, SettingFilePath); //방금 생성된 오브젝트를 유니티에셋으로 생성, 저장할 수 있게
                }

            }
#endif
            return instance;
        }
    }
}
#endregion