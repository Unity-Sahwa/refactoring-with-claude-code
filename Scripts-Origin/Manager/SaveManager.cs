using System; 
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public enum KeyAction
{ 
    UP,
    DOWN,
    LEFT,
    RIGHT,

    LOCKONTARGET,
    INTERACT,

    ATTACK_NORMAL,
    ATTACK_SPECIAL,
    ATTACK_FINISH,
    DASH,

    MENU,

    KEYCOUNT
}

public enum SceneName
{
    Area_0_Tutorial,
    Area_1,
    Area_2,
    Area_3,
    Area_4_FirstBoss
}

public enum AreaName
{
    Area_00, //덕굴 옆 샛길
    Area_01, //덕굴 입구 1

    Area_10, //덕굴 입구 2
    Area_11, //현무 중앙길

    Area_20, //서쪽으로 가는길
    Area_21, //위수의 다리 1
    Area_22, //위수의 다리 2
    Area_23, //위수의 다리 3
    Area_24, //위수의 다리 4

    Area_30, //뱀의 정원 1
    Area_31, //뱀의 정원 2
    Area_32, //뱀의 정원 3
    Area_33, //뱀의 정원 4
    Area_34, //뱀의 정원 5

    Area_40 //위수의 방
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    [SerializeField] private Player player;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private MenuUI menuUI;
    [SerializeField] private MaskChange maskChange;

    #region playState
    private int currentIndex;
    public int CurrentIndex
    {
        get
        {
            return currentIndex;
        }
        set
        {
            currentIndex = value;
        }
    }

    private string[] filePath = new string[4];
    public string[] Filepath
    {
        get { return filePath; }
    }

    private int? selectedIndex = null;
    public int? SelectedIndex
    {
        get
        {
            return selectedIndex;
        }
        set
        {
            selectedIndex = value;
        }
    }

    private int currentHP;
    public int CurrentHP
    {
        get 
        {
            return currentHP; 
        } 
        set
        {
            currentHP = value;
        }
    }

    private bool isHumanMask;
    public bool IsHumanMask
    {
        get
        {
            return isHumanMask;
        }
        set
        {
            isHumanMask = value;
        }
    }


    [SerializeField] private List<GameObject> lights; // Light 오브젝트를 포함하는 리스트로 변경
    [SerializeField] private List<GameObject> postProcessVolumes; // PostProcessVolume이 포함된 빈 오브젝트 리스트로 변경
    #endregion

    #region Position
    private int currentSceneIndex;
    public int CurrentSceneIndex
    {
        get
        {
            return currentSceneIndex;
        }
        set
        {
            currentSceneIndex = value;
        }
    }
    
    private int currentAreaIndex;
    public int CurrentAreaIndex
    {
        get 
        {
            return currentAreaIndex; 
        }
        set 
        {
            currentAreaIndex = value; 
        }
    }
    
    private Vector3 currentPosition;
    public Vector3 CurrentPosition
    {
        get
        {
            return currentPosition;
        }
        set
        {
            currentPosition = value;
        }
    }

    private Transform startPosition;
    #endregion

    #region Sound
    public float masterVolume { get; private set; }
    public float BGMVolume { get; private set; }
    public float enemySFXVolume { get; private set; }
    public float playerSFXVolume { get; private set; }

    private string soundSettingFilePath;
    #endregion

    #region Input
    //Key
    private string inputKeySettingFilePath;
    private Dictionary<KeyAction, KeyCode> inputKeys = new Dictionary<KeyAction, KeyCode>();
    public Dictionary<KeyAction, KeyCode> InputKeys { get { return inputKeys; } }

    private KeyCode[] defaultKeys = new KeyCode[]
    //{KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.None,KeyCode.Escape };

    { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D,
    KeyCode.LeftShift, KeyCode.X,
    KeyCode.Mouse0, KeyCode.Q, KeyCode.F, KeyCode.Mouse1,
    KeyCode.Escape};

    //Mouse 

