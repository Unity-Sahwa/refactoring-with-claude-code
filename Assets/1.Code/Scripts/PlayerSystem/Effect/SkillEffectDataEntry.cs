using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    [Serializable]
    public class SkillEffectDataEntry : IStartData, IPlayerEffect
    {
        [SerializeField] private string _name;
        [SerializeField] private bool _untilFinish;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _rotation;
        [SerializeField] private Vector3 _scale;
        [SerializeField] [Range(0,1)] private float _startProgress;
        [SerializeField] private float _duration;
        [SerializeField] private EffectId _effectId;
        [SerializeField] private EffectAttachPointType _attachKey;
        [Tooltip("도중에 부모에서 떨어져 그 자리 정지할지")]
        [SerializeField] private bool _stopInPlace;
        [Tooltip("멈추는 시점(초)")]
        [SerializeField] private float _stopTime;

        // IStartData
        public float StartProgress => _startProgress;

        // IPlayerEffect
        public bool UntilFinish => _untilFinish;
        public float Duration => _duration;
        public Vector3 Position => _position;
        public Vector3 Rotation => _rotation;
        public Vector3 Scale => _scale;
        public EffectId EffectId => _effectId;
        public EffectAttachPointType AttachKey => _attachKey;
        public bool StopInPlace => _stopInPlace;
        public float StopTime => _stopTime;
    }
}