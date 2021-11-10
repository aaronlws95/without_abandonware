using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsScreen: MonoBehaviour
{
    GameManager gm;

    void Start()
    {
        gm = GameManager.instance;
    }

    public void GoToStartMenu()
    {
        gm.ToggleOptionsMenu();
        SceneManager.LoadScene("StartMenu");
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
