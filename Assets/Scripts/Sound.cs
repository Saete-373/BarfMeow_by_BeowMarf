using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume;

    [Range(0.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    public Sound(AudioClip clip, float volume = 1.0f, float pitch = 1.0f, bool loop = false, bool playOnAwake = false)
    {
        this.clip = clip;
        this.volume = volume;
        this.pitch = pitch;
        this.loop = loop;
    }
}
