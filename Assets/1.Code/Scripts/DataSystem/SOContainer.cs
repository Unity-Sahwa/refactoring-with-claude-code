using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    public class SOContainer : MonoBehaviour, IDataProvider
    {
        [SerializeField] private List<ScriptableObject> SOAssets;

        public List<ScriptableObject> ProvideData()
        {
            List<ScriptableObject> soList = new List<ScriptableObject>();
            foreach (var SOAsset in SOAssets)
            {
                soList.Add(SOAsset);
            }
            return soList;
        }
    }
}
