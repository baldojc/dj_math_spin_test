using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
        [HideInInspector] public AudioSource source;
    }

    public AudioClip menuMusic; // For Start_Menu, OperatorPicker, Difficulty panels
    public AudioClip gameplayMusic; // For all gamePanel_* scenes
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float fxVolume = 1.0f;

    public List<Sound> sounds = new List<Sound>();

    private AudioSource musicSource;
    private float globalVolume = 1f;
    private bool musicEnabled = true;
    private AudioClip currentMusicClip;

    // UI References
    [Header("UI References")]
    public GameObject volumeSliderPanel;
    public Slider volumeSlider;
    public Button musicToggleButton;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Find UI references if not set
        FindUIReferences();

        // Set up volume slider value
        if (volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
            volumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        // Hide volume slider panel initially
        if (volumeSliderPanel != null)
        {
            volumeSliderPanel.SetActive(false);
        }
    }

    void FindUIReferences()
    {
        // Only search for references if they're not assigned
        if (volumeSliderPanel == null || volumeSlider == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                Transform mainMenuPanel = canvas.transform.Find("Start_Main_Menu_Panel");
                if (mainMenuPanel != null)
                {
                    // Look for VolumeSliderPanel
                    volumeSliderPanel = mainMenuPanel.Find("VolumeSliderPanel")?.gameObject;
                    if (volumeSliderPanel != null)
                    {
                        volumeSlider = volumeSliderPanel.GetComponentInChildren<Slider>();
                    }

                    // Look for Music_Button
                    Transform musicButton = mainMenuPanel.Find("Music_Button");
                    if (musicButton != null)
                    {
                        musicToggleButton = musicButton.GetComponent<Button>();
                        if (musicToggleButton != null)
                        {
                            // Add click listener if not already added
                            musicToggleButton.onClick.RemoveAllListeners();
                            musicToggleButton.onClick.AddListener(ToggleVolumeSliderPanel);
                        }
                    }

                    break;
                }
            }
        }
    }

    void Initialize()
    {
        // Set up music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * globalVolume;

        // Set up sound effect sources
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume * fxVolume * globalVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        // Load saved preferences
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        fxVolume = PlayerPrefs.GetFloat("FXVolume", 1.0f);
        globalVolume = PlayerPrefs.GetFloat("GlobalVolume", 1f);

        // Initialize music
        UpdateMusicState();
    }

    private void UpdateMusicState()
    {
        if (musicEnabled)
        {
            if (currentMusicClip == null)
                PlayMenuMusic();
        }
        else
        {
            StopMusic();
        }
    }

    #region Volume Slider Panel

    public void ToggleVolumeSliderPanel()
    {
        if (volumeSliderPanel == null)
        {
            FindUIReferences();
            if (volumeSliderPanel == null) return;
        }

        bool isActive = volumeSliderPanel.activeSelf;
        volumeSliderPanel.SetActive(!isActive);

        // If showing the slider, update its value
        if (!isActive && volumeSlider != null)
        {
            volumeSlider.value = musicVolume;
        }
    }

    #endregion

    #region Music Control

    public void PlayMenuMusic()
    {
        if (!musicEnabled || currentMusicClip == menuMusic) return;

        currentMusicClip = menuMusic;
        musicSource.clip = menuMusic;
        musicSource.volume = musicVolume * globalVolume;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (!musicEnabled || currentMusicClip == gameplayMusic) return;

        currentMusicClip = gameplayMusic;
        musicSource.clip = gameplayMusic;
        musicSource.volume = musicVolume * globalVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusicClip = null;
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);

        UpdateMusicState();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * globalVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetFXVolume(float volume)
    {
        fxVolume = Mathf.Clamp01(volume);

        // Update all sound effects volumes
        foreach (Sound s in sounds)
        {
            s.source.volume = s.volume * fxVolume * globalVolume;
        }

        PlayerPrefs.SetFloat("FXVolume", fxVolume);
    }

    public void SetGlobalVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);

        // Update music volume
        musicSource.volume = musicVolume * globalVolume;

        // Update all sound effects volumes
        foreach (Sound s in sounds)
        {
            s.source.volume = s.volume * fxVolume * globalVolume;
        }

        PlayerPrefs.SetFloat("GlobalVolume", globalVolume);
    }

    #endregion

    #region Sound Effects

    public void PlaySound(string soundName, float pitch = 1f, float volume = 1f, float delay = 0f)
    {
        Sound s = sounds.Find(sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound not found: " + soundName);
            return;
        }

        s.source.pitch = pitch;
        s.source.volume = volume * fxVolume * globalVolume;

        if (delay > 0)
            s.source.PlayDelayed(delay);
        else
            s.source.Play();
    }

    #endregion

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}