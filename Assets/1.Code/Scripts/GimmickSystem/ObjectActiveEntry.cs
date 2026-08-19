using System;
using UnityEngine;

namespace Refactoring
{
    // EventSaveGame이 "이 오브젝트는 이 상태로 저장" 목록에 쓰는 한 줄.
    [Serializable]
    public struct ObjectActiveEntry
    {
        public GameObject Target;
        public bool Active;
    }
}
