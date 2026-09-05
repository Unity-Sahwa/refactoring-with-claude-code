using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    [Serializable]
    public class HitboxDataEntry : IStartData, IPlayerHitbox
    {
        [SerializeField] private string _name;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _rotation;
        [SerializeField] private HitboxShape _shape;
        [Tooltip("HitboxShape.cs 주석 참고")]
        [SerializeField] private Vector3 _shapeScale = Vector3.one;
        [SerializeField] [Range(0,1)] private float _startProgress;
        [SerializeField] private float _duration;
        [SerializeField] private CombatInfo _combat;

        // IStartData
        public float StartProgress => _startProgress;

        // IPlayerHitbox
        public float Duration => _duration;
        public Vector3 Position => _position;
        public Vector3 Rotation => _rotation;
        public HitboxShape Shape => _shape;
        public Vector3 ShapeScale => _shapeScale;
        public CombatInfo Combat => _combat;
    }
}
