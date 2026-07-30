using System;

namespace Refactoring
{
    // 바꾼 조작키 창구. 설정창의 키 버튼이 쓰고, InputHub이 시작할 때 읽는다.
    public interface IInputKeySettings
    {
        event Action OnChanged;

        // 바꾼 키 전체를 담은 글자. 무슨 뜻인지는 입력 쪽만 안다.
        string Bindings { get; set; }
    }

    [Serializable]
    public class InputKeySettingsData : ISaveData
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
