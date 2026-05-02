using Cinemachine;
using MirzaBeig.VolumetricFogLite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBossRoomCamera : EventData
{
    public override void Execute()
    {
        CameraController.instance.StartCoroutine(CameraController.instance.HoldDefaultCameraValue());
    }
}
