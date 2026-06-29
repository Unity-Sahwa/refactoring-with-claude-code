namespace Refactoring
{
    // 현재(지금 움직일·표시할) 캐릭터를 제공한다. Mover·카메라·UI 등 "지금 캐릭터가 누구냐"를 알아야 하는 소비자가 쓴다.
    // 스왑 통지(ICharacterSwapNotifier)와 분리한다: "지금 누구냐"(필수)와 "바뀌었다"(옵션)는 다른 관심사다.
    public interface ICurrentCharacterProvider
    {
        public PlayerCharacter CurrentCharacter {get;}
    }
}
