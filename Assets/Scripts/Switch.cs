using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    public GameObject[] objects;
    public bool isOn;

    SpriteRenderer sr;

    void Start()
    {   
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
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
    }

    void OnTriggerEnter2D()
    {
        isOn = !isOn;
    }
}
