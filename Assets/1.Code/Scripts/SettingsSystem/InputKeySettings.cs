using System;

namespace Refactoring
{
    [Serializable] public class InputKeySettingsData : ISaveData
    {
        public const string FileName = "InputKeySettingsData";

        public string Bindings;
    }

    public class InputKeySettings : SettingsHolder<InputKeySettingsData>, IInputKeySettings
    {
        public string Bindings
        {
            get => Data.Bindings;
            set
            {
                Data.Bindings = value;
                NotifyChanged();
            }
        }
    }
}
