using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtGateTrigger : EventData
{
    public override void Execute()
    {
       
        //캐릭터가 this.gameObject를 바라보도록
        Vector3 targetPosition = new Vector3(this.transform.position.x, MaskChange.instance.CurrentMask.transform.position.y, this.transform.position.z);
        MaskChange.instance.CurrentMask.transform.LookAt(targetPosition);

        //문제: 타임라인이 끝나고 캐릭터가 타임라인 이전의 회전값으로 돌아감
        //해결: PlayerMovement의 Quarternion targetRoation을 강제로 변환해서 타임라인이 끝날때 캐릭터 회전값이 원위치되는 것을 방지
        PlayerMovement.instance.TargetRotation = MaskChange.instance.CurrentMask.transform.rotation;
    }
}
