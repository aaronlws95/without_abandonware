using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    string selectedLevel = "";
    Dropdown dropdown;
    GameManager gm;

    void Start()
    {
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
                    optionDataList.Add(new Dropdown.OptionData(name));
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
        SceneManager.LoadScene(dropdown.options[dropdown.value].text);
    }
}


