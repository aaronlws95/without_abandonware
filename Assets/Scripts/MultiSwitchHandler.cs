using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSwitchHandler : MonoBehaviour
{
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    public GameObject[] objects;
    public GameObject[] switches;
    public bool oneTime = false;
    public bool timing = false;
    public bool timingDefaultState = false; 
    public float timingLength = 5f;
    float timingCount = 0f;
    public bool isOn;
    bool oneTimeActivate = false;

    void Start()
    {   
        foreach (GameObject x in switches)
        {
            x.GetComponent<MultiSwitchSwitch>().isOn = isOn;
        }
    }

    void Update()
    {

        foreach (GameObject x in switches)
        {
            MultiSwitchSwitch curSwitch = x.GetComponent<MultiSwitchSwitch>();
            if (curSwitch.triggered)
            {
                isOn = curSwitch.isOn;
                curSwitch.triggered = false;
            }
            else 
            {
                curSwitch.isOn = isOn;
            }
        }

        if (isOn)
        {
            foreach (GameObject x in switches)
            {
                SpriteRenderer sr = x.GetComponent<SpriteRenderer>();
                sr.sprite = toggleOnSprite;
            }
            foreach (GameObject go in objects)
            {
                go.SetActive(true);
            }
        }
        else 
        {
            foreach (GameObject x in switches)
            {
                SpriteRenderer sr = x.GetComponent<SpriteRenderer>();
                sr.sprite = toggleOffSprite;
            }
            foreach (GameObject go in objects)
            {
                go.SetActive(false);
            }            
        }    
    }
}
