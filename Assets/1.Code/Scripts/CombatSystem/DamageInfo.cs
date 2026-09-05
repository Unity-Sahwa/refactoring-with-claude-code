using UnityEngine;

namespace Refactoring
{
    // 히트박스가 부딪힌 상대에게 넘기는 정보 묶음.
    // 값 전달 전용 DTO라 필드를 public으로 연다. 프로퍼티로 감싸도 숨길 상태가 없음.
    public struct DamageInfo
    {
        // 공격을 가한 주체
        public GameObject Damager;

        // 피해량
        public float Amount;

        // 부딪힌 지점(근사값)
        public Vector3 HitPoint;

        // 칠하는 색
        public InkColorType Color;

        // 잉크 스택값
        public float InkStack;
    }
}
