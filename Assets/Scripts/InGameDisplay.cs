using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameDisplay : MonoBehaviour
{
    GameObject stats;
    GameObject timerObj;
    Stats statsText;
    public Text timer;

    void Start()
    {
        stats = transform.Find("Stats").gameObject;
        timerObj = transform.Find("Timer").gameObject;
        statsText = stats.GetComponent<Stats>();
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
        float minutes = Mathf.Floor(time / 60);
        int seconds = Mathf.RoundToInt(time%60);
        string minStr = minutes.ToString();
        string secStr = seconds.ToString();
        if(minutes < 10) {
            minStr = "0" + minStr;
        }
        if(seconds < 10) {
            secStr = "0" + secStr;
        }        
        timer.text = minStr + ":" + secStr;
    }

}
