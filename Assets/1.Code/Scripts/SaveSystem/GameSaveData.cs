using System;

namespace Refactoring
{
    [Serializable]
    public class GameSaveData : ISaveData
    {
        public const string FileName = "GameSaveData";

        public int SceneIndex;
        public int ZoneIndex;
        public int MaskType;
        public SerializableVector3 PlayerPosition;
        public float Hp;
        public bool LightingState;
        public bool PostProcessState;
    }
}
