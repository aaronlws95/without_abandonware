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
    SoundManager sm;
    public bool enableSound = false;
    void Start()
    {   
        sm = SoundManager.instance;
    }

    void Update()
    {
        if(timing && isOn != timingDefaultState)
        {
            timingCount += Time.deltaTime;
            if (enableSound)
            {
                sm.PlaySound("SwitchTiming");
            }
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
            if (enableSound)
            {
                sm.PlaySound("SwitchDestroy");
            }            
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
                if (enableSound)
                {
                    if(isOn)
                    {
                        sm.PlaySound("SwitchOn");
                    }
                    else 
                    {
                        sm.PlaySound("SwitchOff");
                    }
                }                

                if (oneTime)
                {
                    oneTimeActivate = true;
                }
            }
        }
    }
}
