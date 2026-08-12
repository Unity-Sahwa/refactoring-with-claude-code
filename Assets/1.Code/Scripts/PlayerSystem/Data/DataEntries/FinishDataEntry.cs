using System;
using UnityEngine;

namespace Refactoring
{
    public enum FinishActionType
    {
        Stun,      // 주변 적 모션 정지
        Execute,   // 확보된 처형 대상 처형(Enemy.Execution)
        GearOn,    // 처형 장비(낫/귀신탈) 켜고 평상시 장비 끄기
        GearOff,   // 원상복구
    }

    // 역할: 처형 상태에서 진행률 타이밍에 스턴/처형을 발사하는 일회성 이벤트 데이터.
    // IMotionControl을 구현하지 않으므로 구간이 아닌 한 번만 발행된다.
    [Serializable]
    public class FinishDataEntry : IStartData
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0, 1)] private float startProgress;
        [SerializeField] private FinishActionType action;

        public float StartProgress => startProgress;
        public FinishActionType Action => action;
    }
}
