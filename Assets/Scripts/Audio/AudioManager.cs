using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {  get; private set; }

    float musicVolume;
    float soundEffectVolume;

    [SerializeField] SoundManagerSO soundManager;

    const string MUSICFLOATNAME = "musicVolume";
    const string SOUNDEFFECTFLOATNAME = "soundEffectVolume";


    AudioSource audioSource;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();


        if (!PlayerPrefs.HasKey(MUSICFLOATNAME))
        {
            PlayerPrefs.SetFloat(MUSICFLOATNAME, 1f);
        }
        if (!PlayerPrefs.HasKey(SOUNDEFFECTFLOATNAME))
        {
            PlayerPrefs.SetFloat(SOUNDEFFECTFLOATNAME, 0.5f);
        }
        SetMusicVolume(MUSICFLOATNAME, PlayerPrefs.GetFloat(MUSICFLOATNAME));
        SetSoundEffectVolume(SOUNDEFFECTFLOATNAME, PlayerPrefs.GetFloat(SOUNDEFFECTFLOATNAME));

    }


    public void SetMusicVolume(string floatName, float volume) 
    {
        musicVolume = volume;
        audioSource.volume = musicVolume;
        PlayerPrefs.SetFloat(floatName, musicVolume);
        SaveVolumes();

    }

    public void SetSoundEffectVolume(string floatName, float volume)
    {
        soundEffectVolume = volume;
        SaveVolumes();

    }


    void SaveVolumes()
    {
        
        PlayerPrefs.Save();

    }




    // Gameplay sounds
    public void PlayRockSounds()
    {
        audioSource.PlayOneShot(soundManager.boulderSound, soundEffectVolume);
    }

    public void PlayFootstepSound(GroundTileTypeEnum groundType)
    {
        AudioClip[] footstepSounds = null;
        int numberSoundClips = 0;
        switch (groundType) 
        {
            case GroundTileTypeEnum.Grass:
                footstepSounds = soundManager.footstepOnGrassSoundArray;
                numberSoundClips = footstepSounds.Length;
                break;
            case GroundTileTypeEnum.Stone:
                footstepSounds = soundManager.footstepOnStoneSoundArray;
                numberSoundClips = footstepSounds.Length;
                break;
            default: break;
        }
        if (numberSoundClips > 0 && footstepSounds != null)
        {
            int randomIndex = UnityEngine.Random.Range(0, numberSoundClips);
            audioSource.PlayOneShot(footstepSounds[randomIndex], soundEffectVolume);
            Debug.Log($"Sound played: footstepSounds #{randomIndex}");
        }
    }

    public void PlayHitSound()
    {
        audioSource.PlayOneShot(soundManager.hitSound, soundEffectVolume);
    }

    public void PlayPickupPointsSound()
    {
        audioSource.PlayOneShot(soundManager.pickupPointsSound, soundEffectVolume);
    }

    public void PlayPickupBigPointsSound()
    {
        audioSource.PlayOneShot(soundManager.pickupBigPointsSound, soundEffectVolume);
    }

    public void PlayPickupSpeedSound()
    {
        audioSource.PlayOneShot(soundManager.pickupSpeedSound, soundEffectVolume);
    }

    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(soundManager.jumpSound, soundEffectVolume);
    }

    // Button click sounds
    public void PlayButtonClick()
    {
        audioSource.PlayOneShot(soundManager.buttonClick, soundEffectVolume);
    }

    public void PlaySwitchButtonClick()
    {
        audioSource.PlayOneShot(soundManager.switchButtonClick, soundEffectVolume);
    }

    public void PlayCloseClick()
    {
        audioSource.PlayOneShot(soundManager.closeClick, soundEffectVolume);
    }

    public void PlayInvalidClick()
    {
        audioSource.PlayOneShot(soundManager.invalidClick, soundEffectVolume);
    }

    public void PlayPauseClick()
    {
        audioSource.PlayOneShot(soundManager.pauseClick, soundEffectVolume);
    }
    public void PlayResumeClick()
    {
        audioSource.PlayOneShot(soundManager.resumeClick, soundEffectVolume);
    }

    // Win/Lose sounds
    public void PlayLoseSound()
    {
        audioSource.PlayOneShot(soundManager.loseSound, soundEffectVolume);
    }

    public void PlayWinSound()
    {
        audioSource.PlayOneShot(soundManager.winSound, soundEffectVolume);
    }

    // Music
    public void PlayMainMenuMusic()
    {
        PlayMusic(soundManager.mainMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(soundManager.gameplayMusic);
    }

    public void PlayGameEndMusic()
    {
        PlayMusic(soundManager.gameEndMusic);
    }

    public void PlayScoreScreenMusic()
    {
        PlayMusic(soundManager.scoreScreenMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        audioSource.clip = clip;
        audioSource.volume = musicVolume;
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log(audioSource.clip + "" + musicVolume);
    }

    public void ChangeVolumeClick()
    {
        audioSource.PlayOneShot(soundManager.changeVolumeClick, soundEffectVolume);
    }

    public void PlayLowTimeAlert()
    {
        audioSource.PlayOneShot(soundManager.lowTimeAlertSound, soundEffectVolume);
    }

    public void PlayAddTimeSound()
    {
        audioSource.PlayOneShot(soundManager.addTimeSound, soundEffectVolume);
    }
}
