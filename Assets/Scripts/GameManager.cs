using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance;

    // Options
    public GameObject optionsMenu;
    static GameObject _optionsMenu;
    static OptionsScreen optionsScreen;
    static bool optionsMenuCreated = false;

    // In game display
    public GameObject inGameDisplay;
    InGameDisplay _inGameDisplay;
    public static bool showStats = false;
    
    // Timer
    float timer = 0;


    // Restart level
    bool restartLevel = false;
    bool completedLevel = false;

    // Sound
    SoundManager sm;

    // Player
    Player player;
    static bool playerDataLoaded = false;
    static int currentLevel = 0;
    static PlayerData playerData;

    // Collectibles for winning
    GameObject[] collectibles;
    int collectibleCount;

    string sceneName; // Handle StartMenu
    static bool properStart = false; // Only save level if played through start menu (for debugging)

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
            optionsScreen = _optionsMenu.GetComponent<OptionsScreen>();
            _optionsMenu.name = "OptionsMenu";
            DontDestroyOnLoad(_optionsMenu);
        }

        if (sceneName != "StartMenu")
        {
            player = GameObject.Find("Player").GetComponent<Player>();
            collectibleCount = GameObject.FindGameObjectsWithTag("Collectible").Length;
        }
        else 
        {   
            properStart = true;
        }

        // Load player data
        if (!playerDataLoaded)
        {
            playerData = new PlayerData(0, new List<float>{0f});
            LoadPlayerData();
        }

        // Make sure there is an event system for the UI
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }        

        inGameDisplay = Instantiate(inGameDisplay);
        _inGameDisplay = inGameDisplay.GetComponent<InGameDisplay>();
    }

    public void ClearData()
    {
        playerData = new PlayerData(0, new List<float>{0f});       
        SavePlayerData();
    }

    public int GetPlayerLevel()
    {
        return playerData.level;
    }

    public float GetPlayerBestTime(int idx)
    {
        return playerData.bestTimes[idx];
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
            _inGameDisplay.SetButtons(true);

            if (player.playerState == Player.PlayerState.ACTIVE)
            {
                _inGameDisplay.SetActive(true);
                _inGameDisplay.ShowStats(showStats);

                timer += Time.deltaTime;
                _inGameDisplay.UpdateTimer(timer);
                if (showStats)
                {
                    Vector2 curVelocity = player.GetVelocity();
                    _inGameDisplay.UpdateStatsText(curVelocity.magnitude, curVelocity.x, curVelocity.y);
                }
            }

            if ((player.playerState == Player.PlayerState.DEAD || Input.GetKeyDown(KeyCode.X)) && !restartLevel)
            {
                sm.PlaySound("Die");
                StartCoroutine(RestartLevel());
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleOptionsMenu();
            }

            // Win condition
            if (collectibleCount == 0 && !completedLevel)
            {
                completedLevel = true;
                sm.PlaySound("Win");
                player.ChangePlayerState(Player.PlayerState.WIN);
                if (properStart)
                {
                    StartCoroutine(CompleteLevel());
                }
                else 
                {
                    StartCoroutine(RestartLevel());
                }
            }
        }
    }

    IEnumerator CompleteLevel()
    {
        if(currentLevel == playerData.level)
        {
            playerData.level += 1;
            playerData.bestTimes[currentLevel] = timer;
            playerData.bestTimes.Add(0);
        }
        
        if (currentLevel < playerData.level - 1)
        {
            if (timer < playerData.bestTimes[currentLevel])
            {
                playerData.bestTimes[currentLevel] = timer;
            }
        }
        SavePlayerData();
        currentLevel++;
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void PrevLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }


    public void ToggleOptionsMenu()
    {
        if (!optionsScreen.IsActive())
        {
            Time.timeScale = 0;
            optionsScreen.Activate();
        }
        else 
        {
            optionsScreen.Deactivate();
            Time.timeScale = 1;
        }
    }

    IEnumerator RestartLevel()
    {
        restartLevel = true;
        yield return new WaitForSeconds(1.0f);
        restartLevel = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
