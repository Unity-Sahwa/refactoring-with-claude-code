using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


public partial class CheatData: ScriptableObject
{
    [Header("입력키 쉽게 찾는 방법: 원하는 입력키의 첫글자가 나올때까지 첫글자 계속 입력하면되요. \n예를 들어 'Space' 라고 한다면 S 계속 누르다 보면 Space가 보여요 \n숫자키는 alpha0,1,2... 입니다")]
    [Header("프레임 확인")]
    public int frameTextSize = 25;
    public Color frameTextColor = Color.green;
    public KeyCode showFrame;
    public KeyCode frameLimitOff;
    public KeyCode frame30;
    public KeyCode frame60;
    public KeyCode frame144;

    //[Header("UI,HUD 보이기")]
    //public KeyCode showUI;
    //public KeyCode showHUD;
    
    [Header("위의 기능과는 별개로 작동-----------------------------------------------------------------")]
    [Header("치트 활성화")]
    public KeyCode activateCheatMode;

    [Header("저장, 불러오기(첫번째 슬롯에 적용)")]
    public KeyCode saveData;
    public KeyCode loadData;
    
    [Header("게임속도(0이면 일시정지)")]
    public KeyCode setGameTimeRate;
    public float timeScaleValue;

    [Header("데미지맥스, 풀스택")]
    public KeyCode damageMax;
    public KeyCode paintOverlapMax;

    [Header("이동속도")]
    public KeyCode moveSpeedUp;
    public float moveSpeed;

    [Header("점멸")]
    public KeyCode blink;
    public float blinkDistance;

    [Header("플라이모드")]
    public KeyCode flyMode;
    public KeyCode moveUp;
    public float flySpeed;


    [Header("체력")]
    public KeyCode minHealth;
    public KeyCode maxHealth;

    [Header("사망")]
    public KeyCode dieFromZeroHealth;
    public KeyCode dieFromFall;

    [Header("주변 적제거")]
    public KeyCode clearEnemy;
    public float clearRange;
    public LayerMask clearLayer;

    [Header("좌표이동(근데 지도 업데이트가 필요함, 아니면 이동하길 원하는 공간 말해주세요)")]
    public KeyCode showMap;
}


#region Set
public partial class CheatData : ScriptableObject
{
    #region 싱글톤
    //지연생성 싱글톤: 싱글톤 오브젝트에 접근하는 순간에 오브젝트가 없다면 만들어줌

    private const string SettingFileDirectory = "Assets/Resources";
    private const string SettingFilePath = "Assets/Resources/CheatData.asset";
    //리소스는 되도록 사용X, 전역으로 사용되는 것은 큰 문제 없다고 함
    //Resources 폴더가 없는지 확인, 없으면 생성

    private static CheatData instance;
    public static CheatData Instance
    {
        get
        {
            if (instance != null) //instance가 존재한다면 가져오기
            {
                return instance;
            }

            //없다면 
            instance = Resources.Load<CheatData>("CheatData");

            //에디터 타임에서 자동으로 미리 생성되도록, 런타임에 무조건 있어야함.
#if UNITY_EDITOR

            if (instance == null)
            {
                if (!AssetDatabase.IsValidFolder(SettingFileDirectory))//해당 파일이 유효한지
                {
                    AssetDatabase.CreateFolder("Assets", "Resources"); //아니라면 Assets 아래에 Resources 폴더 생성
                }

                //어떤 이유로 파일이 안가져왔다면 하드하게 가져오기
                instance = AssetDatabase.LoadAssetAtPath<CheatData>(SettingFilePath);

                if (instance == null) //그럼에도 안가져와진다면 없다는 것. 새로 만들어주기
                {
                    instance = CreateInstance<CheatData>(); //이렇게 생성하면 메모리에만 존재. 파일 에셋으로 저장이 안됨
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

