using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageReaction : EventData
{
    [SerializeField] PlayerState playerState;
   
    Player player;

    private void Start()
    {
        player = Player.instance;
    }

    //피격, 사망 반응 추가
    public override void Execute()
    {
        playerState.ChangePlayerState(PlayerStateType.DEAD);
        playerState.ChangePlayerSubState(PlayerSubStateType.DEAD_FALL);
        player.StartCoroutine(player.CoDieAction());
    }
}
