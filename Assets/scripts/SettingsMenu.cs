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
        // load saved settings
        volume.onValueChanged.AddListener(SetVolume);
        quality.onValueChanged.AddListener(SetQuality);
        fullScreen.onValueChanged.AddListener(SetFullscreen);

        // load and apply saved settings
        volume.value = PlayerPrefs.GetFloat("Volume", 0.75f); 
        quality.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel()); 
        fullScreen.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
    }

    // volume
    public void SetVolume(float volumeValue)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volumeValue) * 20); // for dB scaling
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
