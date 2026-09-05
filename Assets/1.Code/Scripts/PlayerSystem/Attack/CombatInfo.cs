using System;
using UnityEngine;

namespace Refactoring
{
    // 책임: 히트박스가 부딪힌 상대에게 줄 전투값 묶음.
    [Serializable]
    public class CombatInfo
    {
        [SerializeField] private float _damage;
        [SerializeField] private InkColorType _color;
        [SerializeField] private float _inkStack;
        [SerializeField] private SoundType _hitSound;

        public float Damage => _damage;
        public InkColorType Color => _color;
        public float InkStack => _inkStack;
        public SoundType HitSound => _hitSound;
    }
}
