using System;

namespace Refactoring
{
    public interface ICharacterSwapNotifier
    {
        public event Action OnCharacterSwapped;
    }
}