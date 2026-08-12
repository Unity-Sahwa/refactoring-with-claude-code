using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

        [Inject(true)] private SaveSlotManager _slots;

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

            SceneManager.LoadScene(_nextScene);
        }
    }
}
