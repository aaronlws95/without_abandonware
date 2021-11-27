using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateButtons : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject wallrun;
    GameObject bounce;
    GameObject reverse;
    public GameObject player;
    void Start()
    {
        wallrun = transform.Find("Wallrun").gameObject;
        bounce = transform.Find("Bounce").gameObject;     
        reverse = transform.Find("Reverse").gameObject;
        
        wallrun.GetComponent<Button>().onClick.AddListener(delegate{player.GetComponent<Player>().SetWallRun();});
        bounce.GetComponent<Button>().onClick.AddListener(delegate{player.GetComponent<Player>().SetBounce();});
        reverse.GetComponent<Button>().onClick.AddListener(delegate{player.GetComponent<Player>().SetReverse();});
    }
}
