using System;
using UnityEngine;

namespace Refactoring
{
    // 이동 조각 하나의 정보. 한 스킬이 여러 조각을 가질 수 있어 배열로 상태 데이터에 등록된다.
    [Serializable]
    public class SkillMoveDataEntry : IStartData, ISkillMove
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0, 1)] private float startProgress; // 이 진행률에 도달하면 이동을 켠다
        [SerializeField] private float duration;                    // 켜고 끄기까지 지속시간(초)
        [SerializeField] private Vector3 direction;                 // 캐릭터 로컬 기준 이동 방향
        [SerializeField] private float speed;                       // 초당 이동 속도

        //IStartData
        public float StartProgress => startProgress;

        //ISkillMove
        public float Duration => duration;
        public Vector3 Direction => direction;
        public float Speed => speed;
    }
}
