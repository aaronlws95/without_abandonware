using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsScreen: MonoBehaviour
{
    GameManager gm;
    GameObject controlsScreen;
    GameObject mainScreen;
    Toggle statsToggle;
    void Start()
    {
        gm = GameManager.instance;
        controlsScreen = transform.Find("ControlsScreen").gameObject;
        mainScreen = transform.Find("MainScreen").gameObject;
        statsToggle = mainScreen.transform.Find("Toggle").gameObject.GetComponent<Toggle>();
    }

    public void Activate()
    {
        mainScreen.SetActive(true);
        statsToggle.isOn = GameManager.showStats;
    }

    public void Deactivate()
    {
        mainScreen.SetActive(false);
    }

    public bool IsActive()
    {
        return mainScreen.activeSelf;
    }

    public void GoToStartMenu()
    {
        gm.ToggleOptionsMenu();
        SceneManager.LoadScene("StartMenu");
    }

    public void ToggleControlsScreen()
    {
        controlsScreen.SetActive(!controlsScreen.activeSelf);
        mainScreen.SetActive(!controlsScreen.activeSelf);
    }

    public void ToggleStats(bool value)
    {
        statsToggle.isOn = value;
        GameManager.showStats = value;   
    }

    public void SetVolumeSFX(float volume)
    {
        SoundManager.volumeSFX = volume/10;
    }

    public void SetVolumeBGM(float volume)
    {
        SoundManager.volumeBGM = volume/10;
    }    
}
