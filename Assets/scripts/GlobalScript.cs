using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScript : MonoBehaviour
{
    //the purpose of this script is to always exist and always be unique while playing the game.
    //it's attatched to the main camera, as it's unique and will always exist

    [SerializeField] GameObject pauseMenu;

    public static bool restarting = false;

    void Start()
    {
        Time.timeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !pauseMenu.active)
        {
            Time.timeScale = 1;
            restartLevel();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            alternatePauseMenu();
        }
    }

    public void alternatePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.active);

        //if the game is paused
        if (pauseMenu.active)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void leaveGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void restartLevel()
    {
        restarting = true;
        SceneManager.LoadScene("Demo");
    }
}
