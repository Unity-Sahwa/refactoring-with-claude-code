using System;

// 역할: 이벤트 구독 해제
public class DisposeAction : IDisposable
{
    private Action _dispose;

    public DisposeAction(Action dispose)
    {
        _dispose = dispose;
    }

    //대원TODO: 이게 언제 호출되지? IDisposable을 구현한게
    public void Dispose()
    {
        _dispose?.Invoke();
        _dispose = null;
    }
}
