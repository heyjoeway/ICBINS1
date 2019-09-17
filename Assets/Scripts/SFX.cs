using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class SFX {
    static Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    public static AudioClip Get(string path) {
        if (!audioClipCache.ContainsKey(path))
            audioClipCache[path] = Resources.Load<AudioClip>(path);

        return audioClipCache[path];
    }

    static AudioMixer _mixer;

    public static void PlayOneShot(AudioSource audioSource, string path, float volume = 1F) {
        if (_mixer == null)
            _mixer = Resources.Load<AudioMixer>("Main");

        float _mixerVolume;
        _mixer.GetFloat("SFX Volume", out _mixerVolume);
        _mixerVolume = 1F - (_mixerVolume / -40F);

        audioSource.PlayOneShot(
            Get(path),
            volume * _mixerVolume
        );
    }

    public static void Play(AudioSource audioSource, string path, float pitch = 1) {
        audioSource.clip = Get(path);
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}
