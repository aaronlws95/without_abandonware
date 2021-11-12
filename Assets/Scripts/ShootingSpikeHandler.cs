using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingSpikeHandler : MonoBehaviour
{
    public GameObject shootingSpike;
    GameObject _shootingSpike;
    public float fireRate = 1f;
    public float speed = 8f;
    float counter;

    void Update()
    {
        counter += Time.deltaTime;

        if(counter > fireRate)
        {
             _shootingSpike = Instantiate(shootingSpike, transform.position, transform.rotation);
            _shootingSpike.GetComponent<Rigidbody2D>().velocity = transform.up * speed;
            counter = 0;
        }
    }
}
