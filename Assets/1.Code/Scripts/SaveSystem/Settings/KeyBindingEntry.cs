using System;

namespace Refactoring
{
    [Serializable]
    public struct KeyBindingEntry
    {
        // JsonUtility가 필드명을 그대로 키로 써서, 이름을 고치면 이미 저장된 키 설정을 못 읽는다.
        // 그래서 컨벤션(PascalCase)에서 벗어나 있어도 그대로 둔다.
        public string actionName;
        public int keyCode;
    }
}
