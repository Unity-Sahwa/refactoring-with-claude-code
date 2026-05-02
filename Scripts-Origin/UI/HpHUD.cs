using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpHUD : MonoBehaviour
{
    /// <summary>
    /// 체력 HUD 갯수를 체력에 맞춰 활성화 비활성화하도록 만듦
    /// </summary>
    public static HpHUD instance;
    
    Player player;

    [Header("순서대로 정렬하기")]
    [SerializeField] private GameObject[] hpStack;
    [SerializeField]  TMP_Text hpText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        player = Player.instance;
    }

    public void ChangeHPStack(int currentHP)
    {
        if (currentHP <= 0)
        {
            currentHP = 0;
        }

        for (int i = 0; i < currentHP; i++)
        {
            hpStack[i].SetActive(true);
        }
        for (int i = currentHP; i < hpStack.Length; i++)
        {
            hpStack[i].SetActive(false);
        }

        hpText.text = currentHP + "/" + hpStack.Length;
    }
}
