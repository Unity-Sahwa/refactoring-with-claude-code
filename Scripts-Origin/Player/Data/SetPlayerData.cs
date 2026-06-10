using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetPlayerData : MonoBehaviour
{
    [Header("더블클릭해서 확인하세요")]
    public TextAsset DataParameterGuide;

    [Space(20)]
    public scriptStruct[] playerData;

    [Space(20)]
    public scriptStruct cameraData;
    public GameObject defaultCamera;
    public GameObject lockOnCamera;

    [Space(20)]
    public scriptStruct[] enemyData;

    [Space(20)]
    public CheatMode cheatData;


    [System.Serializable]   
    public struct scriptStruct
    {
        public string name;
        public ScriptableObject Data;
    }
}
