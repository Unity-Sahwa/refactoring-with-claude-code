using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEngine;

public interface IPopup
{
    public IPopupController PopupController { set; }
    public void Close(); //스택에서도 지워야함.
    public void Open(); //+위치 셋팅
}
                                                                