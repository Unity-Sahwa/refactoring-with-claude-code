using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IModelUpdatable
{
    public delegate void SendDataDelegate(DataPropertyNameEnum property, object data);
    public event SendDataDelegate SendDataEvent;

    public void AddEvent();
}
