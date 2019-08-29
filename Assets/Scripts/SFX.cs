using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SFX {
    static Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    public static AudioClip Get(string path) {
        if (!audioClipCache.ContainsKey(path))
            audioClipCache[path] = Resources.Load<AudioClip>(path);

        return audioClipCache[path];
    }

    public static void PlayOneShot(AudioSource audioSource, string path) {
        audioSource.PlayOneShot(Get(path));
    }

    public static void Play(AudioSource audioSource, string path, float pitch = 1) {
        audioSource.clip = Get(path);
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}
