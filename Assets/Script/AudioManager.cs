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
    public Button musicButton;
    public Button muteButton;

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
    }



    void FindUIReferences()
    {
        // Only search for references if they're not assigned
        if (musicButton == null || muteButton == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                Transform mainMenuPanel = canvas.transform.Find("Start_Main_Menu_Panel");
                if (mainMenuPanel != null)
                {
                    // Look for music_button
                    Transform musicBtnTransform = mainMenuPanel.Find("music_button");
                    if (musicBtnTransform != null)
                    {
                        musicButton = musicBtnTransform.GetComponent<Button>();
                        if (musicButton != null)
                        {
                            // Add click listener if not already added
                            musicButton.onClick.RemoveAllListeners();
                            musicButton.onClick.AddListener(EnableMusic);
                        }
                    }

                    // Look for mute_button
                    Transform muteBtnTransform = mainMenuPanel.Find("mute_button");
                    if (muteBtnTransform != null)
                    {
                        muteButton = muteBtnTransform.GetComponent<Button>();
                        if (muteButton != null)
                        {
                            // Add click listener if not already added
                            muteButton.onClick.RemoveAllListeners();
                            muteButton.onClick.AddListener(DisableMusic);
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

    public void EnableMusic()
    {
        musicEnabled = true;
        PlayerPrefs.SetInt("MusicEnabled", 1);
        UpdateMusicState();
        Debug.Log("Music enabled");
    }

    public void DisableMusic()
    {
        musicEnabled = false;
        PlayerPrefs.SetInt("MusicEnabled", 0);
        StopMusic();
        Debug.Log("Music disabled");
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        // Update music volume if music source exists
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * globalVolume;
        }
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



    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}