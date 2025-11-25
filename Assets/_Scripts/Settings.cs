using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings Manager - Handles volume sliders and other settings
/// Links UI sliders to AudioManager
/// </summary>
public class Settings : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;

    [Header("Volume Icons")]
    [SerializeField] private GameObject sfxOnIcon;
    [SerializeField] private GameObject sfxOffIcon;
    [SerializeField] private GameObject musicOnIcon;
    [SerializeField] private GameObject musicOffIcon;

    [Header("Volume Texts")]
    [SerializeField] private TMPro.TextMeshProUGUI sfxVolumeText;
    [SerializeField] private TMPro.TextMeshProUGUI musicVolumeText;

    void Start()
    {
        // Initialize slider values from AudioManager
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.GetSoundEffectsVolume();
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            UpdateSFXIcons(sfxVolumeSlider.value);
            UpdateSFXText(sfxVolumeSlider.value);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            UpdateMusicIcons(musicVolumeSlider.value);
            UpdateMusicText(musicVolumeSlider.value);
        }
    }

    /// <summary>
    /// Called by SFX volume slider OnValueChanged event
    /// </summary>
    public void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSoundEffectsVolume(value);
        UpdateSFXIcons(value);
        UpdateSFXText(value);
    }

    /// <summary>
    /// Called by Music volume slider OnValueChanged event
    /// </summary>
    public void SetMusicVolume(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        UpdateMusicIcons(value);
        UpdateMusicText(value);
    }

    private void UpdateSFXIcons(float volume)
    {
        bool isOn = volume > 0f;

        if (sfxOnIcon != null)
            sfxOnIcon.SetActive(isOn);

        if (sfxOffIcon != null)
            sfxOffIcon.SetActive(!isOn);
    }

    private void UpdateMusicIcons(float volume)
    {
        bool isOn = volume > 0f;

        if (musicOnIcon != null)
            musicOnIcon.SetActive(isOn);

        if (musicOffIcon != null)
            musicOffIcon.SetActive(!isOn);
    }

    private void UpdateSFXText(float volume)
    {
        if (sfxVolumeText != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(volume * 10).ToString();
        }
    }

    private void UpdateMusicText(float volume)
    {
        if (musicVolumeText != null)
        {
            musicVolumeText.text = Mathf.RoundToInt(volume * 10).ToString();
        }
    }
}