using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Refactoring
{
    // 카메라 임펄스 한 번에 쓰이는 수치 묶음. Velocity는 z를 안 써서 2D 흔들림만 난다(멀미 방지).
    [Serializable]
    public struct ShakeData
    {
        public CinemachineImpulseDefinition.ImpulseShapes ImpulseShape;
        public float AmplitudeGain;
        public float FrequencyGain;
        public float Duration;
        public Vector2 Velocity;
    }
}
