using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    private Queue<TextData> textQueue = new Queue<TextData>();
    private bool isTyping = false;
    private bool skipTyping = false;
    private static TextManager _instance;
    
    public static TextManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TextManager>();
            }
            return _instance;
        }
    }

    public void AddTextToQueue(TextMeshProUGUI typingText, string fullText, float typingSpeed, float timeBeforeDisappear, bool allowSkip)
    {
        textQueue.Enqueue(new TextData(typingText, fullText, typingSpeed, timeBeforeDisappear, allowSkip));

        if (!isTyping)
        {
            StartCoroutine(ProcessTextQueue());
        }
    }

    //큐의 텍스트 데이터들을 처리하는 코루틴
    IEnumerator ProcessTextQueue()
    {
        while (textQueue.Count > 0)
        {
            TextData textData = textQueue.Dequeue();
            yield return StartCoroutine(TypingCoroutine(textData));
        }
    }

    //타이핑 효과 실행하는 코루틴
    IEnumerator TypingCoroutine(TextData textData)
    {
        isTyping = true;
        skipTyping = false;

        textData.typingText.text = "";

        for (int i = 0; i <= textData.fullText.Length; i++)
        {
            if (skipTyping)
            {
                textData.typingText.text = textData.fullText;
                break;
            }

            textData.typingText.text = textData.fullText.Substring(0, i);
            yield return new WaitForSeconds(textData.typingSpeed);
        }

        yield return new WaitForSeconds(textData.timeBeforeDisappear);

        textData.typingText.text = "";

        isTyping = false;
    }

    void Update()
    {
        if (isTyping && Input.GetMouseButtonDown(0))
        {
            skipTyping = true;
        }
    }
}

public class TextData
{
    public TextMeshProUGUI typingText;
    public string fullText;
    public float typingSpeed;
    public float timeBeforeDisappear;
    public bool allowSkip;

    public TextData(TextMeshProUGUI typingText, string fullText, float typingSpeed, float timeBeforeDisappear, bool allowSkip)
    {
        this.typingText = typingText;
        this.fullText = fullText;
        this.typingSpeed = typingSpeed;
        this.timeBeforeDisappear = timeBeforeDisappear;
        this.allowSkip = allowSkip;
    }
}
