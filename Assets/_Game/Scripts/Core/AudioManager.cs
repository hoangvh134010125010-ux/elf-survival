using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;

    [Header("BGM")]
    public AudioClip defaultBGM;

    [Header("UI & SFX")]
    public AudioClip buttonClickSFX;
    public AudioClip buySuccessSFX;
    public AudioClip equipItemSFX;
    public AudioClip upgradeSuccessSFX;
    public AudioClip upgradeFailSFX;
    public AudioClip notificationSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (defaultBGM != null && Instance.musicSource != null && Instance.musicSource.clip == null)
            {
                Instance.PlayBGM(defaultBGM);
            }
            Destroy(gameObject);
            return;
        }

        EnsureAudioSources();

        sfxSource.spatialBlend = 0f; 
        musicSource.spatialBlend = 0f;
        voiceSource.spatialBlend = 0f;

        LoadVolumeSettings();
    }

    private void EnsureAudioSources()
    {
        AudioSource[] sources = GetComponents<AudioSource>();
        int needed = 3 - sources.Length;
        for (int i = 0; i < needed; i++)
        {
            gameObject.AddComponent<AudioSource>();
        }

        sources = GetComponents<AudioSource>();
        musicSource = sources[0];
        sfxSource = sources[1];
        voiceSource = sources[2];
    }

    private void Start()
    {
        if (defaultBGM != null)
        {
            PlayBGM(defaultBGM);
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.ignoreListenerPause = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayVoice(AudioClip clip)
    {
        if (voiceSource == null || clip == null) return;
        voiceSource.PlayOneShot(clip);
    }

    public void PlayButtonClick() => PlaySFX(buttonClickSFX);
    public void PlayBuySuccess() => PlaySFX(buySuccessSFX);
    public void PlayEquipItem() => PlaySFX(equipItemSFX);
    public void PlayUpgradeSuccess() => PlaySFX(upgradeSuccessSFX);
    public void PlayUpgradeFail() => PlaySFX(upgradeFailSFX);
    public void PlayNotification() => PlaySFX(notificationSFX);

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null) musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null) sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVol", volume);
    }

    public void SetVoiceVolume(float volume)
    {
        if (voiceSource != null) voiceSource.volume = volume;
        PlayerPrefs.SetFloat("VoiceVol", volume);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.6f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);
        float voiceVol = PlayerPrefs.GetFloat("VoiceVol", 1f);

        if (musicSource != null) musicSource.volume = musicVol;
        if (sfxSource != null) sfxSource.volume = sfxVol;
        if (voiceSource != null) voiceSource.volume = voiceVol;
    }
}