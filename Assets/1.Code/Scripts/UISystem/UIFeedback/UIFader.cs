using UnityEngine;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 전투 중이거나 화면에 적이 보이면 이 UI를 서서히 띄우고, 아니면 서서히 지운다.
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFader : MonoBehaviour
    {
        [Tooltip("완전히 나타나거나 사라지는 데 걸리는 시간(초)")]
        [SerializeField] private float _fadeTime = 0.3f;

        [Tooltip("조건이 꺼진 뒤에도 최소한 유지되는 시간(초)")]
        [SerializeField] private float _minHoldTime = 2f;

        private float _hideTimer;

        [Preserve, Inject] private ICurrentStateProvider _stateProvider;

        [Preserve, Inject] private ILockOnTargetDetector _targetDetector;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if (IsShowing())
            {
                _hideTimer = _minHoldTime;
            }
            else
            {
                _hideTimer -= Time.deltaTime;
            }

            float targetAlpha = _hideTimer > 0f ? 1f : 0f;

            // 목표값 쪽으로 매 프레임 조금씩만 이동. 도중에 조건이 뒤집혀도 방향만 바뀌어서 중복 재생 처리가 필요 없다.
            float step = Time.deltaTime / Mathf.Max(_fadeTime, 0.0001f);
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, step);
        }

        // 이동(Locomotion) 말고 다른 상태이거나, 화면에 적이 한 마리라도 잡히면 보여준다.
        private bool IsShowing()
        {
            if (_stateProvider != null && _stateProvider.CurrentState != PlayerStateType.Locomotion)
            {
                return true;
            }

            return _targetDetector != null && _targetDetector.Candidates.Count > 0;
        }
    }
}
