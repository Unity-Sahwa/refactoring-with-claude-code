using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class R_HPlayerAnimation : MonoBehaviour, IPlayerComponent, IPlayerAnimatable
{
    public PlayerComponentEnum ComponentID
    {
        get
        {
            return componentID;
        }
    }
    private PlayerComponentEnum componentID = PlayerComponentEnum.AnimalAnimation;

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