using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class DjAudioManager : MonoBehaviour
{
    public static DjAudioManager Instance;

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

    public AudioClip menuMusic; 
    public AudioClip gameplayMusic; 
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float fxVolume = 1.0f;

    public List<Sound> sounds = new List<Sound>();

    private AudioSource musicSource;
    private float globalVolume = 1f;
    private bool musicEnabled = true;
    private AudioClip currentMusicClip;
    private bool wasPlayingBeforePause = false;

    private MusicType currentMusicType = MusicType.Menu;

   
    public enum MusicType
    {
        Menu,
        Gameplay
    }

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
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * globalVolume;

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume * fxVolume * globalVolume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

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
            {
               
                if (currentMusicType == MusicType.Menu)
                    PlayMenuMusic();
                else
                    PlayGameplayMusic();
            }
            else if (!musicSource.isPlaying && wasPlayingBeforePause)
            {
                musicSource.Play();
            }
        }
        else
        {
            StopMusic();
        }
    }

    #region Music Control

    public void PlayMenuMusic()
    {
        if (!musicEnabled) return;

        currentMusicType = MusicType.Menu;

        if (currentMusicClip != menuMusic)
        {
            currentMusicClip = menuMusic;
            musicSource.clip = menuMusic;
            musicSource.volume = musicVolume * globalVolume;
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
    }

    public void PlayGameplayMusic()
    {
        if (!musicEnabled) return;

        currentMusicType = MusicType.Gameplay;

        if (currentMusicClip != gameplayMusic)
        {
            currentMusicClip = gameplayMusic;
            musicSource.clip = gameplayMusic;
            musicSource.volume = musicVolume * globalVolume;
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
        else if (!musicSource.isPlaying)
        {
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            wasPlayingBeforePause = true;
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicEnabled && wasPlayingBeforePause)
        {
            musicSource.UnPause();
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
        currentMusicClip = null;
        wasPlayingBeforePause = false;
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

    public void ToggleMusic()
    {
        if (musicEnabled)
            DisableMusic();
        else
            EnableMusic();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

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

        foreach (Sound s in sounds)
        {
            s.source.volume = s.volume * fxVolume * globalVolume;
        }

        PlayerPrefs.SetFloat("FXVolume", fxVolume);
    }

    public void SetGlobalVolume(float volume)
    {
        globalVolume = Mathf.Clamp01(volume);

        musicSource.volume = musicVolume * globalVolume;

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