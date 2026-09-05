using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: StateRunner가 PlayerCameraShakeHandler에게 실어 보내는 흔들기 데이터.
    [Serializable]
    public class CameraShakeDataEntry : IStartData, IPlayerCameraShake
    {
        [SerializeField] private string _name;
        [SerializeField] [Range(0f, 1f)] private float _startProgress;
        [SerializeField] private ShakeData _shake;

        public float StartProgress => _startProgress;
        public ShakeData Shake => _shake;
    }
}
