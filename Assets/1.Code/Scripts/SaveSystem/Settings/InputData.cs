using System;
using System.Collections.Generic;

namespace Refactoring
{
    [Serializable]
    public class InputData : ISaveData
    {
        public const string FileName = "InputData";

        public List<KeyBindingEntry> KeyBindings;
        public float MouseSensitivity;
    }
}
