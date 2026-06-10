using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

//Study 싱글톤 패턴
//싱글톤을 사용하는 이유
//  싱글톤을 가지는 매니저,컨트롤러,데이터(SO)에 접근이 쉬워진다.
//  인스턴스를 하나만 생성하기 때문에 관리가 쉬워진다.
//단점
//  여러 클래스에서 싱글톤 인스턴스를 사용하게 될 경우 결합도가 높아져 유지보수가 어려워짐. 코드가 얽히게 될 수 있음.
//  독립적으로 테스트 하기 어려워 진다.

//기존 싱글톤 개선
//  싱글톤을 클래스마다 반복작성 -> 싱글톤 클래스로 매니저,컨트롤러,데이터 클래스의 인스턴스 생성
//  기존에 Awake에서 인스턴스 생성 -> Awake, Start 순서에 구애받지 않도록 호출시 인스턴스 생성되도록 구현.
//  의존성 주입 문제
//  매니저, 컨트롤러 클래스에 유연하게 관리

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                SetupInstance();
            }

            return instance;
        }
    }

    private static void SetupInstance()
    {
        instance = FindAnyObjectByType<T>();
        if (instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = typeof(T).Name;
            instance = gameObj.AddComponent<T>();
            DontDestroyOnLoad(gameObj);
        }
    }
}
