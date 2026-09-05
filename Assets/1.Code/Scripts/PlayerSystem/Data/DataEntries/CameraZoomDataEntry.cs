using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: StateRunner가 PlayerCameraZoomHandler에게 실어 보내는 줌 데이터.
    [Serializable]
    public class CameraZoomDataEntry : IStartData, IPlayerCameraZoom
    {
        [SerializeField] private string _name;
        [SerializeField] [Range(0f, 1f)] private float _startProgress;
        [SerializeField] private float _distanceScale = 1f;
        [SerializeField] private float _zoomOutTime;
        [SerializeField] private float _zoomHoldTime;
        [SerializeField] private float _zoomInTime;

        public float StartProgress => _startProgress;
        public float DistanceScale => _distanceScale;
        public float ZoomOutTime => _zoomOutTime;
        public float ZoomHoldTime => _zoomHoldTime;
        public float ZoomInTime => _zoomInTime;
    }
}
