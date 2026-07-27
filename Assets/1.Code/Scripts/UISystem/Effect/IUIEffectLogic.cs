using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: UIEffectLogic을 UIEffectRunner에게 제공하는 프레임
    public interface IUIEffectLogic
    {
        Type ConfigType { get; } // 어떤 UI 효과의 로직인가

        // 시작할 때 한 번. 원위치처럼 나중에 되돌릴 값을 여기서 기억함.
        void Begin(GameObject target, UIEffectConfig config);

        // 매 프레임 실행. progress(진행률)에 따라 UI ㅎ효과가 진행됨
        void Tick(GameObject target, UIEffectConfig config, float progress);

        // 끝날 때 한 번. Begin에서 기억한 값으로 되돌림.
        void End(GameObject target, UIEffectConfig config);
    }
}
