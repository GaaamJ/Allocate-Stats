using UnityEngine;

public class AudioCuePlayer : MonoBehaviour
{
    [SerializeField] private AudioCue cue = AudioCue.None;

    public void Play()
    {
        AudioManager.PlayCue(cue);
    }

    public void Play(AudioCue overrideCue)
    {
        AudioManager.PlayCue(overrideCue);
    }
}
