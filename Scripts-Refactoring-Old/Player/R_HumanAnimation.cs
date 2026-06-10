using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class R_HumanAnimation : MonoBehaviour, IPlayerComponent, IPlayerAnimatable
{
    public PlayerComponentEnum ComponentID
    {
        get
        {
            return componentID;
        }
    }
    private PlayerComponentEnum componentID = PlayerComponentEnum.HumanAnimation;

    public Animator PlayerAnimator
    {
        get
        {
            return playerAnimator;
        }
    }
    private Animator playerAnimator;


    private void Awake()
    {
        
    }

    private void Initialize()
    {
        playerAnimator = GetComponent<Animator>();
    }


}
