using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class InputKeySettingUI : MonoBehaviour
{
    private SaveManager saveManager;

    //버튼별로 
    [Header("버튼이랑 텍스트 순서같도록")]
    [SerializeField] private Button[] inputKeyButton;
    private TextMeshProUGUI[] inputKeyButtonText = new TextMeshProUGUI[(int)KeyAction.KEYCOUNT];
    //private TextMeshProUGUI[] inputKeyText;

    public bool isEditingKey { get; private set; }
    public bool completeEditingKey { get; private set; }
    private int currentKeyIndex = -1; 

    private void Awake()
    {
        for (int i = 0; i < inputKeyButton.Length; i++)
        {
            //Closure 문제발생
            // 반복문에서 람다함수, delegate 함수 사용시 발생하는 문제
            // i값의 복사값을 사용하는 것이 아니라 i변수 그 자체를 참조, 반복문이 다 끝난 후 i값을 참조함
            // 해결을 위해선 i의 복사값을 직접 만들어주기
            
            int index = i;
            inputKeyButton[index].onClick.AddListener(()=> EditInputKey(index));

            inputKeyButtonText[i] = inputKeyButton[i].gameObject.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        saveManager = SaveManager.instance;

        for (int i = 0; i < (int)KeyAction.KEYCOUNT; i++)
        {
            //saveManager.InputKeys의 key 값을 inputKeyText에 적용
            //inputKeyText[i].text = $"{(KeyAction)i}";

            //saveManager.InputKeys의 value 값을 inputKeyButtonText 적용
            ChangeInputKeyButtonText(i);
        }
    }

    private void Update()
    {
        if (isEditingKey)
        {
            //편집중에 입력한 키코드 정보가 None 이 아닐 경우 변환
            if (DetectPressedKeyCode() != KeyCode.None)
            {
                //메뉴버튼이 마우스 좌클릭이면 매우 불편해짐
                if (currentKeyIndex == (int)KeyAction.MENU)
                {
                    if (DetectPressedKeyCode() == KeyCode.Mouse0)
                    {
                        return;
                    }
                }

                //조작키 변경
                saveManager.ChangeKeySetting(currentKeyIndex, DetectPressedKeyCode());
                saveManager.SaveInputKeyData();

                //UI 변경
                ChangeInputKeyButtonText(currentKeyIndex);

                currentKeyIndex = -1;
                isEditingKey = false;

                //메뉴 나가기랑 중복되지 않도록
                completeEditingKey = true;
            }
        }
    }

    public void CompleteEditingKey(bool complete)
    {
        completeEditingKey = complete;
    }

    private KeyCode DetectPressedKeyCode()
    {//현재 입력된 키코드 정보 반환
        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(kcode))
            {
                return kcode;
            }
        }
        return KeyCode.None;
    }

    private void EditInputKey(int index)
    {//버튼을 누르면 실행되는 함수
        isEditingKey = true;
        currentKeyIndex = index;
    }

    private void ChangeInputKeyButtonText(int index)
    {//버튼 텍스트 변경

        if (saveManager.InputKeys.ContainsKey((KeyAction)index))
        {
            string text = saveManager.InputKeys[(KeyAction)index].ToString();

            inputKeyButtonText[index].text = text;
        }
    }
}
