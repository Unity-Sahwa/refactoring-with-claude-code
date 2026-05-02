using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class _UserInput : MonoBehaviour, IInjectable
{
    //입력을 제공할 대상들
    private List<IUserInputReceiver> inputReceivers = new List<IUserInputReceiver>();

    //어떤 대상에게 어떤 입력키 액션을 전달할지
    private Dictionary<InputActionEnum, List<IUserInputReceiver>> inputActions = new Dictionary<InputActionEnum, List<IUserInputReceiver>>();
    
    //클래스에 저장된 입력키 액션, 입력키코드
    private Dictionary<InputActionEnum, KeyCode> inputKeys = new Dictionary<InputActionEnum, KeyCode>();
    
    private bool holdingDown = false;
    private bool[] holdingDownArr = new bool[(int)InputActionEnum.Count];

    //public delegate void OnInputUpdated(KeyActionEnum inputkeys);    
    //public event OnInputUpdated OnInputUpdatedEvent;
    //public delegate void OnDirInputUpdated(Dictionary<KeyActionEnum, int> dirValue);
    //public event OnDirInputUpdated OnDirInputUpdatedEvent;

    private void Awake()
    {
        GetDefaultKeys();
    }

    private void Start()
    {
        GetInputAction();
    }

    private void Update()
    {
        if (Input.anyKey || holdingDown) SendInput();
    }
    private void GetDefaultKeys()
    {
        KeyCode[] defaultKeys = new KeyCode[] { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, 
                                                KeyCode.Space,
                                                KeyCode.Mouse0, KeyCode.Q, KeyCode.F,
                                                KeyCode.Mouse1,
                                                KeyCode.LeftShift, KeyCode.X,
                                                KeyCode.Escape};
        for (int i = 0; i < (int)InputActionEnum.Count; i++)
        {
            inputKeys.Add((InputActionEnum)i, defaultKeys[i]);
        }
    }
    public void Inject(List<IUserInputReceiver> receivers)
    {
        //<DIContainer>에서 inputReceivers 받아옴.
        inputReceivers = receivers;
    }
    private void GetInputAction()
    {
        //IUserInputReceiver.GetInputActions를 통해 받아온 Dictionary<InputKeyEnum, IUserInputReceiver> 합치기
        for (int i = 0; i < inputReceivers.Count; i++)
        {
            //해당하는 inputReceiver의 입력키 액션을 임시저장
            List<InputActionEnum> keyActionList = inputReceivers[i].DeliverInputActions();

            foreach (var item in keyActionList)
            {
                if (!inputActions.ContainsKey(item))
                {
                    inputActions.Add(item, new List<IUserInputReceiver> { inputReceivers[i] });
                }
                else
                {
                    inputActions[item].Add(this.inputReceivers[i]);
                }
            }
        }
    }

    // 인터페이스로 연결된 대상들에게 입력신호 전달
    private void SendInput()
    {
        for (int i = 0; i < inputKeys.Count; i++)
        {
            var key = inputKeys.ElementAt(i).Key; //KeyAction
            var keyValue = inputKeys.ElementAt(i).Value; //Keycode
            
            if (!inputActions.ContainsKey(key)) continue;
            List < IUserInputReceiver > inputReceiver = inputActions[key];

            //해당 키가 눌렸을 때 inputReceiver에 입력신호 전달
            if (Input.GetKeyDown(keyValue))
            {
                holdingDownArr[i] = true;
                foreach (var receiver in inputReceiver)
                {
                    receiver.DeliverInput(key, InputStateEnum.Down);
                }
            }

            //해당 키가 때졌을 때 inputReceiver에 입력신호 전달
            if (Input.GetKeyUp(keyValue))
            {
                holdingDownArr[i] = false;
                foreach (var receiver in inputReceiver)
                {
                    receiver.DeliverInput(key, InputStateEnum.Up);
                }
            }
        }

        //키가 눌려진 상태인지 체크
        holdingDown = holdingDownArr.Any(value => value == true);
    }
}