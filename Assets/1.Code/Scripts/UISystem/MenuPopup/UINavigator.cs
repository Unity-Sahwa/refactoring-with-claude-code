using System.Collections.Generic;

namespace Refactoring
{
    public class UINavigator
    {
        private readonly Stack<IWindow> _windows = new Stack<IWindow>();

        public bool HasOpenWindow => _windows.Count > 0;

        // 맨 위에 열려 있는 창. 없으면 null.
        public IWindow Top => _windows.Count > 0 ? _windows.Peek() : null;

        public void Open(IWindow window)
        {
            if (window == null)
            {
                return;
            }

            _windows.Push(window);
            window.Open();
        }

        public void CloseTop()
        {
            if (_windows.Count == 0)
            {
                return;
            }

            IWindow top = _windows.Pop();
            top.Close();
        }
    }
}
