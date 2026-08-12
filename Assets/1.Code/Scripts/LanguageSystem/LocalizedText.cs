using TMPro;
using UnityEngine;

namespace Refactoring
{
    // 글자 오브젝트에 붙는 부품. 키 하나만 들고 있고, 언어가 바뀌면 자기 글자와 폰트를 갱신한다.
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        [TextKey][SerializeField] private string _key;

        [Inject(true)] private ILanguageSettings _language;

        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            // 꺼져 있는 동안 언어가 바뀌었을 수 있어서, 켜질 때도 한 번 갱신한다.
            if (_language != null)
            {
                _language.OnChanged += HandleLanguageChanged;
            }

            Refresh();
        }

        private void Start()
        {
            // 첫 프레임엔 주입이 아직 안 됐을 수 있어서 한 번 더 맞춘다.
            Refresh();
        }

        private void OnDisable()
        {
            if (_language != null)
            {
                _language.OnChanged -= HandleLanguageChanged;
            }
        }

        private void HandleLanguageChanged()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_language == null || _text == null)
            {
                return;
            }

            _text.text = _language.GetText(_key);

            TMP_FontAsset font = _language.GetFont();

            if (font != null)
            {
                _text.font = font;
            }
        }
    }
}
