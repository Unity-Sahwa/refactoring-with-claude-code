using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: StateRunner가 PlayerObjectToggleHandler에게 실어 보내는 켜기/끄기 데이터. (일회성이라 되돌리지 않는다)
    [Serializable]
    public class ObjectToggleDataEntry : IStartData, IPlayerObjectToggle
    {
        [SerializeField] private ToggleTargetKey _key;
        [Tooltip("이 진행률에 도달하면 실행")]
        [SerializeField] [Range(0, 1)] private float _startProgress;
        [Tooltip("true면 켜기, false면 끄기")]
        [SerializeField] private bool _activate;

        public ToggleTargetKey Key => _key;
        public float StartProgress => _startProgress;
        public bool Activate => _activate;
    }
}
