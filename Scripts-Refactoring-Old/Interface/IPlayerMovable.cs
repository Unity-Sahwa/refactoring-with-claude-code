using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMovable : IPlayerComponent
{
    public Rigidbody PlayerRigidbody { get; }
    public CapsuleCollider PlayerCollider { get; }

    public Vector3 DirVector { get; }

    public void Inject(Rigidbody rb, CapsuleCollider collider) 
    {
        Debug.Log("구현되지 않았습니다.");
    }
    public void GetInput(Vector3 dirVector, PlayerMoveActionEnum actionEnum);
}
