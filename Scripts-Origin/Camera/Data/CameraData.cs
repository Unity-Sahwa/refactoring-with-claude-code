using UnityEditor;
using UnityEngine;
using static SetPlayerData;

//주목
public partial class CameraData : ScriptableObject
{
    [Header("주목 ")] 
    [Header("최대 탐지 거리")]
    [Range(0, 40)] public float detectRange = 20;
    
    [Header("가깝다고 판단되는 적의 거리")]
    [Range(0, 20)] public float distanceWithCloseTarget = 5;

    [Header("적 레이어")]
    public LayerMask enemyLayer;

    [Header("장애물 레이어")]
    public LayerMask obstacleLayer;

    [Header("주목 시야각 (60도 설정하면 120도 시야각 적용)")]
    [Range(0, 70)] public float maximumAngleWithTarget = 60;

    [Header("주목 유지 한계 거리")]
    [Range(0, 30)] public float maximumDistanceWithTarget = 60;
}

//마커
public partial class CameraData : ScriptableObject
{
    [Space(20)]
    [Header("주목(마커)")]
    [Header("타겟 마커 위치 오프셋")]
    public Vector3 targetMarkerOffset = new Vector3(0, 1.5f, 0);

    [Header("타겟 마커 스케일")]
    public Vector3 targetMarkerScale = new Vector3(0.04f, 0.04f, 0.04f);

    [Header("비주목 마커 컬러 (빨,초,파,투명도)")]
    public Vector4 detectedTargetMarkerColor = new Vector4(1, 0, 0, 0.2f);

    [Header("주목 마커 컬러 (빨,초,파,투명도)")]
    public Vector4 LockOnTargetMarkerColor = new Vector4(1, 0, 0, 1);
}


public partial class CameraData : ScriptableObject
{
    [Space(20)]
    [Header("좌우 스와이프 속도비율")]
    public float cameraHorizontalSwipeRate;
}

public partial class CameraData : ScriptableObject
{
    #region 싱글톤
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/CameraData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static CameraData instance;
    public static CameraData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<CameraData>("CameraData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<CameraData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<CameraData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
                    AssetDatabase.CreateAsset(instance, SettingFilePath); //방금 생성된 오브젝트를 유니티에셋으로 생성, 저장할 수 있게
                }

            }
#endif
            return instance;
        }
    }
    #endregion
}