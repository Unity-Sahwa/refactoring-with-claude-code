using UnityEngine;

namespace Refactoring
{
    // 책임: AudioPlayer가 쓰는 공통 수치값을 담아 둔다.
    [CreateAssetMenu(menuName = "Refactoring/Audio/AudioPlayerData")]
    public class AudioPlayerData : ScriptableObject
    {
        [SerializeField] private int _initialVoices = 8;

        public int InitialVoices => _initialVoices;
    }
}
