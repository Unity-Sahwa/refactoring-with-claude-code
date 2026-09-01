using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 책임: 문 앞 트리거에 들어온 플레이어에게 상호작용 UI를 띄우고, 상호작용 입력이 오면 타임라인을 재생한다.
    // 문 열기·캐릭터 회전·걷기·UI 끄기는 전부 타임라인 트랙이 처리한다. 여기선 재생 시작과 게임 상태 전환만 한다.
    public class InteractionTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameObject _promptUI;

        [SerializeField]
        private PlayableDirector _director;

        [Preserve, Inject(true)] private IInputPressedProvider _input;
        [Preserve, Inject(true)] private IGameStateController _gameState;

        private bool _isPlayerInside;

        private void Start()
        {
            _promptUI?.SetActive(false);
        }

        private void OnDisable()
        {
            HidePrompt();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isPlayerInside || other.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                return;
            }

            _isPlayerInside = true;
            _promptUI?.SetActive(true);

            if (_input != null)
            {
                _input.OnInputPressed += HandleInputPressed;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
            {
                return;
            }

            HidePrompt();
        }

        private void HidePrompt()
        {
            if (!_isPlayerInside)
            {
                return;
            }

            _isPlayerInside = false;
            _promptUI?.SetActive(false);

            if (_input != null)
            {
                _input.OnInputPressed -= HandleInputPressed;
            }
        }

        private void HandleInputPressed(InputActionType action)
        {
            if (action != InputActionType.Interaction)
            {
                return;
            }

            HidePrompt();
            _gameState?.Push(GameStateType.Cutscene);
            _director.stopped += HandleTimelineStopped;
            _director.Play();
            gameObject.SetActive(false);
        }

        private void HandleTimelineStopped(PlayableDirector director)
        {
            director.stopped -= HandleTimelineStopped;
            _gameState?.Pop(GameStateType.Cutscene);
        }
    }
}
