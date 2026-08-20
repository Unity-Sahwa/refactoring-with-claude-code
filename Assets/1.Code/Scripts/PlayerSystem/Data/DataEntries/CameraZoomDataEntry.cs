using System;
using UnityEngine;

namespace Refactoring
{
    //StateRunner에서 PlayerCameraZoomHandler에게 전달되는 이벤트에서 사용되는 데이터 형태
    [Serializable]
    public class CameraZoomDataEntry : IStartData, IPlayerCameraZoom
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0f, 1f)] private float startProgress;
        [SerializeField] private float distanceScale = 1f;
        [SerializeField] private float zoomOutTime;
        [SerializeField] private float zoomHoldTime;
        [SerializeField] private float zoomInTime;

        public float StartProgress => startProgress;
        public float DistanceScale => distanceScale;
        public float ZoomOutTime => zoomOutTime;
        public float ZoomHoldTime => zoomHoldTime;
        public float ZoomInTime => zoomInTime;
    }
}
