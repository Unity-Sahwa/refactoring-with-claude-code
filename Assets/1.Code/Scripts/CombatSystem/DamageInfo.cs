using UnityEngine;

namespace Refactoring
{
    // 히트박스가 부딪힌 상대에게 넘기는 정보 묶음.
    // 히트박스는 상대가 무엇인지 모르고, 이 정보만 전달한다.
    public struct DamageInfo
    {
        public GameObject Damager;          // 공격을 가한 주체
        public float Amount;                // 피해량
        public Vector3 HitPoint;            // 부딪힌 지점(근사값)
        public InkColor Color;              // 칠하는 색
        public float InkStack;              // 잉크 스택값
    }
}
