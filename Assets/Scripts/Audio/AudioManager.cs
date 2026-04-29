using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Library")]
    [SerializeField] private AudioCueLibrary library;

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambientSource;

    [Header("Volumes")]
    [SerializeField, Range(0f, 1f)] private float masterSfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float masterMusicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float masterAmbientVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool logCuePlayback = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSources();
    }

    public static void PlayCue(AudioCue cue)
    {
        if (cue == AudioCue.None) return;
        Instance?.Play(cue);
    }

    public static void StopMusicCue()
    {
        Instance?.StopMusic();
    }

    public static void StopAmbientCue()
    {
        Instance?.StopAmbient();
    }

    public void Play(AudioCue cue)
    {
        if (logCuePlayback)
            Debug.Log($"[Audio] Cue requested: {cue}");

        if (library == null)
        {
            if (logCuePlayback)
                Debug.LogWarning($"[Audio] No AudioCueLibrary assigned. Cue skipped: {cue}");
            return;
        }

        if (!library.TryGetCue(cue, out var entry))
        {
            if (logCuePlayback)
                Debug.LogWarning($"[Audio] Cue not found or has no clips. Cue skipped: {cue}");
            return;
        }

        var clip = entry.GetRandomClip();
        if (clip == null)
        {
            if (logCuePlayback)
                Debug.LogWarning($"[Audio] Cue has no playable clip. Cue skipped: {cue}");
            return;
        }

        EnsureSources();

        if (logCuePlayback)
            Debug.Log($"[Audio] Playing {cue} as {entry.PlaybackType}: {clip.name}");

        switch (entry.PlaybackType)
        {
            case AudioCuePlaybackType.MusicLoop:
                PlayLoop(musicSource, clip, entry.Volume * masterMusicVolume, entry.GetRandomPitch());
                break;
            case AudioCuePlaybackType.AmbientLoop:
                PlayLoop(ambientSource, clip, entry.Volume * masterAmbientVolume, entry.GetRandomPitch());
                break;
            default:
                sfxSource.pitch = entry.GetRandomPitch();
                sfxSource.PlayOneShot(clip, entry.Volume * masterSfxVolume);
                sfxSource.pitch = 1f;
                break;
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void StopAmbient()
    {
        if (ambientSource != null)
            ambientSource.Stop();
    }

    private void PlayLoop(AudioSource source, AudioClip clip, float volume, float pitch)
    {
        if (source == null || clip == null) return;

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.loop = true;
        source.Play();
    }

    private void EnsureSources()
    {
        sfxSource = EnsureSource(sfxSource, false);
        musicSource = EnsureSource(musicSource, true);
        ambientSource = EnsureSource(ambientSource, true);
    }

    private AudioSource EnsureSource(AudioSource source, bool loop)
    {
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = loop;
        return source;
    }
}
