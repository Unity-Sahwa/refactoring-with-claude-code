using System.Collections;
using TMPro;
using UnityEngine;

public class EventTypingEffect : MonoBehaviour
{
    private TextMeshProUGUI typingText;

    [Header("타이핑 속도 (글자당 지연 시간)")]
    public float typingSpeed = 0.1f;

    [Header("스킵 가능 여부")]
    public bool allowSkip = true;

    [Header("텍스트가 사라지는 시간(초)")]
    public float timeBeforeDisappear = 3f;

    private bool isTyping = false;
    private bool skipTyping = false;
    private string savedText = ""; // 저장된 텍스트 내용

    private void OnEnable()
    {
        // TextMeshProUGUI 컴포넌트 가져오기
        typingText = GetComponent<TextMeshProUGUI>();

        if (typingText != null)
        {
            // 활성화될 때 저장된 텍스트 복원
            typingText.text = savedText;
            if (!isTyping)
            {
                StartCoroutine(TypingCoroutine());
            }
        }
    }

    void Update()
    {
        if (allowSkip && isTyping && Input.GetMouseButtonDown(0))
        {
            skipTyping = true;
        }
    }

    IEnumerator TypingCoroutine()
    {
        isTyping = true;
        skipTyping = false;

        // TextMeshProUGUI에 작성된 텍스트 가져오기
        string fullText = typingText != null ? typingText.text : "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            if (skipTyping)
            {
                typingText.text = fullText;
                break;
            }

            typingText.text = fullText.Substring(0, i); // 부분적으로 텍스트를 잘라서 표시
            yield return new WaitForSeconds(typingSpeed);
        }

        // 타이핑이 완료된 후 지정된 시간만큼 대기
        yield return new WaitForSeconds(timeBeforeDisappear);

        if (typingText != null)
        {
            typingText.text = "";
        }

        isTyping = false; // 타이핑 완료

        // 텍스트 내용 저장 및 부모 비활성화
        savedText = fullText;
        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
        }
    }
}

