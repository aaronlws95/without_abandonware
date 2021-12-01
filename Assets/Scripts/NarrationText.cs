using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NarrationText : MonoBehaviour
{
    // Start is called before the first frame update
    static bool isActive = true;
    static string sceneName = "";
    void Start()
    {
        var activesceneName = SceneManager.GetActiveScene().name;
        if (activesceneName != sceneName)
        {
            sceneName = activesceneName;
            isActive = true;
        }
        gameObject.SetActive(isActive);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKey && !Input.GetKey(KeyCode.Escape) && isActive)
        {
            gameObject.SetActive(false);
            isActive = false;
        }
    }
}
