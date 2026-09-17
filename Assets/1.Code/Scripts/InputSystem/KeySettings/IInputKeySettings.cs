using System;

namespace Refactoring
{
    public interface IInputKeySettings
    {
        event Action OnChanged;
        string Bindings { get; set; }
    }
}
