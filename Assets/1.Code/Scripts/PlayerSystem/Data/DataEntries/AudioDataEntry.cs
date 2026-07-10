using System;
using UnityEngine;

namespace Refactoring
{
    //StateRunner에서 PlayerAudioHandler에게 전달되는 이벤트에서 사용되는 데이터 형태
    [Serializable]
    public class AudioDataEntry : IStartData, IPlayerAudio
    {
        [SerializeField] private string name;
        [SerializeField] private AudioId id;
        [SerializeField] [Range(0f, 1f)] private float startProgress;
        [SerializeField] private bool untilFinish; // 상태 전환돼도 안 끊고 끝까지 둘지

        public float StartProgress => startProgress;
        public AudioId Id => id;
        public bool UntilFinish => untilFinish;
    }
}
