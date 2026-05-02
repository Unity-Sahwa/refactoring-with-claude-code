using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class BaseDataSO : ScriptableObject
{
    public Dictionary<DataPropertyNameEnum, Action<object>> DataGroup
    {
        get => dataGroup;
    }
    private Dictionary<DataPropertyNameEnum, Action<object>> dataGroup = new Dictionary<DataPropertyNameEnum, Action<object>>();

    public delegate void UpdateDataDelegate(DataPropertyNameEnum property, object changedData);
    public event UpdateDataDelegate UpdateDataEvent;
    public void Initialize()
    {
        PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (PropertyInfo property in properties)
        {
            //Get,Set을 구현한 프로퍼티만 추가
            bool hasGetter = property.GetGetMethod() != null;
            bool hasSetter = property.GetSetMethod() != null;

            if (hasGetter && hasSetter)
            {
                object outEnum;

                if (Enum.TryParse(typeof(DataPropertyNameEnum), property.Name, out outEnum))
                {
                    Debug.Log($"{property.Name}의 Enum값 존재(O)");
                    dataGroup.Add((DataPropertyNameEnum)outEnum, (value) => { property.SetValue(this, value); });
                }
                else
                {
                    Debug.Log($"{property.Name}의 Enum값 존재(X)");
                }
            }
        }
    }

    public void UpdateDataFromView(DataPropertyNameEnum property, object changedData)
    {
        dataGroup[property].Invoke(changedData);
    }
    
    public void UpdateData(DataPropertyNameEnum property,  object changedData)
    {
        UpdateDataEvent.Invoke(property, changedData);
    }



}
