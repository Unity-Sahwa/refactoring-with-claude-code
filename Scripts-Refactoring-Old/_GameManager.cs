#region GameManager 하는 일
//  상태를 파악하고 상태에 맞게 입력 신호 전달
//  게임 상태관리
//  게임 상호작용 관리
//  Undone: GameManager 정확히 뭐하냐?
//  최상위 클래스 -> 다른 클래스에서 의존하지 않도록 만들어야 겠는데
//  사망, 재시작 구현, 
#endregion
using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;

public class _GameManager : Singleton<_GameManager>, IUserInputReceiver
{
    //이벤트 버스로 전역에서 일어나는 소식을 주고받게 만들기
    //https://wolstar.tistory.com/87?category=773508
    //https://www.youtube.com/watch?v=4_DTAnigmaQ
    //https://unialgames.tistory.com/entry/UnityTipEventBus

    public bool IsInputBlockedByMenu
    {
        get { return isInputBlockedByMenu; }
    }
    private bool isInputBlockedByMenu = false;

    public bool IsInputBlockedBySequence
    {
        get { return isInputBlockedBySequence; }
    }
    private bool isInputBlockedBySequence = false;

    public bool IsMobilePlatform
    {
        get
        {
            return isMobilePlatform;
        }
    }

    private bool isMobilePlatform = false;


    private void CheckIfPCPlatform()
    {
    }

    public void GetGameState()
    {
        //UI나 연출관리에서 신호 전달받음
    }

    public void GameOver()
    {
    }

    public void GamePause()
    {
    }

    public void GameRestart()
    {
    }

    public void DeliverInput(InputActionEnum keyAction, InputStateEnum inputState)
    {
    }

    List<InputActionEnum> IUserInputReceiver.DeliverInputActions()
    {
        //임시로 설정했고 하위 기능들에서 인터페이스를 통해 받아오도록 구현해야함.
        List<InputActionEnum> ListBuffer = new List<InputActionEnum>()
        { InputActionEnum.Menu,
          InputActionEnum.Up, InputActionEnum.Left, InputActionEnum.Down, InputActionEnum.Right };

        return ListBuffer;
    }
}
