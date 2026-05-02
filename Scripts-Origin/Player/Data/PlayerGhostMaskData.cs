using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




public partial class PlayerGhostMaskData : ScriptableObject
{
    [Header("공용")]
    [Header("귀신탈 스킬 쿨타임")]
    public float cooldown = 20;
    [Header("처형 타겟 탐지 범위")]
    public float detectRange=20;
    [Header("처형 가능 각도")]
    public float viewAngle=100;
    [Header("처형 스킬 범위")]
    public float skillRange=20;
}

public partial class PlayerGhostMaskData : ScriptableObject
{
    [Space(20)]
    [Header("사람탈 처형")]
    [Tooltip("낫으로 변경시간")]
    public float humanSetGhostWeaponTime;
    [Tooltip("원래 무기로 변경시간")]
    public float humanSetOriginalWeaponTime;
    [Tooltip("적 사망판정 시간")]
    public float humanKillTargetTime;

    [Space(10)]
    [Tooltip("물리 이동")]
    public SkillMoveStruct[] humanSkillMove;
    [Tooltip("행동 제한")]
    public RestrictStruct humanRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] huamnHitGroundAnimationSpeed;
    public animationSpeedStruct[] humanSwingAnimationSpeed;

    [Space(10)]
    [Tooltip("컷 이펙트")]
    public EffectStruct humanCutEffect;
    [Tooltip("돔 이펙트")]
    public EffectStruct humanDomeEffect;
    
    [Space(10)]
    [Tooltip("스킬 사운드")]
    public SoundStruct humanHitGroundSound;
    public SoundStruct humanSwingSound;
    public SoundStruct humanAfterSwingSound;

    [Space(10)]
    [Header("카메라 쉐이크")]
    public CameraShakeStruct humanFinishSwingCameraShake;
    public CameraShakeStruct humanFinishHitGroundCameraShake;

    [Space(10)]
    [Header("타임스케일")]
    public TimeScaleStruct humanFinishTimeScale;

    //[Header("S스킬 - 타격판정")]
    //[Tooltip("타격 이펙트")]
    //public EffectStruct inkFloorHitEffect;
    //[Tooltip("타격 사운드")]
    //public SoundStruct inkFloorHitSound;
    //[Tooltip("카메라 쉐이크")]
    //public CameraShakeStruct inkFloorHitCameraShake;
}

public partial class PlayerGhostMaskData : ScriptableObject
{
    [Space(20)]
    [Header("동물탈 처형")]
    [Tooltip("낫으로 변경시간")]
    public float animalSetGhostWeaponTime;
    [Tooltip("원래 무기로 변경시간")]
    public float animalSetOriginalWeaponTime;
    [Tooltip("적 사망판정 시간")]
    public float animalKillTargetTime;


    [Space(10)]
    [Tooltip("물리 이동")]
    public SkillMoveStruct[] animalSkillMove;
    [Tooltip("행동 제한")]
    public RestrictStruct animalRestrict;
    [Tooltip("애니메이션 속도")]
    public animationSpeedStruct[] animalSweapAnimationSpeed;
    public animationSpeedStruct[] animalSwingAnimationSpeed;


    [Space(10)]
    [Tooltip("컷 이펙트")]
    public EffectStruct animalCutEffect;
    [Tooltip("돔 이펙트")]
    public EffectStruct animalDomeEffect;
    
    [Space(10)]
    [Tooltip("스킬 사운드")]
    public SoundStruct animalSweapSound;
    public SoundStruct animalSwingSound;
    public SoundStruct animalAfterSwingSound;
    
    [Space(10)]
    [Header("카메라 쉐이크")]
    public CameraShakeStruct animalFinishSweapCameraShake;
    public CameraShakeStruct animalFinishSwingCameraShake;

    [Space(10)]
    [Header("타임스케일")]
    public TimeScaleStruct animalFinishTimeScale;

}

public partial class PlayerGhostMaskData : ScriptableObject
{
    #region 싱글톤
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/PlayerGhostMaskData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static PlayerGhostMaskData instance;
    public static PlayerGhostMaskData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<PlayerGhostMaskData>("PlayerGhostMaskData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<PlayerGhostMaskData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<PlayerGhostMaskData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
                    AssetDatabase.CreateAsset(instance, SettingFilePath); //방금 생성된 오브젝트를 유니티에셋으로 생성, 저장할 수 있게
                }

            }
#endif
            return instance;
        }
    }
    #endregion
}