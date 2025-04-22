using UnityEngine;
using System.Collections.Generic;

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

    public List<Sound> sounds = new List<Sound>();

    private AudioSource musicSource;
    private float globalVolume = 1f;
    private bool musicEnabled = true;
    private AudioClip currentMusicClip;

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
            s.source.volume = s.volume * globalVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        // Load saved preferences
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.6f);
        globalVolume = PlayerPrefs.GetFloat("GlobalVolume", 1f);

        PlayMenuMusic();
    }

    #region Music Control
    public void PlayMenuMusic()
    {
        if (!musicEnabled || currentMusicClip == menuMusic) return;

        currentMusicClip = menuMusic;
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayGameplayMusic()
    {
        if (!musicEnabled || currentMusicClip == gameplayMusic) return;

        currentMusicClip = gameplayMusic;
        musicSource.clip = gameplayMusic;
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

        if (musicEnabled)
        {
            // Resume current music type
            if (currentMusicClip == menuMusic) PlayMenuMusic();
            else if (currentMusicClip == gameplayMusic) PlayGameplayMusic();
        }
        else
        {
            StopMusic();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * globalVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
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
        s.source.volume = volume * globalVolume;
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