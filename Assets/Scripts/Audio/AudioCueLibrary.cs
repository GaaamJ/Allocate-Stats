using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioCueLibrary", menuName = "Marginalia/Audio Cue Library")]
public class AudioCueLibrary : ScriptableObject
{
    [SerializeField] private AudioCueEntry[] cues;

    public bool TryGetCue(AudioCue cue, out AudioCueEntry entry)
    {
        if (cues != null)
        {
            foreach (var candidate in cues)
            {
                if (candidate != null && candidate.Cue == cue && candidate.HasClips)
                {
                    entry = candidate;
                    return true;
                }
            }
        }

        entry = null;
        return false;
    }
}

[Serializable]
public class AudioCueEntry
{
    [SerializeField] private AudioCue cue;
    [SerializeField] private AudioCuePlaybackType playbackType = AudioCuePlaybackType.OneShot;
    [SerializeField] private AudioClip[] clips;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = Vector2.one;

    public AudioCue Cue => cue;
    public AudioCuePlaybackType PlaybackType => playbackType;
    public bool HasClips => clips != null && clips.Length > 0;
    public float Volume => volume;

    public AudioClip GetRandomClip()
    {
        if (!HasClips) return null;
        return clips[UnityEngine.Random.Range(0, clips.Length)];
    }

    public float GetRandomPitch()
    {
        float min = Mathf.Min(pitchRange.x, pitchRange.y);
        float max = Mathf.Max(pitchRange.x, pitchRange.y);
        return Mathf.Approximately(min, max) ? min : UnityEngine.Random.Range(min, max);
    }
}
