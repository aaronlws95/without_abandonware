using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationText : MonoBehaviour
{
    // Start is called before the first frame update
    static bool isActive = true;

    void Start()
    {
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