    private string mouseSettingFilePath;
    public float mouseSpeedWithXAxis { get; private set; } //좌우
    public float mouseSpeedWithYAxis { get; private set; } //상하
    #endregion

    private void Awake()
    {
        #region 싱글톤
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
        #endregion

        //filePath마다 저장경로 연결
        for (int i = 0; i < filePath.Length; i++)
        {
            filePath[i] = Path.Combine(Application.persistentDataPath, $"CharacterSlotData{i}");
        }

        //소리파일 경로 결합. 저장된 데이터, 아니면 디폴트 볼륨값 불러오기
        soundSettingFilePath = Path.Combine(Application.persistentDataPath, "SoundSettingData");
        if (!File.Exists(soundSettingFilePath))
        {
            ChangeVolumeSetting(0, 0, 0, 0);
            SaveSoundData();
        }
        LoadSoundData();

        //키셋팅 경로 결합. 저장된 데이터, 아니면 디폴트 키 불러오기
        inputKeySettingFilePath = Path.Combine(Application.persistentDataPath, "InputKeySettingData");
        if (!File.Exists(inputKeySettingFilePath))
        {
            ChangeKeysSetting(defaultKeys);
            SaveInputKeyData();
        }
        LoadInputKeyData();

        //마우스 경로 결합. 저장된 데이터, 아니면 디폴트 볼륨값 불러오기
        mouseSettingFilePath = Path.Combine(Application.persistentDataPath, "MouseSettingData");
        if (!File.Exists(mouseSettingFilePath))
        {
            ChangeMouseSetting(250, 15);
            SaveMouseData();
        }
        LoadMouseData();
    }
    private void Start()
    {
        player = PlayerController.instance.player;

        //시작시 저장된 데이터 슬롯에 기록
        menuUI.ResetSlot();
        for (int i = 0; i < filePath.Length; i++)
        {
            WriteSlotDate(i);
        }
        menuUI.SortSlots();

        //씬 바뀔때 계속 0으로 갈순없음
        //currentIndex = 0;
        selectedIndex = null;
    }

    #region PlayerState Data
    //다음 저장 경로로 넘기는 함수(currentIndex가 )
    public void MoveToNextIndex()
    {
        //저장 파일이 하나도 없으면 반환
        bool useFunction = false;
        for (int i = 0; i < filePath.Length; i++)
        {
            if (File.Exists(filePath[i]))
            {
                useFunction = true;
            }
        }
        if (!useFunction)
        {
            return;
        }

        CurrentIndex = (CurrentIndex + 1) % filePath.Length;
    }

    public void MoveToPreviousIndex()
    {
        //저장 파일이 하나도 없으면 반환
        bool useFunction = false;
        for (int i = 0; i < filePath.Length; i++)
        {
            if (File.Exists(filePath[i]))
            {
                useFunction = true;
            }
        }
        if (!useFunction)
        {
            return;
        }

        CurrentIndex = (filePath.Length + CurrentIndex - 1) % filePath.Length;
    }

    public void SelectIndex(int index)
    {
        //문제가 있다면 선택하고 취소했을때 currentIndex가 그대로 남음
        selectedIndex = index;

        //currentIndex = index;
        //currentFilePath = filePath[currentIndex];
    }

    //selectedIndex를 currentIndex로 확정지음
    public void SetCurrentIndex()
    {
        CurrentIndex = selectedIndex.Value;
    }

    //public bool HasSaveRecord()
    //{
    //    if (!File.Exists(currentFilePath))
    //    {
    //        return false;
    //    }
    //    using (StreamReader reader = new StreamReader(currentFilePath))
    //    {
    //        string line;
    //        if ((line = reader.ReadLine()) == null)
    //        {
    //            return false;
    //        }
    //    }

    //    return true;
    //}

