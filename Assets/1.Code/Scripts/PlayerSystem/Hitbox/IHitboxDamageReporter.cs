using UnityEngine;

namespace Refactoring
{
    // 핸들러가 히트박스를 켤 때 전투값과 공격자를 주입하는 통로.
    // Provider는 이 인터페이스로 감지 컴포넌트를 캐싱하고, 핸들러가 켤 때 Setup을 호출한다.
    public interface IHitboxDamageReporter
    {
        void Setup(IHitboxCombat combat, GameObject damager, int targetMask);
    }
}
