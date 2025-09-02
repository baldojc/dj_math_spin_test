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
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float fxVolume = 0.5f;

    public List<Sound> sounds = new List<Sound>();

    private AudioSource musicSource;
    private float globalVolume = 1f;
    private AudioClip currentMusicClip;
    private bool wasPlayingBeforePause = false;

    private MusicType currentMusicType = MusicType.Menu;

    [Header("UI References")]
    public Slider musicSlider;
    public Slider fxSlider;
    public Button musicMuteButton;
    public Button fxMuteButton;

    private float lastMusicVolume = 0.5f;
    private float lastFXVolume = 0.5f;

    public float MusicVolume => musicVolume;
    public float FXVolume => fxVolume;

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

        // Only use volume for mute/unmute logic.
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        fxVolume = PlayerPrefs.GetFloat("FXVolume", 0.5f);
        globalVolume = PlayerPrefs.GetFloat("GlobalVolume", 1f);

        lastMusicVolume = musicVolume > 0 ? musicVolume : 0.5f;
        lastFXVolume = fxVolume > 0 ? fxVolume : 0.5f;

        UpdateMusicState();
        SetupSlidersAndButtons();
    }

    private void SetupSlidersAndButtons()
    {
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
            musicSlider.onValueChanged.AddListener(SetMusicVolumeFromSlider);
        }
        if (fxSlider != null)
        {
            fxSlider.value = fxVolume;
            fxSlider.onValueChanged.AddListener(SetFXVolumeFromSlider);
        }
        if (musicMuteButton != null)
        {
            musicMuteButton.onClick.AddListener(ToggleMusicMuteFromButton);
        }
        if (fxMuteButton != null)
        {
            fxMuteButton.onClick.AddListener(ToggleFXMuteFromButton);
        }
    }

    public void SetMusicVolumeFromSlider(float value)
    {
        SetMusicVolume(value);
        if (musicSlider != null && musicSlider.value != value)
            musicSlider.value = value;
    }

    public void SetFXVolumeFromSlider(float value)
    {
        SetFXVolume(value);
        if (fxSlider != null && fxSlider.value != value)
            fxSlider.value = value;
    }

    public void ToggleMusicMuteFromButton()
    {
        if (musicVolume > 0f)
        {
            MuteMusic();
            if (musicSlider != null) musicSlider.value = 0f;
        }
        else
        {
            UnmuteMusic();
            if (musicSlider != null) musicSlider.value = lastMusicVolume;
        }
    }

    public void ToggleFXMuteFromButton()
    {
        if (fxVolume > 0f)
        {
            MuteFX();
            if (fxSlider != null) fxSlider.value = 0f;
        }
        else
        {
            UnmuteFX();
            if (fxSlider != null) fxSlider.value = lastFXVolume;
        }
    }

    private void UpdateMusicState()
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

    #region Music Control

    public void PlayMenuMusic()
    {
        currentMusicType = MusicType.Menu;

        if (currentMusicClip != menuMusic)
        {
            currentMusicClip = menuMusic;
            musicSource.Stop();
            musicSource.clip = menuMusic;
            musicSource.time = 0f;
            musicSource.volume = musicVolume * globalVolume;
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
        else
        {
            musicSource.Stop();
            musicSource.time = 0f;
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
    }

    public void PlayGameplayMusic()
    {
        currentMusicType = MusicType.Gameplay;

        if (currentMusicClip != gameplayMusic)
        {
            currentMusicClip = gameplayMusic;
            musicSource.Stop();
            musicSource.clip = gameplayMusic;
            musicSource.time = 0f;
            musicSource.volume = musicVolume * globalVolume;
            musicSource.Play();
            wasPlayingBeforePause = true;
        }
        else
        {
            musicSource.Stop();
            musicSource.time = 0f;
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
        if (wasPlayingBeforePause)
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

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * globalVolume;
        }
        if (musicVolume > 0f)
            lastMusicVolume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void MuteMusic()
    {
        lastMusicVolume = musicVolume > 0f ? musicVolume : lastMusicVolume;
        SetMusicVolume(0f);
    }

    public void UnmuteMusic()
    {
        SetMusicVolume(lastMusicVolume > 0f ? lastMusicVolume : 0.5f);
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
        if (fxVolume > 0f)
            lastFXVolume = fxVolume;
        PlayerPrefs.SetFloat("FXVolume", fxVolume);
    }

    public void MuteFX()
    {
        lastFXVolume = fxVolume > 0f ? fxVolume : lastFXVolume;
        SetFXVolume(0f);
    }

    public void UnmuteFX()
    {
        SetFXVolume(lastFXVolume > 0f ? lastFXVolume : 0.5f);
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