using System;
using UnityEngine;

namespace Refactoring
{
    // 이동 허용/회전 허용 구간 하나. MoveControl·RotateControl 두 카테고리가 같은 구조라 공용으로 쓴다.
    // 한 상태가 여러 구간을 가질 수 있어 배열로 상태 데이터에 등록된다.
    [Serializable]
    public class MotionControlDataEntry : IStartData, IMotionControl
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0, 1)] private float startProgress; // 이 진행률에 도달하면 허용을 켠다
        [SerializeField] [Range(0, 1)] private float duration;      // 시작부터 허용을 유지하는 진행률 길이
        [SerializeField] private bool untilEnd;                     // true면 duration 무시, Reset까지 유지

        //IStartData
        public float StartProgress => startProgress;

        public float Duration => duration;
        public bool UntilEnd => untilEnd;
    }
}
