using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    private Sound[] bgmSounds;
    private float[] bgmMaxVolumes;
    private Sound[] sfxSounds;
    private float[] sfxMaxVolumes;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;

            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;

        }
    }

    void Start()
    {
        bgmSounds = Array.FindAll(sounds, sound => sound.BGM);

        bgmMaxVolumes = new float[bgmSounds.Length];
        for (int i = 0; i < bgmSounds.Length; i++)
        {
            bgmMaxVolumes[i] = bgmSounds[i].volume;
        }


        sfxSounds = Array.FindAll(sounds, sound => !sound.BGM);

        sfxMaxVolumes = new float[sfxSounds.Length];
        for (int i = 0; i < sfxSounds.Length; i++)
        {
            sfxMaxVolumes[i] = sfxSounds[i].volume;
        }
    }

    void DelayPlay(Sound sound, float delay)
    {
        sound.source.PlayDelayed(delay);
        // Debug.Log($"Sound '{name}' should be playing now after a delay of {delay} seconds.");
    }

    void NormalPlay(Sound sound)
    {
        sound.source.Play();
        // Debug.Log($"Sound '{name}' should be playing now.");

    }

    public void Play(string name, float delay = 0f)
    {
        // Debug.Log("Attempting to play sound: " + name);
        Sound sound = Array.Find(sounds, s => s.name == name);

        if (sound != null)
        {
            sound.source.mute = false;
            if (delay > 0f)
            {
                DelayPlay(sound, delay);
            }
            else
            {
                NormalPlay(sound);
            }
        }
        else
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
    }

    public void StopSound(string _name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == _name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + _name + " not found.");
            return;
        }
        s.source.Stop();
    }

    public void UpdateBGMVolume(float volume)
    {
        for (int i = 0; i < bgmSounds.Length; i++)
        {
            bgmSounds[i].source.volume = bgmMaxVolumes[i] * volume;
        }
    }

    public void UpdateSFXVolume(float volume)
    {
        for (int i = 0; i < sfxSounds.Length; i++)
        {
            sfxSounds[i].source.volume = sfxMaxVolumes[i] * volume;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if (currentSoundIndex != testIndex)
        // {
        //     sounds[currentSoundIndex].source.Stop();
        //     currentSoundIndex = testIndex;
        //     sounds[currentSoundIndex].source.Play();

        // }
        // sounds[testIndex].source.volume = testVolume;
    }
}
