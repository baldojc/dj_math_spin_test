using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private Slider slider;

    void Start()
    {
        slider = GetComponent<Slider>();

        if (slider != null)
        {
            // Set initial value from PlayerPrefs
            slider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);

            // Add listener to update volume when slider changes
            slider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void OnVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnEnable()
    {
        // When slider panel becomes visible, update the slider value
        if (slider != null && AudioManager.Instance != null)
        {
            slider.value = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        }
    }
}