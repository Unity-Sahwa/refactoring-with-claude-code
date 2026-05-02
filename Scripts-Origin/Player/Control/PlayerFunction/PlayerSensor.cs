using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSensor : MonoBehaviour
{
    private Player player; 
    
    private PlayerCommonData commonData;

    public bool canNotMoveforward {  get; private set; }
    public bool canNotMoveBackward {  get; private set; }

    private void Start()
    {
        player = Player.instance;
        commonData = PlayerCommonData.Instance;
    }

    private void Update()
    {
        DetectFrontCollider();
        DetectBackCollider();
    }
    public void DetectFrontCollider()
    {
        //트리거는 어떤 반응인지 확인하기
        Vector3 playerPosition = player.transform.position + player.transform.up;
        Debug.DrawRay(playerPosition, player.transform.forward * commonData.playerForwardSensorRange, Color.red);

        if (Physics.Raycast(playerPosition, player.transform.forward, commonData.playerForwardSensorRange, commonData.collisionLayer, QueryTriggerInteraction.Ignore))
        {


            canNotMoveforward = true;
        }
        else
        {
            canNotMoveforward = false;
        }
    }

    
    public void DetectBackCollider()
    {
        //트리거는 어떤 반응인지 확인하기
        Vector3 playerPosition = player.transform.position + player.transform.up;
        Debug.DrawRay(playerPosition, -player.transform.forward * commonData.playerBackwardSensorRange, Color.green);

        if (Physics.Raycast(playerPosition, -player.transform.forward, commonData.playerBackwardSensorRange, commonData.collisionLayer, QueryTriggerInteraction.Ignore))
        {
            canNotMoveBackward = true;
        }
        else
        {
            canNotMoveBackward = false;
        }
    }
}
