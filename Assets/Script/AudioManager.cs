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

    [Header("Music Settings")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    private AudioSource musicSource;

    [Header("Sound Effects")]
    public List<Sound> sounds = new List<Sound>();

    private float globalVolume = 1f;
    private bool musicEnabled = true;

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
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume * globalVolume;
        musicSource.loop = true;

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

        if (musicEnabled) PlayMusic();
    }

    #region Music Controls
    public void PlayMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);

        if (musicEnabled) PlayMusic();
        else StopMusic();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * globalVolume;
        }
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public bool IsMusicEnabled()
    {
        return musicEnabled;
    }
    #endregion

    #region Sound Effects
    public void PlaySound(string soundName, float pitch = 1f)
    {
        Sound s = sounds.Find(sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        s.source.pitch = pitch;
        s.source.Play();
    }

    public void StopSound(string soundName)
    {
        Sound s = sounds.Find(sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        s.source.Stop();
    }
    #endregion

    #region Global Controls
    public void SetGlobalVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        PlayerPrefs.SetFloat("GlobalVolume", globalVolume);
    }

    private void UpdateAllVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * globalVolume;
        }

        foreach (Sound s in sounds)
        {
            s.source.volume = s.volume * globalVolume;
        }
    }
    #endregion

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}