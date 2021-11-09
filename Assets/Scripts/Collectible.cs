using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    SoundManager sm;

    void Start()
    {
        sm = SoundManager.instance;
    }
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            sm.PlaySound("Collect");
            Destroy(transform.gameObject);
        }
    }
}
