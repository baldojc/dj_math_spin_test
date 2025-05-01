using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider fxSlider;
    [SerializeField] private GameObject musicSliderPanel;
    [SerializeField] private GameObject fxSliderPanel;

    private void Start()
    {
        // Set initial values from PlayerPrefs
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (fxSlider != null)
        {
            fxSlider.value = PlayerPrefs.GetFloat("FXVolume", 1.0f);
            fxSlider.onValueChanged.AddListener(OnFXVolumeChanged);
        }

        // Hide panels initially
        if (musicSliderPanel != null)
            musicSliderPanel.SetActive(false);

        if (fxSliderPanel != null)
            fxSliderPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // When the panel becomes visible, update the slider values
        if (musicSlider != null && AudioManager.Instance != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        }

        if (fxSlider != null && AudioManager.Instance != null)
        {
            fxSlider.value = PlayerPrefs.GetFloat("FXVolume", 1.0f);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetFXVolume(value);
        }
    }

    public void ToggleMusicSliderPanel()
    {
        if (musicSliderPanel != null)
        {
            // Hide FX panel if it's visible
            if (fxSliderPanel != null && fxSliderPanel.activeSelf)
                fxSliderPanel.SetActive(false);

            // Toggle music panel
            musicSliderPanel.SetActive(!musicSliderPanel.activeSelf);
        }
    }

    public void ToggleFXSliderPanel()
    {
        if (fxSliderPanel != null)
        {
            // Hide music panel if it's visible
            if (musicSliderPanel != null && musicSliderPanel.activeSelf)
                musicSliderPanel.SetActive(false);

            // Toggle FX panel
            fxSliderPanel.SetActive(!fxSliderPanel.activeSelf);
        }
    }
}