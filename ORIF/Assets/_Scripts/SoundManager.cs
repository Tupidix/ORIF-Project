using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager responsible for playing sound effects and background music.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClip touchscreenSound;
    [SerializeField] private AudioClip notificationSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip failureSound;
    [SerializeField] private AudioClip backgroundMusic;

    [SerializeField] private AudioClip releaseLittleFork;
    [SerializeField] private AudioClip releaseLittleSpoon;
    [SerializeField] private AudioClip releaseSoundPlate;
    [SerializeField] private AudioClip releaseSoundGlassWine;
    [SerializeField] private AudioClip releaseSoundGlassWater;
    [SerializeField] private AudioClip releaseKnife;
    [SerializeField] private AudioClip releaseFork;
    [SerializeField] private AudioClip releaseSpoon;

    [SerializeField] private AudioClip tablePlaced;
    [SerializeField] private AudioClip tableRemoved;

    [SerializeField] private AudioClip grapSound;
    [SerializeField] private AudioClip lockedSound;
    [SerializeField] private AudioClip lastTenSecondsForCountingDown;

    private AudioSource _sfxSource;
    private AudioSource _musicSource;

    private const float BackgroundMusicVolume = 0.25f;

    private static readonly Dictionary<string, System.Func<SoundManager, AudioClip>> TagToReleaseClipMap =
        new Dictionary<string, System.Func<SoundManager, AudioClip>>
        {
            { "Plate",        sm => sm.releaseSoundPlate },
            { "GlassWater",   sm => sm.releaseSoundGlassWater },
            { "GlassWine",    sm => sm.releaseSoundGlassWine },
            { "DinnerFork",   sm => sm.releaseFork },
            { "FishFork",     sm => sm.releaseFork },
            { "CarvingFork",  sm => sm.releaseFork },
            { "FishKnife",    sm => sm.releaseKnife },
            { "SteakKnife",   sm => sm.releaseKnife },
            { "DinnerKnife",  sm => sm.releaseKnife },
            { "DessertKnife", sm => sm.releaseKnife },
            { "ButterKnife",  sm => sm.releaseKnife },
            { "FruitKnife",   sm => sm.releaseKnife },
            { "DinnerSpoon",  sm => sm.releaseSpoon },
            { "DessertSpoon", sm => sm.releaseLittleSpoon },
            { "TeaSpoon",     sm => sm.releaseLittleSpoon },
            { "FruitSpoon",   sm => sm.releaseLittleSpoon },
            { "FruitFork",    sm => sm.releaseLittleFork },
            { "DessertFork",  sm => sm.releaseLittleFork },
            { "SaladFork",    sm => sm.releaseLittleFork },
        };

    private void Awake()
    {
        InitializeSingleton();
        CreateAudioSources();
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    /// <summary>
    /// Ensures only one SoundManager instance persists across scenes.
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Creates the AudioSource components for SFX and background music.
    /// </summary>
    private void CreateAudioSources()
    {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
    }

    /// <summary>
    /// Plays an audio clip as a one-shot sound effect.
    /// </summary>
    /// <param name="clip">The AudioClip to play. Ignored if null.</param>
    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
            _sfxSource.PlayOneShot(clip);
    }

    /// <summary>Plays the touchscreen tap sound.</summary>
    public void PlayTouchscreenSound() => PlayClip(touchscreenSound);

    /// <summary>Plays the locked item sound.</summary>
    public void PlayLockedSound() => PlayClip(lockedSound);

    /// <summary>Plays the grab interaction sound.</summary>
    public void PlayGrapSound() => PlayClip(touchscreenSound);

    /// <summary>Plays the notification sound.</summary>
    public void PlayNotificationSound() => PlayClip(notificationSound);

    /// <summary>Plays the success sound effect.</summary>
    public void PlaySuccessSound() => PlayClip(successSound);

    /// <summary>Plays the failure sound effect.</summary>
    public void PlayFailureSound() => PlayClip(failureSound);

    /// <summary>Plays the countdown warning sound for the last 10 seconds.</summary>
    public void PlayLastTenSecondsCountdownSound() => PlayClip(lastTenSecondsForCountingDown);

    /// <summary>
    /// Plays the appropriate release sound for the given cutlery tag.
    /// </summary>
    /// <param name="tag">The cutlery tag identifying which release sound to play.</param>
    public void PlayReleaseSound(string tag)
    {
        var clip = GetReleaseClipForTag(tag);
        PlayClip(clip);
    }

    /// <summary>
    /// Looks up the release AudioClip associated with the given tag.
    /// </summary>
    /// <param name="tag">The cutlery tag to look up.</param>
    /// <returns>The matching AudioClip, or null if not found.</returns>
    private AudioClip GetReleaseClipForTag(string tag)
    {
        if (TagToReleaseClipMap.TryGetValue(tag, out var clipSelector))
            return clipSelector(this);

        return null;
    }

    /// <summary>
    /// Starts playing the looping background music at a reduced volume.
    /// </summary>
    private void PlayBackgroundMusic()
    {
        _musicSource.clip = backgroundMusic;
        _musicSource.volume = BackgroundMusicVolume;
        _musicSource.Play();
    }
}
