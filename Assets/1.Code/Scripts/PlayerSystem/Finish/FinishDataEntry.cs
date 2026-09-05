using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    public enum FinishActionType
    {
        // 주변 적 모션 정지
        Stun,
        // 확보된 처형 대상 처형(Enemy.Execution)
        Execute,
    }

    // 책임: 처형 상태에서 진행률 타이밍에 스턴·처형을 발사하는 일회성 데이터. (IMotionControl 미구현이라 한 번만 발행)
    [Serializable]
    public class FinishDataEntry : IStartData
    {
        [SerializeField] private string _name;
        [SerializeField] [Range(0, 1)] private float _startProgress;
        [SerializeField] private FinishActionType _action;

        public float StartProgress => _startProgress;
        public FinishActionType Action => _action;
    }
}
