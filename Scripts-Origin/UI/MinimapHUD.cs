using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MinimapHUD : MonoBehaviour
{
    private Transform target;
    [SerializeField] private Camera minimapCamera;
    //[SerializeField] private GameObject minimapBackGround;

    private void FixedUpdate()
    {
        ShowMinimap();
    }

    public void ShowMinimap()
    {
        //성능 개선의 여지가 있는가?
        if (!PlayerController.instance.maskChange.CurrentMask) return;
        target = PlayerController.instance.maskChange.CurrentMask.transform;

        minimapCamera.transform.position = new Vector3(target.position.x, minimapCamera.transform.position.y, target.position.z);
        //minimapBackGround.transform.position 
        //    = new Vector3(target.position.x, minimapBackGround.transform.position.y, target.position.z);
    }

}
