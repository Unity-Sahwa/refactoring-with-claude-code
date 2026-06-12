using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    // 충돌 감지·정보 전달 확인용 더미. 받은 DamageInfo를 로그로 찍고, 부딪힌 위치를 Scene 뷰에 빨간 구체로 표시한다.
    [RequireComponent(typeof(Collider))]
    public class Test_DummyDamageable : MonoBehaviour, IDamageable
    {
        private readonly List<Vector3> _hitPoints = new(); // 부딪힌 위치 기록(Scene 뷰 표시용)

        public void ApplyDamage(DamageInfo info)
        {
            string damagerName = info.Damager != null ? info.Damager.name : "null";
            Debug.Log(
                $"[Dummy] 피격: damager={damagerName}, 피해={info.Amount}, 색={info.Color}, " +
                $"스택={info.InkStack}, 충돌점={info.HitPoint}", this);

            if (info.Reactions != null)
            {
                foreach (var effect in info.Reactions)
                {
                    Debug.Log($"  [Dummy] 효과: {effect.GetType().Name}", this);
                }
            }

            _hitPoints.Add(info.HitPoint);
        }

        // Scene 뷰에서 부딪힌 위치들을 빨간 구체로 확인한다.
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            foreach (var point in _hitPoints)
            {
                Gizmos.DrawWireSphere(point, 0.1f);
            }
        }
    }
}
