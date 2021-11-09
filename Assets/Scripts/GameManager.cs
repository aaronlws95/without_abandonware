using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject optionsMenu;
    static bool optionsMenuActive = false;
    static bool optionsMenuCreated = false;
    bool restartLevel = false;
    SoundManager sm;
    Player player;
    GameObject[] collectibles;
    int collectibleCount;

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
        if (!optionsMenuCreated)
        {
            optionsMenuCreated = true;
            optionsMenu = Instantiate(optionsMenu);
            optionsMenu.name = "OptionsMenu";
            DontDestroyOnLoad(optionsMenu);
        }
        else
        {
            optionsMenu = GameObject.Find("OptionsMenu");
        }

        player = GameObject.Find("Player").GetComponent<Player>();

        collectibleCount = GameObject.FindGameObjectsWithTag("Collectible").Length;
    }

    public void RemoveCollectible()
    {
        collectibleCount -= 1;
    }

    void Update()
    {
        if ((player.playerState == Player.PlayerState.DEAD || Input.GetKeyDown(KeyCode.X)) && !restartLevel)
        {
            StartCoroutine(RestartLevel());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            optionsMenuActive = !optionsMenuActive;
            optionsMenu.SetActive(optionsMenuActive);
        }

        if (collectibleCount == 0)
        {
            player.playerState = Player.PlayerState.DEAD;
        }
    }

    IEnumerator RestartLevel()
    {
        restartLevel = true;
        sm.PlaySound("Die");
        yield return new WaitForSeconds(1.0f);
        restartLevel = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
