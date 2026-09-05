using UnityEngine;

namespace Refactoring
{
    // HitChannel이 타격 성공 시 실어보내는 정보 묶음.
    public struct HitReport
    {
        public GameObject Attacker;
        public GameObject Target;
        public Vector3 Point;
        public SoundType Sound;
    }
}
