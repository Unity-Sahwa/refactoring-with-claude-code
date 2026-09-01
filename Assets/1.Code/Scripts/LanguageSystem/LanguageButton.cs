using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;

namespace Refactoring
{
    // 언어 버튼 하나. 언어 하나를 맡아서, 누르면 그 언어로 바꾼다.
    // 언어가 늘면 버튼을 하나 더 두고 거기서 그 언어를 고르면 된다.
    [RequireComponent(typeof(Button))]
    public class LanguageButton : MonoBehaviour
    {
        // 이 버튼이 맡은 언어.
        [SerializeField]
        private Language _target;

        [Preserve, Inject(true)] private ILanguageSettings _language;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (_language == null)
            {
                return;
            }

            _language.Current = _target;
        }
    }
}
