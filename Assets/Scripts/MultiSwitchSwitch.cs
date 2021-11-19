using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSwitchSwitch : MonoBehaviour
{
    public bool oneTime = false;
    public bool timing = false;
    public bool timingDefaultState = false; 
    public float timingLength = 5f;
    public float timingCount = 0f;
    public bool isOn;
    bool oneTimeActivate = false;
    public bool triggered;

    void Start()
    {   
    }

    void Update()
    {
        if(timing && isOn != timingDefaultState)
        {
            timingCount += Time.deltaTime;
            if (timingCount > timingLength) 
            {   
                timingCount = 0;
                isOn = !isOn;
                triggered = true;
            }
        }

        if (oneTimeActivate)
        {
            Destroy(transform.gameObject);
        }        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            if (timing && isOn == timingDefaultState || !timing)
            {
                triggered = true;
                isOn = !isOn;
                if (oneTime)
                {
                    oneTimeActivate = true;
                }
            }
            else if (timing && isOn != timingDefaultState)
            {
                timingCount = 0;
            }
        }
    }
}
