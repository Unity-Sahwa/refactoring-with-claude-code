using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetSOData : MonoBehaviour
{
    public scriptStruct[] player;
    public scriptStruct[] enemy;
    public scriptStruct[] camera;
    public scriptStruct[] settings;

    [System.Serializable]   
    public struct scriptStruct
    {
        public string name;
        public ScriptableObject Data;
    }
}
