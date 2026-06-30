using System;
using UnityEngine;

namespace Refactoring
{
    // 역할: 진행률 구간 동안 켜지는 공용 구간 데이터(입력차단·버퍼·이동·회전·슈퍼아머·무적에 공용)
    [Serializable]
    public class IntervalDataEntry : IStartData, IMotionControl
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
