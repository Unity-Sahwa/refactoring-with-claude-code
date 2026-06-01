using System;
using System.Collections.Generic;
using UnityEngine;

namespace Refactoring
{
    [Serializable]
    public struct SOGroup
    {
        public string name;
        public List<ScriptableObject> so;

    }
    public class SOContainer : MonoBehaviour, IDataProvider
    {
        [SerializeField] private List<SOGroup> SOAssets;

        public List<ScriptableObject> ProvideData()
        {
            List<ScriptableObject> soList = new List<ScriptableObject>();
            foreach (var SOGroup in SOAssets)
            {
                foreach (var soData in SOGroup.so)
                {
                    soList.Add(soData);
                }
            }
            return soList;
        }
    }
}
