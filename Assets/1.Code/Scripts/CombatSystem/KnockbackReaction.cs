using System;
using UnityEngine;

namespace Refactoring
{
    // 넉백 효과. 방향과 세기를 전달한다. 실제로 어떻게 미는지는 받는 쪽이 정한다.
    [Serializable]
    public class KnockbackReaction : IHitReaction
    {
        [SerializeField] private Vector3 direction;   // 넉백 방향(받는 쪽이 해석)
        [SerializeField] private float force;         // 넉백 세기

        public Vector3 Direction => direction;
        public float Force => force;
    }
}
