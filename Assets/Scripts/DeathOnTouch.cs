using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathOnTouch : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            col.gameObject.GetComponent<Player>().ChangePlayerState(Player.PlayerState.DEAD);
        }
    }
}
