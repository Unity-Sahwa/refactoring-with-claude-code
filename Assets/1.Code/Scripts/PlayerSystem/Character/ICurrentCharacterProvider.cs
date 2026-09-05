using UnityEngine;

namespace Refactoring
{
    // 현재(지금 움직일·표시할) 캐릭터의 부품을 제공한다. Mover·카메라·UI 등 "지금 캐릭터"를 알아야 하는 소비자가 쓴다.
    // 캐릭터 클래스 자체를 넘기지 않는다: 소비자가 캐릭터 구현을 알면 Character 시스템을 갈아 끼울 수 없다.
    // 스왑 통지(ICharacterSwapNotifier)와 분리한다: "지금 누구냐"(필수)와 "바뀌었다"(옵션)는 다른 관심사다.
    public interface ICurrentCharacterProvider
    {
        // 현재 캐릭터의 종류. 캐릭터가 없으면 null.
        public PlayerCharacterType? CurrentType { get; }

        // 현재 캐릭터가 가진 컴포넌트. Transform도 이걸로 받는다. 캐릭터·컴포넌트가 없으면 null.
        public T GetCurrentComponent<T>() where T : Component;
    }
}
