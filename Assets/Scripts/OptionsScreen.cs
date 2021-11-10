using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsScreen: MonoBehaviour
{
    public void GoToStartMenu()
    {
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
