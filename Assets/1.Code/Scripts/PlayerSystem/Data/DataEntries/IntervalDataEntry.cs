using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 진행률 구간 동안 켜지는 공용 구간 데이터(입력차단·버퍼·이동·회전·슈퍼아머·무적에 공용)
    [Serializable]
    public class IntervalDataEntry : IStartData, IMotionControl
    {
        [SerializeField] private string _name;
        [Tooltip("이 진행률에 도달하면 허용을 켠다")]
        [SerializeField] [Range(0, 1)] private float _startProgress;
        [Tooltip("시작부터 허용을 유지하는 진행률 길이")]
        [SerializeField] [Range(0, 1)] private float _duration;
        [Tooltip("true면 _duration 무시, Reset까지 유지")]
        [SerializeField] private bool _untilEnd;

        // IStartData
        public float StartProgress => _startProgress;

        public float Duration => _duration;
        public bool UntilEnd => _untilEnd;
    }
}
