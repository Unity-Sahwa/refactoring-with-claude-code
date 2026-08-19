using System;
using UnityEngine;

namespace Refactoring
{
    //StateRunner에서 PlayerCameraShakeHandler에게 전달되는 이벤트에서 사용되는 데이터 형태
    [Serializable]
    public class CameraShakeDataEntry : IStartData, IPlayerCameraShake
    {
        [SerializeField] private string name;
        [SerializeField] [Range(0f, 1f)] private float startProgress;
        [SerializeField] private ShakeData shake;

        public float StartProgress => startProgress;
        public ShakeData Shake => shake;
    }
}
