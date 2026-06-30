using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Refactoring
{
    [Serializable]
    public class SkillEffectDataEntry : IStartData, IPlayerEffect
    {
        [SerializeField] private string name;
        [SerializeField] private bool untilFinish;
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotation;
        [SerializeField] private Vector3 scale;
        [SerializeField] [Range(0,1)] private float startProgress;
        [SerializeField] private float duration;
        [SerializeField] private AssetReferenceGameObject effectObject;
        [SerializeField] private EffectAttachPointType attachKey;
        [SerializeField] private bool stopInPlace;   // 도중에 부모에서 떨어져 그 자리 정지할지
        [SerializeField] private float stopTime;       // 멈추는 시점(초)

        //IStartData
        public float StartProgress => startProgress;

        //IPlayerEffect
        public bool UntilFinish => untilFinish;
        public float Duration => duration;
        public Vector3 Position => position;
        public Vector3 Rotation => rotation;
        public Vector3 Scale => scale;
        public AssetReferenceGameObject EffectObject => effectObject;
        public EffectAttachPointType AttachKey => attachKey;
        public bool StopInPlace => stopInPlace;
        public float StopTime => stopTime;
    }
}