using System;
using UnityEngine;

namespace Refactoring
{
    // 히트박스가 부딪힌 상대에게 줄 전투값에 관한 정보.
    [Serializable]
    public class CombatInfo
    {
        [SerializeField] private float damage;
        [SerializeField] private InkColor color;
        [SerializeField] private float inkStack;
        [SerializeField] private AudioId hitSound;

        public float Damage => damage;
        public InkColor Color => color;
        public float InkStack => inkStack;
        public AudioId HitSound => hitSound;
    }
}
