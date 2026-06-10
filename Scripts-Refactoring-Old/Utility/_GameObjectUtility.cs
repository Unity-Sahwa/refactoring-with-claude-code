using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class _GameObjectUtility 
{
    //0903
    //지정한 오브젝트의 1계층의 자식만 조사하는 GetComponent 함수
    //GetComponentInChildren은 깊이 탐색을 진행함.
    public static T GetComponentDirectChildren<T>(GameObject parent) where T : Component
    {
        foreach (Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();
            if (component != null)
            {
                return component;
            }
        }
        return null;
    }
}
