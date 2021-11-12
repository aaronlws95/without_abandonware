using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingSpike : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            col.gameObject.GetComponent<Player>().ChangePlayerState(Player.PlayerState.DEAD);
        }
        if (col.tag == "Player" || col.gameObject.layer == 6)
        {
            Destroy(transform.gameObject);
        }
    }
}
