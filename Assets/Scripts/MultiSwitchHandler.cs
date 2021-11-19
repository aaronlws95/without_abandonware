using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiSwitchHandler : MonoBehaviour
{
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    public GameObject[] objects;
    public GameObject[] switches;
    public bool isOn;

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
            if (x != null)
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
        }

        if (isOn)
        {
            foreach (GameObject x in switches)
            {
                if (x != null)
                {
                    SpriteRenderer sr = x.GetComponent<SpriteRenderer>();
                    sr.sprite = toggleOnSprite;
                }
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
                if (x!=null)
                {
                    SpriteRenderer sr = x.GetComponent<SpriteRenderer>();
                    sr.sprite = toggleOffSprite;
                }
            }
            foreach (GameObject go in objects)
            {
                go.SetActive(false);
            }            
        }    
    }
}
