using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    public GameObject[] objects;
    public bool oneTime = false;
    public bool timing = false;
    public bool timingDefaultState = false; 
    public float timingLength = 5f;
    float timingCount = 0f;
    public bool isOn;
    bool oneTimeActivate = false;
    SpriteRenderer sr;
    SoundManager sm;
    public bool enableSound = false;

    void Start()
    {   
        sr = GetComponent<SpriteRenderer>();
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
            }
        }

        if (isOn)
        {
            sr.sprite = toggleOnSprite;
            foreach (GameObject go in objects)
            {
                go.SetActive(true);
            }
        }
        else 
        {
            sr.sprite = toggleOffSprite;
            foreach (GameObject go in objects)
            {
                go.SetActive(false);
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
