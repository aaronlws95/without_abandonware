using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject optionsMenu;
    static GameObject _optionsMenu;
    static bool optionsMenuActive = false;
    static bool optionsMenuCreated = false;
    bool restartLevel = false;
    SoundManager sm;
    Player player;
    GameObject[] collectibles;
    int collectibleCount;
    string sceneName;
    static PlayerData playerData;
    static bool playerDataLoaded = false;
    int currentLevel = 0;
    static bool properStart = false;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError ("More than one GameManager in the scene.");
        }
        else 
        {
            instance = this;
        }
    }

    void Start()
    {
        sm = SoundManager.instance;
        sceneName = SceneManager.GetActiveScene().name;

        if (!optionsMenuCreated)
        {
            optionsMenuCreated = true;
            _optionsMenu = Instantiate(optionsMenu);
            _optionsMenu.name = "OptionsMenu";
            DontDestroyOnLoad(_optionsMenu);
        }

        if (sceneName != "StartMenu")
        {
            player = GameObject.Find("Player").GetComponent<Player>();
            collectibleCount = GameObject.FindGameObjectsWithTag("Collectible").Length;
        }
        else if (sceneName == "StartMenu")
        {   
            properStart = true;
        }

        playerData = new PlayerData(0);
        if (!playerDataLoaded)
        {
            LoadPlayerData();
        }
    }

    public int GetPlayerLevel()
    {
        return playerData.level;
    }

    public void SetPlayerLevel(int _level)
    {
        playerData.level = _level;
    }

    public void SavePlayerData()
    {
        SaveSystem.SavePlayer(playerData);        
    }

    public void SetCurrentLevel(int _level)
    {
        currentLevel = _level;
    }

    void LoadPlayerData()
    {
        PlayerData data = SaveSystem.LoadPlayer();
        if(data != null)
        {
            playerData = data;
        }
        playerDataLoaded = true;
    }        

    public void RemoveCollectible()
    {
        collectibleCount -= 1;
    }

    void Update()
    {
        if (sceneName != "StartMenu")
        {
            if ((player.playerState == Player.PlayerState.DEAD || Input.GetKeyDown(KeyCode.X)) && !restartLevel)
            {
                sm.PlaySound("Die");
                StartCoroutine(RestartLevel());
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleOptionsMenu();
            }

            if (collectibleCount == 0)
            {
                if (properStart)
                {
                    CompleteLevel();
                }
                else 
                {
                    StartCoroutine(RestartLevel());
                }
            }
        }
    }

    public void CompleteLevel()
    {
        if(currentLevel >= playerData.level)
        {
            playerData.level += 1;
            SavePlayerData();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ToggleOptionsMenu()
    {
        optionsMenuActive = !optionsMenuActive;
        _optionsMenu.SetActive(optionsMenuActive);
    }

    IEnumerator RestartLevel()
    {
        restartLevel = true;
        yield return new WaitForSeconds(1.0f);
        restartLevel = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