    // CSV 파일에 캐릭터 정보를 저장, 다음 슬롯으로 넘어감
    public void SaveSloatData()
    {//currentIndex, 날짜, 위치, 탈상태, 조명, 포스트 프로세싱 저장 후 다음 currentIndex로 넘김
        float health = player.currentHP;
        int maskType = maskChange.HumanMask.activeSelf ? 1 : 0;
        string currentTime = null;
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        using (StreamWriter writer = new StreamWriter(filePath[CurrentIndex]))
        {
            // currentIndex
            writer.WriteLine("CurrentIndex");
            writer.WriteLine($"{CurrentIndex}");
            
            // 시간(초단위)
            writer.WriteLine("Time");
            currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            writer.WriteLine($"{currentTime}");

            // SceneIndex
            writer.WriteLine("SceneIndex");
            writer.WriteLine($"{currentSceneIndex}");

            // AreaIndex
            writer.WriteLine("AreaIndex");
            writer.WriteLine($"{currentAreaIndex}");

            // 캐릭터 탈상태
            writer.WriteLine("MaskType");
            int isHumanMask = maskChange.HumanMask.activeSelf ? 1 : 0;
            writer.WriteLine($"{isHumanMask}");

            // 캐릭터 위치
            writer.WriteLine("CharacterPosition");
            writer.WriteLine($"{CurrentPosition.x},{CurrentPosition.y},{CurrentPosition.z}");

            // 체력
            writer.WriteLine("Health");
            writer.WriteLine($"{health}");

            // 조명 활성화 유무
            writer.WriteLine("LightObject,Enabled");
            foreach (var light in lights)
            {
                int isEnabled = light.activeSelf ? 1 : 0; // 활성화 상태를 1 또는 0으로 저장
                writer.WriteLine($"{light.name},{isEnabled}");
            }

            // 포스트 프로세싱 활성화 유무
            writer.WriteLine("PostProcessingObject,Enabled");
            foreach (var postProcessVolume in postProcessVolumes)
            {
                int isEnabled = postProcessVolume.activeSelf ? 1 : 0; // 활성화 상태를 1 또는 0으로 저장
                writer.WriteLine($"{postProcessVolume.name},{isEnabled}");
            }
        }

        //현재 슬롯에 기록
        menuUI.RecordSlot(CurrentIndex, currentSceneIndex, currentAreaIndex, currentTime);
    }
    public void LoadSlotData()
    {
        if (!File.Exists(filePath[CurrentIndex]))
        {
            return;
        }

        using (StreamReader reader = new StreamReader(filePath[CurrentIndex]))
        {
            string line;

            //변수가 여러 개일 때
            bool isLightSection = false;
            bool isPostProcessingSection = false;

            //읽는게 끝날 때까지 반복
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');

                //순서
                if (values[0] == "CurrentIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    CurrentIndex = int.Parse(values[0]);
                }

                if (values[0] == "SceneIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentSceneIndex = int.Parse(values[0]);
                }

                if (values[0] == "AreaIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentAreaIndex = int.Parse(values[0]);
                }

                if (values[0] == "MaskType")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    if (int.Parse(values[0]) == 1)
                    {
                        if (maskChange.AnimalMask.activeSelf)
                        {
                            maskChange.ChangeCharacter();
                        }
                    }
                    else
                    {
                        if (maskChange.HumanMask.activeSelf)
                        {
                            maskChange.ChangeCharacter();
                        }
                    }
                }

                // 캐릭터 위치와 체력 불러오기
                if (values[0] == "CharacterPosition")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');

                    maskChange.CurrentMask.transform.position = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                }

