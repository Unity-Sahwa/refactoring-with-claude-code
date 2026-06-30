using System;
using UnityEngine;

namespace Refactoring
{
    // 페이로드 없는 타이밍 on/off 구간 하나. 슈퍼아머·무적처럼 "정해진 진행률 동안 켜두는" 구간에 쓴다.
    // 한 상태가 여러 구간을 가질 수 있어 배열로 상태 데이터에 등록된다.
    [Serializable]
    public class TimingDataEntry : IStartData, IMotionControl
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0, 1)] private float startProgress; // 이 진행률에 도달하면 구간을 켠다
        [SerializeField] [Range(0, 1)] private float duration;      // 시작부터 구간을 유지하는 진행률 길이
        [SerializeField] private bool untilEnd;                     // true면 duration 무시, Reset까지 유지

        //IStartData
        public float StartProgress => startProgress;

        //IMotionControl
        public float Duration => duration;
        public bool UntilEnd => untilEnd;
    }
}
