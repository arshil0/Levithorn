using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScript : MonoBehaviour
{
    //the purpose of this script is to always exist and always be unique while playing the game.
    //it's attatched to the main camera, as it's unique and will always exist

    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject levelsContentPrefab;

    public static GameObject levelsContentObject;
    public static bool restarting = false;

    void Start()
    {
        levelsContentObject = GameObject.Find("Levels");
        Time.timeScale = 1;
        CollectibleManager.instance.UpdateExistingOranges();
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

        /*
        //understand which level the player is in, that is done by getting the current active camera and getting its name
        string currentLevel = GetComponent<Cinemachine.CinemachineBrain>().ActiveVirtualCamera.Name;

        GameObject levelContent = GameObject.Find("Levels/" + currentLevel);

        if (levelContent != null)
        {
            print(levelContent);
        }
        */

        //there should be only 1 player object at a time, but just in case.
        //find all player objects and delete them
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Destroy(player);
        }

        //re-instantiate the player (the player will get placed in the right position)
        Instantiate(playerPrefab);

        //destroy the levels object (which holds the initial objects positions)
        Destroy(levelsContentObject);

        //re-instantiate the level content
        levelsContentObject = Instantiate(levelsContentPrefab);



        //SceneManager.LoadScene("Demo");
    }
}
