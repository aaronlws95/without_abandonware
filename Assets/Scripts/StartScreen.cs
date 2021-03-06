using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreen : MonoBehaviour
{
    public GameObject optionsScreen;
    public GameObject startScreen;
    public GameObject playScreen;
    
    public void OpenStartScreen()
    {
        optionsScreen.GetComponent<OptionsScreen>().Deactivate();
        playScreen.SetActive(false);
        startScreen.SetActive(true);
    }

    public void OpenOptionsScreen()
    {
        optionsScreen.GetComponent<OptionsScreen>().Activate();
        startScreen.SetActive(false);
    }

    public void OpenPlayScreen()
    {
        playScreen.SetActive(true);
        startScreen.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}
