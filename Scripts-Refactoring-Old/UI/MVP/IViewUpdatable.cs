using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IViewUpdatable
{

    public Dictionary<DataPropertyNameEnum, object> ViewUpdatableProp
    {
        get;
    }

    public void Initialize();
    public void GetData(DataPropertyNameEnum property, object data);

}
