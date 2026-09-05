using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Scripting;

namespace Refactoring
{
    // 정해둔 씬을 부른다. 기믹 트리거가 Execute를 부르거나, 버튼에 붙이면 누를 때 바로 돈다.
    // Single 모드라 이전 씬은 자동으로 내려감(메모리 해제 직접 관리 안 해도 됨).
    public class EventSceneLoad : EventData
    {
        // 빌드 설정에 적힌 씬 번호. 세이브 데이터도 같은 번호를 쓴다.
        [SerializeField]
        private int _nextScene;

        // 새 게임 시작용. 켜면 씬을 띄우기 직전에 저장된 칸을 전부 지운다.
        // 확인창이 아니라 여기서 지우는 이유는, 도중에 그만두면 기록이 남아 있어야 하기 때문이다.
        [SerializeField]
        private bool _deleteAllSaves;

        // 씬을 부르기 전에 기다릴 시간. 페이드가 끝날 때까지 버는 용도라 EventUIFade의 시간과 손으로 맞춘다.
        [SerializeField]
        private float _delay;

        [Preserve, Inject(true)] private SaveSlotManager _slots;
        [Preserve, Inject(true)] private IHealthInfo _health;
        [Preserve, Inject(true)] private ICurrentCharacterProvider _character;

        // 씬을 넘겨 들고 갈 체력과 캐릭터. 새 씬의 GameStateSaver가 Start에서 꺼내 쓰고 비운다.
        //
        // static인 이유:
        // LoadScene에는 값을 실어 보낼 인자가 없다. 이 오브젝트는 씬과 함께 파괴돼서 보통 필드는 같이 사라진다.
        // static 필드는 클래스에 붙어 있어 씬이 바뀌어도 남는다. SlotLoadRunner._pending과 같은 방식이다.
        private static float? _pendingHp;
        private static PlayerCharacterType? _pendingType;

        // 도메인 리로드를 끄면 지난 판의 값이 딸려 온다. 그래서 시작할 때 손으로 비운다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ResetStatic()
        {
            _pendingHp = null;
            _pendingType = null;
        }

        // 새 씬이 꺼내 간다. 한 번 꺼내면 비워서, 그냥 뜬 다음 씬이 옛 값을 다시 먹지 않게 한다.
        public static bool TakePending(out float hp, out PlayerCharacterType type)
        {
            bool has = _pendingHp.HasValue && _pendingType.HasValue;

            hp = _pendingHp ?? 0f;
            type = _pendingType ?? default;

            _pendingHp = null;
            _pendingType = null;

            return has;
        }

        private void Awake()
        {
            // 버튼에 붙은 경우엔 스스로 연결한다. 인스펙터에서 On Click을 손으로 잇지 않아도 된다.
            if (TryGetComponent(out Button button))
            {
                button.onClick.AddListener(Execute);
            }
        }

        public override void Execute()
        {
            if (_deleteAllSaves)
            {
                _slots?.DeleteAll();
            }

            if (_delay > 0f)
            {
                StartCoroutine(LoadAfter());
            }
            else
            {
                LoadNow();
            }
        }

        // 메뉴에서는 timeScale이 0이라 Invoke가 영영 안 터진다. 그래서 실제 시간으로 센다.
        private IEnumerator LoadAfter()
        {
            yield return new WaitForSecondsRealtime(_delay);
            LoadNow();
        }

        private void LoadNow()
        {
            // 메뉴 버튼에 붙은 경우엔 플레이어가 없어서 둘 다 비어 있다. 그때는 넘길 것도 없다.
            if (_health != null && _character?.CurrentType != null)
            {
                _pendingHp = _health.Current;
                _pendingType = _character.CurrentType.Value;
            }

            SceneManager.LoadScene(_nextScene);
        }
    }
}
