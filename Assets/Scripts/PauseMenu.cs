using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private static bool paused = false;
    [SerializeField]
    private GameObject pauseMenu = null;
    [SerializeField]
    private GameObject playCanvas = null;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && Player.allowMovement)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (paused)
        {
            Resume();
            playCanvas.SetActive(true);
        }
        else
        {
            Pause();
            playCanvas.SetActive(false);
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        paused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        paused = false;
    }

    public void ReturnToLevel()
    {
        GameManager.instance.ReturnToLevel();
        Resume();
    }
}
