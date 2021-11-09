using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings: MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();
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
