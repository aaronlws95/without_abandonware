using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NarrationText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKey && !Input.GetKey(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }
}
