using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    SoundManager sm;
    GameManager gm;

    void Start()
    {
        sm = SoundManager.instance;
        gm = GameManager.instance;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            gm.RemoveCollectible();
            sm.PlaySound("Collect");
            Destroy(transform.gameObject);
        }
    }
}
