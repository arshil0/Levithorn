using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsPanel;

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void startNewGame()
    {
        PlayerPrefs.DeleteAll();
        Player.lastCheckpointPosition = new Vector3(0f, 0f, 0f);
        TransitionPoint.mainCameraFocusLevel = 1;
        SceneManager.LoadScene("Demo");
    }

    public void continueGame()
    {
        GlobalScript.restarting = true;
        SceneManager.LoadScene("Demo");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
