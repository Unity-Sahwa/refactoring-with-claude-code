using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Image[] baseImages;
    public Image[] overlapImages;

    public Image MaskHUD;
    public Sprite[] maskSprites;

    private float maxHP;

    public void UpdateMask(int maskIndex)
    {
        Debug.Log(maskIndex);
        if (maskIndex >= 0 && maskIndex < maskSprites.Length)
        {
            MaskHUD.sprite = maskSprites[maskIndex];
            MaskHUD.enabled = true;
        }
    }
}
