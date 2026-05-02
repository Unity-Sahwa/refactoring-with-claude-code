using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WisuPhysicsAttack : MonoBehaviour
{
    public Player player;
    public float damage;
    private void Start()
    {     
        player = GameObject.FindWithTag("PlayerScript").GetComponent<Player>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {            
            if (player != null && !player.CheckDie())
            {
                var message = new DamageMessage();
                message.amount = damage;
                player.ApplyDamage(message);
            }
        }
    }
}
