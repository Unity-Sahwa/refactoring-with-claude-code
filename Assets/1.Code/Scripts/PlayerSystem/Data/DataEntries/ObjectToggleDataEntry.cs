using System;
using UnityEngine;

namespace Refactoring
{
    // StateRunner에서 PlayerObjectToggleHandler에게 전달되는 이벤트에서 사용되는 데이터 형태.
    // 구간이 아니라 일회성 설정임 - StartProgress에 도달하면 activate 상태로 바꾸고 끝. 상태가 끝나도 되돌리지 않는다.
    [Serializable]
    public class ObjectToggleDataEntry : IStartData, IPlayerObjectToggle
    {
        [SerializeField] private ToggleTargetKey key;
        [SerializeField] [Range(0, 1)] private float startProgress; // 이 진행률에 도달하면 실행
        [SerializeField] private bool activate;                     // true면 켜기, false면 끄기

        public ToggleTargetKey Key => key;
        public float StartProgress => startProgress;
        public bool Activate => activate;
    }
}
