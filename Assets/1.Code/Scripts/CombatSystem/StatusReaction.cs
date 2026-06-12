using System;
using UnityEngine;

namespace Refactoring
{
    // 상태이상 효과. 종류와 지속시간을 전달한다. 실제 적용은 받는 쪽이 한다.
    [Serializable]
    public class StatusReaction : IHitReaction
    {
        [SerializeField] private StatusType type;
        [SerializeField] private float duration;   // 지속시간(초)

        public StatusType Type => type;
        public float Duration => duration;
    }
}
