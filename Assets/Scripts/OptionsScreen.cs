using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class OptionsScreen: MonoBehaviour
{
    GameManager gm;
    GameObject controlsScreen;
    GameObject mainScreen;
    bool controlScreenActive = false;
    
    void Start()
    {
        gm = GameManager.instance;
        controlsScreen = transform.Find("ControlsScreen").gameObject;
        mainScreen = transform.Find("MainScreen").gameObject;
    }

    public void Activate()
    {
        mainScreen.SetActive(true);
    }

    public void Deactivate()
    {
        mainScreen.SetActive(false);
    }

    public void GoToStartMenu()
    {
        gm.ToggleOptionsMenu();
        SceneManager.LoadScene("StartMenu");
    }

    public void ToggleControlsScreen()
    {
        controlScreenActive = !controlScreenActive;
        controlsScreen.SetActive(controlScreenActive);
        mainScreen.SetActive(!controlScreenActive);
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
