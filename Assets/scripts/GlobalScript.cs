using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScript : MonoBehaviour
{
    //the purpose of this script is to always exist and always be unique while playing the game.
    //it's attatched to the main camera, as it's unique and will always exist

    //when things are loading in (specially the player), we want to make sure that it knows the game is restarting, to handle the restart logic
    public static bool restarting = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            restartLevel();
        }
    }

    public void restartLevel()
    {
        SceneManager.LoadScene("Demo");
        restarting = true;
    }
}
