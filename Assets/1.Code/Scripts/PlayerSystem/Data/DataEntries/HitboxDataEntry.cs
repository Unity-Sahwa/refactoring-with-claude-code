using System;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public class HitboxDataEntry : IStartData, IPlayerHitbox
    {
        [SerializeField] private string name;
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private HitboxShape shape;
        [SerializeField] private Vector3 shapeScalescale = Vector3.one;      // HitboxShape.cs 주석 참고
        [SerializeField] [Range(0,1)] private float startProgress;
        [SerializeField] private float duration;
        [SerializeField] private CombatInfo combat;

        //IStartData
        public float StartProgress => startProgress;

        //IPlayerHitbox
        public float Duration => duration;
        public Vector3 Position => position;
        public Vector3 Rotation => rotation;
        public HitboxShape Shape => shape;
        public Vector3 ShapeScale => shapeScalescale;
        public CombatInfo Combat => combat;
    }
}
