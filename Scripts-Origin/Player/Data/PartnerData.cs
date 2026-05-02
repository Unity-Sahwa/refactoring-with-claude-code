using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public partial class PartnerData: ScriptableObject
{
    [Header("파트너가 따라다니는 최대거리")]
    public float followMaxDistance;

    [Header("파트너가 부드럽게 따라다니는 정도(가까울 때) \nx축은 거리 비율, y축은 smoothTime 값을 의미 \nx축에서 0에 가까울수록 플레이어와 파트너의 거리가 가까워짐")]
    [Range(0, 1)] public float defaultFollowSmoothTime;

    [Header("파트너가 부드럽게 따라다니는 정도(가까울 때) \nx축은 거리 비율, y축은 smoothTime 값을 의미")]
    public AnimationCurve smoothTimeByDistanceCurve;

    [Header("애니메이션 커브가 적용되는 거리")]
    public float smoothingDistance;
}

public partial class PartnerData : ScriptableObject
{
    [Space(20)]
    //충돌
    public float speedRate;
}

public partial class PartnerData : ScriptableObject
{
    #region 싱글톤
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/PartnerData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static PartnerData instance;
    public static PartnerData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<PartnerData>("PartnerData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<PartnerData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<PartnerData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
                    AssetDatabase.CreateAsset(instance, SettingFilePath); //방금 생성된 오브젝트를 유니티에셋으로 생성, 저장할 수 있게
                }
            }
#endif
            return instance;
        }
    }
    #endregion
}