using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IUserInputReceiver : IInjectable
{
    //어떤 키를 받았는지 + 눌렸는지 아닌지 
    public void DeliverInput(InputActionEnum inputAction, InputStateEnum inputState);

    //IUserInputReceiver를 가진 클래스의 하위 모듈이 어떤 액션을 취할지 모아서 UserInput에게 제공
    List<InputActionEnum> DeliverInputActions();
}
