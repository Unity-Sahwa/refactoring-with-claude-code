using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Refactoring
{
    // 책임: StateRunner가 PlayerAudioHandler에게 실어 보내는 소리 데이터.
    [Serializable]
    public class AudioDataEntry : IStartData, IPlayerAudio
    {
        [SerializeField] private string _name;
        [SerializeField] private SoundType _id;
        [SerializeField] [Range(0f, 1f)] private float _startProgress;
        [Tooltip("상태 전환돼도 안 끊고 끝까지 둘지")]
        [SerializeField] private bool _untilFinish;

        public float StartProgress => _startProgress;
        public SoundType Id => _id;
        public bool UntilFinish => _untilFinish;
    }
}
