using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScript : MonoBehaviour
{
    //the purpose of this script is to always exist and always be unique while playing the game.
    //it's attatched to the main camera, as it's unique and will always exist

    [SerializeField] GameObject pauseMenu;

    //when things are loading in (specially the player), we want to make sure that it knows the game is restarting, to handle the restart logic
    public static bool restarting = false;

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
        SceneManager.LoadScene("Demo");
        restarting = true;
    }
}
