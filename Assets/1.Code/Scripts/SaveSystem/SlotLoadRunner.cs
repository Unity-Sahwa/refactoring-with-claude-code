using UnityEngine;
using UnityEngine.SceneManagement;

namespace Refactoring
{
    // 불러오기 창의 창구. 슬롯 목록을 넘겨주고, 고른 칸으로 게임을 시작시킨다.
    // 씬을 띄우는 일과 값을 되돌리는 일의 순서만 맡는다. 파일도 플레이어도 직접 안 만진다.
    public class SlotLoadRunner : MonoBehaviour, ISaveSlots
    {
        [Inject(true)] private SaveSlotManager _slots;

        // 씬이 다 뜬 뒤에 값을 넣어야 해서, 넣을 데이터를 잠깐 들고 있는다.
        //
        // static인 이유:
        // 이 데이터는 씬을 띄우기 직전에 넣어두고, 새 씬이 다 뜬 뒤에 꺼내 쓴다. 씬 전환을 걸쳐야 하는 값이다.
        // 그런데 이 오브젝트는 씬이 바뀔 때 씬과 함께 파괴된다. 보통 필드에 담으면 같이 사라진다.
        // static 필드는 오브젝트가 아니라 클래스에 붙어 있어서 씬이 바뀌어도 남는다.
        // 그래서 씬을 띄우기 전의 SlotLoadRunner가 여기 넣어둔 값을, 새 씬에 생긴 SlotLoadRunner가 그대로 꺼내 쓴다.
        //
        // DontDestroyOnLoad로 오브젝트째 살리지 않는 이유:
        // AttributeInjector가 씬이 뜰 때마다 씬 안의 오브젝트를 전부 모아서 주입 대상으로 등록한다.
        // 오브젝트를 살려두면 살아남은 옛것과 새 씬에 놓인 사본이 둘 다 등록돼서,
        // "구현이 2개, 첫 번째만 주입" 경고가 뜨고 엉뚱한 쪽이 주입될 수 있다.
        // 씬을 넘어 남아야 하는 건 오브젝트가 아니라 이 값 하나뿐이라, 값만 남기면 그 충돌이 아예 안 생긴다.
        private static GameSaveData _pending;

        // 이번 씬이 불러오기(낙사 복귀 포함)로 떴는지. 씬을 그냥 넘어온 것과 구별하려고 둔다.
        // 씬 시작용 연출(EventStartView 등)은 불러오기 때 자리를 도로 덮으므로 이걸 보고 스스로 건너뛴다.
        // sceneLoaded가 Start보다 먼저라, Start에서 읽으면 이미 값이 정해져 있다.
        public static bool LoadedFromSlot { get; private set; }

        // static은 게임을 껐다 켜야 지워진다. 에디터에서 도메인 리로드를 끄면 지난 판의 값이 그대로 딸려 온다.
        // 그래서 게임이 시작될 때 손으로 비운다. AttributeInjector.CollectType도 같은 이유로 같은 짓을 한다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void ResetStatic()
        {
            _pending = null;
            LoadedFromSlot = false;
        }

        private void OnEnable()
        {
            // 씬이 뜨는 차례는 (옛 씬 파괴) → (새 씬 오브젝트 Awake/OnEnable) → sceneLoaded 순이다.
            // 그래서 새 씬에 생긴 이 오브젝트가 여기서 구독해도 자기 씬의 sceneLoaded를 놓치지 않는다.
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        public SaveSlotInfo[] GetSlots()
        {
            return _slots != null ? _slots.GetSlots() : new SaveSlotInfo[0];
        }

        public void LoadSlot(int index)
        {
            GameSaveData data = _slots?.TakeSlot(index);

            if (data == null)
            {
                return;
            }

            // 불러오기는 저장된 씬이 지금 씬과 같아도 반드시 새로 띄운다.
            // 자리만 되돌리면 이미 죽인 적이나 이미 지난 기믹이 그대로 남아서, 저장했을 때의 판이 아니게 된다.
            _pending = data;
            SceneManager.LoadScene(data.SceneIndex);
        }

        // 낙하 복귀용. 이쪽은 판을 되감는 게 아니라 떨어진 사람만 되돌리는 것이라 씬을 다시 띄우지 않는다.
        public void Begin(GameSaveData data)
        {
            // 같은 씬이면 새로 띄울 것 없이 바로 되돌린다.
            if (data.SceneIndex == SceneManager.GetActiveScene().buildIndex)
            {
                FindStateSaver()?.Restore(data);
                return;
            }

            _pending = data;
            SceneManager.LoadScene(data.SceneIndex);
        }

        // 씬이 다 뜬 뒤라야 플레이어가 존재한다. 그전에 값을 넣으면 아무 일도 일어나지 않는다.
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoadedFromSlot = _pending != null;

            if (_pending == null)
            {
                return;
            }

            GameSaveData data = _pending;
            _pending = null;

            FindStateSaver()?.Restore(data);
        }

        // GameStateSaver는 씬마다 새로 생긴다. 필드로 들고 있으면 씬이 바뀐 뒤에 이미 파괴된 옛것을 가리킨다.
        // 그래서 들고 있지 않고 쓸 때마다 지금 씬에서 찾는다. 부르는 때가 불러오기와 낙하 복귀뿐이라 찾는 값이 안 아깝다.
        private GameStateSaver FindStateSaver()
        {
            return FindFirstObjectByType<GameStateSaver>(FindObjectsInactive.Include);
        }
    }
}
