using System.Collections.Generic;
using UnityEngine;

public class DIContainerPrototype : MonoBehaviour
{
    private List<IInjectable> injectableClasses = new List<IInjectable>();

    private _UserInput userInput;
    private R_PlayerStateMachine playerStateMachine;

    private MVP_Presenter presenter;
    
    private void Awake()
    {
        FindInjectableClass();
        GetClasses();

        if (injectableClasses.Count > 0)
        {
            if (userInput) userInput.Inject(InjectList<IUserInputReceiver>());
            if(playerStateMachine) playerStateMachine.Initialize(InjectList<IPlayerComponent>());

            if (presenter)
            {
                var viewUpdatable = InjectList<IViewUpdatable>();
                var modelUpdatable = InjectList<IModelUpdatable>();

                if (viewUpdatable != null) presenter.InitializeViewUpdater(viewUpdatable);
                if (modelUpdatable != null) presenter.InitializeModelUpdater(modelUpdatable);
            }
        }
    }

    private void FindInjectableClass()
    {
        //비활성화된 오브젝트를 포함해서 정렬없이 모든 MonoBehaviour 클래스 들고 오기
        MonoBehaviour[] monoBehaviours;
        monoBehaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        //그 중에서 IInjectable 인터페이스 구현한 클래스 저장
        foreach (var buffer in monoBehaviours)
        {
            if (buffer is IInjectable injectable)
            {
                injectableClasses.Add(injectable);
            }
        }
    }
    private void GetClasses()
    {
        foreach (var buffer in injectableClasses)
        {
            if (buffer is _UserInput)
            {
                userInput = (_UserInput)buffer;
            }
            else if(buffer is R_PlayerStateMachine)
            {
                playerStateMachine = (R_PlayerStateMachine)buffer;
            }
            else if (buffer is MVP_Presenter)
            {
                presenter = (MVP_Presenter)buffer;
            }
        }
    }
    private List<T> InjectList<T>() where T : class
    {
        List<T> listBuffer = new List<T>();

        foreach (var buffer in injectableClasses)
        {
            if (buffer is T injectableClass)
            {
                listBuffer.Add(injectableClass);
            }
        }

        if (listBuffer.Count <= 0)
        {
            return null;
        }

        return listBuffer;
    }
}
