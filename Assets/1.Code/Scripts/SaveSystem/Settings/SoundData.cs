using System;

namespace Refactoring
{
    [Serializable]
    public class SoundData : ISaveData
    {
        public const string FileName = "SoundData";

        public float MasterVolume;
        public float BgmVolume;
        public float EnemySfxVolume;
        public float PlayerSfxVolume;
    }
}
