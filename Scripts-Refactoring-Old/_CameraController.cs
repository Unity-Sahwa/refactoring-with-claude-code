using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _CameraController : Singleton<_CameraController>
{
    private Camera defaultCamera;
    public Camera DefaultCamera
    {
        get { return defaultCamera; }
    }

    private void Awake()
    {
        defaultCamera = _GameObjectUtility.GetComponentDirectChildren<Camera>(gameObject);
    }
}