                if (values[0] == "Health")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    player.currentHP = int.Parse(values[0]);
                    HpHUD.instance.ChangeHPStack((int)player.currentHP);
                }
                
                // 라이트 오브젝트 활성화 상태 불러오기
                else if (values[0] == "LightObject")
                {
                    isLightSection = true;
                    continue;
                }
                else if (isLightSection && values.Length == 2)
                {
                    foreach (var light in lights)
                    {
                        if (light.name == values[0])
                        {
                            light.SetActive(values[1] == "1");
                            break;
                        }
                    }
                }
                // 포스트 프로세싱 오브젝트 활성화 상태 불러오기
                else if (values[0] == "PostProcessingObject")
                {
                    isPostProcessingSection = true;
                    continue;
                }
                else if (isPostProcessingSection && values.Length == 2)
                {
                    foreach (var postProcessVolume in postProcessVolumes)
                    {
                        if (postProcessVolume.name == values[0])
                        {
                            postProcessVolume.SetActive(values[1] == "1");
                            break;
                        }
                    }
                }
            }
        }
    }

    //현재의 데이터를 얻는게 아님!
    //currentIndex 경로의 씬, 지역, 플레이어 위치 데이터 가져오기 
    //current~ 변수에 할당
    public void GetLoadData()
    {
        using (StreamReader reader = new StreamReader(filePath[CurrentIndex]))
        {
            string line;

            //읽는게 끝날 때까지 반복
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');

                if (values[0] == "CurrentIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    CurrentIndex = int.Parse(values[0]);
                }

                else if (values[0] == "SceneIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentSceneIndex = int.Parse(values[0]);
                }


                else if (values[0] == "AreaIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentAreaIndex = int.Parse(values[0]);
                }

                else if (values[0] == "MaskType")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');

                    isHumanMask = (int.Parse(values[0]) == 1);
                }

                // 캐릭터 위치와 체력 불러오기
                if (values[0] == "CharacterPosition")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');

                    currentPosition = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                }

                if (values[0] == "Health")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentHP = int.Parse(values[0]);
                }
            }
        }
    }

    public void ResetPlayerPosition()
    {
        //씬별로 saveManager의 첫번째 자식오브젝트의 위치에 플레이어 배치
        startPosition = transform.GetChild(0).transform;
        maskChange.CurrentMask.transform.position = startPosition.position;
        maskChange.CurrentMask.transform.rotation= startPosition.rotation;
    }
    public void ResetData()
    {//저장된 파일, 슬롯 전부 리셋

        for (int i = 0; i < filePath.Length; i++)
        {
            if (File.Exists(filePath[i]))
            {
                File.Delete(filePath[i]);
            }

        }

        menuUI.ResetSlot();

        CurrentIndex = 0;
    }

    //저장된 데이터를 슬롯에 기록. 
    public void WriteSlotDate(int playStateIndex)
    {
        //저장된 데이터가 없다면 return;
        if (!File.Exists(filePath[playStateIndex]))
        {
            return;
        }

        //저장된 데이터가 있다면 기록
        using (StreamReader reader = new StreamReader(filePath[playStateIndex]))
        {
            string line;

            string currentTime = null;
            string currentSceneIndex = null;
            string currentAreaIndex = null;

            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');

                //기록된 시간을 찾아서 슬롯UI에 기록
                if (values[0] == "Time")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentTime = values[0];
                }

                if (values[0] == "SceneIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentSceneIndex = values[0];
                }

                if (values[0] == "AreaIndex")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    currentAreaIndex = values[0];
                }
            }
            //"자동 저장" 이라고 적어주기
            //스위치식으로

            menuUI.RecordSlot(playStateIndex, int.Parse( currentSceneIndex), int.Parse( currentAreaIndex), currentTime);
        }
    }
    #endregion

    #region Volume
    public void SaveSoundData()
    {//소리, 단축키 등 환경적인 요소 데이터 저장

        using (StreamWriter writer = new StreamWriter(soundSettingFilePath))
        {
            //Volume
            writer.WriteLine("Volume");
            writer.WriteLine($"{masterVolume},{BGMVolume},{enemySFXVolume},{playerSFXVolume}" );
        }
    }

    public void ChangeVolumeSetting(float master, float BGM, float enemySFX, float playerSFX )
    {
        masterVolume = master;
        BGMVolume = BGM;
        enemySFXVolume = enemySFX;
        playerSFXVolume = playerSFX;
    }
    public void LoadSoundData()
    {
        if (!File.Exists(soundSettingFilePath))
        {
            return;
        }

        using (StreamReader reader = new StreamReader(soundSettingFilePath))
        {
            string line;

            //읽는게 끝날 때까지 반복
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');

                //마스터, BGM, SFX 볼륨 불러오기
                if (values[0] == "Volume")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    masterVolume = float.Parse(values[0]);
                    BGMVolume = float.Parse(values[1]);
                    enemySFXVolume = float.Parse(values[2]);
                    playerSFXVolume = float.Parse(values[3]);
                }
            }
        }
    }
    #endregion

    #region InputKey
    public void SaveInputKeyData()
    {//수정된 inputKeys를 파일에 저장
        using (StreamWriter writer = new StreamWriter(inputKeySettingFilePath))
        {
            writer.WriteLine("KeyAction,KeyCode");
            foreach (var dict in inputKeys)
            {
                writer.WriteLine($"{(int)dict.Key},{dict.Value}");
            }
        }
    }

    public void     ChangeKeysSetting(KeyCode[] keycode)
    {//inputKeys 변수에 값을 할당

        for (int i = 0; i < (int)KeyAction.KEYCOUNT; i++)
        {
            KeyAction keyAction = (KeyAction)i;

            if (!inputKeys.ContainsKey(keyAction))
            {//inputKeys가 해당 key를 가지고 있지 않다면 
                inputKeys.Add(keyAction, keycode[i]);
            }
            else
            {
                inputKeys[(KeyAction)i] = keycode[i];
            }
        }
    }

    public void ChangeKeySetting(int index, KeyCode keycode)
    {//inputKeys 변수에 값을 할당

        if (!inputKeys.ContainsKey((KeyAction)index))
        {
            inputKeys.Add((KeyAction)index, keycode);
        }
        else
        {
            inputKeys[(KeyAction)index] = keycode;
        }
    }


    public void LoadInputKeyData()
    {//불러올 파일이 없다면 inputKeys는 디폴트키가 된다.

        if (!File.Exists(inputKeySettingFilePath))
        {
            return;
        }

        using (StreamReader reader = new StreamReader(inputKeySettingFilePath))
        {
            string line;
            bool isKeyActionSection = false;

            //읽는게 끝날 때까지 반복
            while ((line = reader.ReadLine()) != null)
            {
                //values에 ','로 분리시켜서 문자열 저장
                var values = line.Split(',');

                //KeyAction 부분부터 시작
                if (values[0] == "KeyAction")
                {
                    isKeyActionSection = true;
                    continue;
                }
                else if (isKeyActionSection && values.Length == 2)
                {

                    int keyActionIdex = int.Parse(values[0]);
                    KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), values[1]);

                    ChangeKeySetting(keyActionIdex, keyCode);
                }
            }
        }
    }

    #endregion

    #region Mouse
    public void SaveMouseData()
    {//소리, 단축키 등 환경적인 요소 데이터 저장

        using (StreamWriter writer = new StreamWriter(mouseSettingFilePath))
        {
            writer.WriteLine("Mouse");
            writer.WriteLine($"{mouseSpeedWithXAxis},{mouseSpeedWithYAxis}");
        }
    }

    public void ChangeMouseSetting(float xValue, float yValue)
    {
        mouseSpeedWithXAxis = xValue;
        mouseSpeedWithYAxis = yValue;
    }
    public void LoadMouseData()
    {
        if (!File.Exists(mouseSettingFilePath))
        {
            return;
        }

        using (StreamReader reader = new StreamReader(mouseSettingFilePath))
        {
            string line;

            //읽는게 끝날 때까지 반복
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');

                //마스터, BGM, SFX 볼륨 불러오기
                if (values[0] == "Mouse")
                {
                    line = reader.ReadLine();
                    values = line.Split(',');
                    mouseSpeedWithXAxis = float.Parse(values[0]);
                    mouseSpeedWithYAxis = float.Parse(values[1]);
                }
            }
        }
    }
    #endregion
}