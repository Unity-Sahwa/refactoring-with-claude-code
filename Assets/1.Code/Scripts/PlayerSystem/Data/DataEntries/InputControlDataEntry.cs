using System;
using UnityEngine;

namespace Refactoring
{
    // 입력 차단(InputBlock) / 선입력 저장(InputBuffer) 구간 하나. 두 카테고리가 같은 구조라 공용으로 쓴다.
    // 한 상태가 여러 구간을 가질 수 있어 배열로 상태 데이터에 등록된다.
    [Serializable]
    public class InputControlDataEntry : IStartData, IMotionControl
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
