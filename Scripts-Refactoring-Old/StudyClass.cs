using System;
using UnityEngine;

public class StudyClass : Attribute //사용자 정의 Attribute 만들기 위해서는 상속받아야함
{
    [SerializeField] string attributeString; //이것 또한 Attribute

    public string Desc { get; }

    public StudyClass(string desc)
    {
        Desc = desc;
    }

    private void Start()
    {
        TestMethod();
    }


    [Obsolete("TestMethod는 더이상 사용하지 않습니다")]
    public void TestMethod()
    {
        Debug.Log("TestMethod");
    }
}
