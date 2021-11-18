using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class InGameDisplay : MonoBehaviour
{
    GameObject stats;
    GameObject timerObj;
    Stats statsText;
    public Text timer;

    GameObject nextButton;
    GameObject prevButton;
    void Start()
    {
        stats = transform.Find("Stats").gameObject;
        timerObj = transform.Find("Timer").gameObject;
        nextButton = transform.Find("Next").gameObject;
        prevButton = transform.Find("Prev").gameObject;
        statsText = stats.GetComponent<Stats>();
    }

    public void SetButtons(bool value)
    {
        nextButton.SetActive(value);
        prevButton.SetActive(value);
    }

    public void SetActive(bool value)
    {
        timerObj.SetActive(value);
    }

    public void ShowStats(bool value)
    {
        stats.SetActive(value);
    }

    public void UpdateStatsText(float _speed, float _velocityY, float _velocityX)
    {
        statsText.speed.text = "Speed: " + _speed.ToString("F4");
        statsText.velocityY.text = "Y Velocity: " + _velocityY.ToString("F4");
        statsText.velocityX.text = "X Velocity: " + _velocityX.ToString("F4");
    }

    public void UpdateTimer(float time)
    {

        int intTime = (int)time;
        int minutes = intTime / 60;
        int seconds = intTime % 60;
        float fraction = time * 1000;
        fraction = (fraction % 1000);

        timer.text = String.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
    }

}
