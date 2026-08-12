using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Refactoring
{
    // 테스트 전용: 처형 상태 SO(HFinishAttackState 등) 수치를 반복 조정할 때 쓴다.
    // 지정 키를 누르면 적·덧칠스택·입력게이트를 전부 무시하고 처형 상태로 바로 들어가고,
    // 처형(Execute) 신호가 오면 덧칠스택 조건 없이 주변 적에게 큰 데미지를 넣어 죽는 것까지 확인한다.
    // ponytail: 씬에 빈 오브젝트로 붙여 쓰고 튜닝 끝나면 지운다. 다른 시스템은 건드리지 않음.
    public class Test_FinishTrigger : MonoBehaviour
    {
        [SerializeField] private Key _key = Key.G;
        [SerializeField] private float _damageRadius = 20f;
        [SerializeField] private float _damage = 999f;
        [SerializeField] private LayerMask _enemyMask;

        [Inject] private IStateTriggerRaiser _triggerRaiser;
        [Inject(true)] private IPlayerStateEventSubscriber _eventSubscriber;

        private readonly Collider[] _hits = new Collider[32];
        private IDisposable _finishEventDisposable;

        private void Awake()
        {
            _finishEventDisposable = _eventSubscriber?.Register(StateEventCategory.Finish, HandleFinish);
        }

        private void OnDestroy()
        {
            _finishEventDisposable?.Dispose();
        }

        private void Update()
        {
            // 이 프로젝트는 Input System 패키지를 쓰므로 구 Input 클래스는 예외가 난다.
            if (Keyboard.current == null || !Keyboard.current[_key].wasPressedThisFrame)
            {
                return;
            }

            if (_triggerRaiser == null)
            {
                Debug.LogError("[Test_FinishTrigger] IStateTriggerRaiser 주입 실패. 씬에 상태머신이 있는지 확인.");
                return;
            }

            _triggerRaiser.RaiseTrigger(StateTriggerType.FinishAttack);
        }

        // 처형 신호(Execute)만 가로채 주변 적을 강제로 죽인다. Stun은 원래 경로가 알아서 처리한다.
        private void HandleFinish(IStartData data)
        {
            if (data is not FinishDataEntry entry || entry.Action != FinishActionType.Execute)
            {
                return;
            }

            int count = Physics.OverlapSphereNonAlloc(transform.position, _damageRadius, _hits, _enemyMask);
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].TryGetComponent(out IDamageable target))
                {
                    target.ApplyDamage(new DamageInfo { Damager = gameObject, Amount = _damage });
                }
            }
        }
    }
}
