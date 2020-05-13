using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private static bool paused = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.BackQuote))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (!paused)
        {
            Time.timeScale = 0f;
            paused = true;
        }
        else
        {
            Time.timeScale = 1f;
            paused = false;
        }
    }
}
