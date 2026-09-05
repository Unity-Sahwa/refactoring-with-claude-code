using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: 스킬 이동 조각 하나의 정보. (한 스킬이 여러 조각을 가질 수 있어 배열로 등록된다)
    [Serializable]
    public class SkillMoveDataEntry : IStartData, ISkillMove
    {
        [SerializeField] private string _name;
        [Tooltip("이 진행률에 도달하면 이동을 켠다")]
        [SerializeField] [Range(0, 1)] private float _startProgress;
        [Tooltip("켜고 끄기까지 지속시간(초)")]
        [SerializeField] private float _duration;
        [Tooltip("캐릭터 로컬 기준 이동 방향")]
        [SerializeField] private Vector3 _direction;
        [Tooltip("초당 이동 속도")]
        [SerializeField] private float _speed;

        // IStartData
        public float StartProgress => _startProgress;

        // ISkillMove
        public float Duration => _duration;
        public Vector3 Direction => _direction;
        public float Speed => _speed;
    }
}
