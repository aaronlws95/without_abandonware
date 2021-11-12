using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    Dropdown dropdown;
    GameManager gm;
    List<string> sceneNames;

    void Start()
    {
        sceneNames = new List<string>();
        gm = GameManager.instance;
        List<Dropdown.OptionData> optionDataList = new List<Dropdown.OptionData>();

        int counter = 0;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
            if(name != "StartMenu")
            {
                if (counter > gm.GetPlayerLevel())
                {
                    optionDataList.Add(new Dropdown.OptionData("Locked"));
                }
                else
                {
                    string displayName;
                    float time = gm.GetPlayerBestTime(counter);
                    if (time == 0)
                    {
                        displayName = name + " - --:--";
                    }
                    else 
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
                        displayName = name + " - " +  minStr + ":" + secStr;   
                    }
                    optionDataList.Add(new Dropdown.OptionData(displayName));
                    sceneNames.Add(name);
                }
                counter++;
            }
        }

        dropdown = GetComponent<Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(optionDataList);
    }

    public void PlayLevel()
    {
        gm.SetCurrentLevel(dropdown.value);
        SceneManager.LoadScene(sceneNames[dropdown.value]);
    }
}


