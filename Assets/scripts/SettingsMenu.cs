using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volume;
    public TMP_Dropdown quality;
    public Toggle fullScreen;

    void Start()
    {

        //IMPORTANT:
        //make sure that the settings menu is active (in Unity) so the Start function is called and these values are updated
        //otherwise the player has to open the settings to have them applied, they won't be automatically applied on game load

        gameObject.SetActive(false);
        // load saved settings
        volume.onValueChanged.AddListener(SetVolume);
        quality.onValueChanged.AddListener(SetQuality);
        fullScreen.onValueChanged.AddListener(SetFullscreen);

        // load and apply saved settings
        volume.value = PlayerPrefs.GetFloat("Volume", 0.75f);
        quality.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        fullScreen.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        // apply the values on game load
        SetVolume(volume.value);
        SetQuality(quality.value);
        SetFullscreen(fullScreen.isOn);
    }

    // volume
    public void SetVolume(float volumeValue)
    {

        float vol = -99999;
        if (volumeValue > 0)
        {
            vol = Mathf.Log10(volumeValue) * 20;
        }
        audioMixer.SetFloat("MasterVolume", vol); // for dB scaling
        PlayerPrefs.SetFloat("Volume", volumeValue);
        PlayerPrefs.Save();
    }

    // quality level
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.Save();
    }

    // fullscreen mode
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // back to main menu
    public void CloseSettingsPanel()
    {
        gameObject.SetActive(false);
    }
}
