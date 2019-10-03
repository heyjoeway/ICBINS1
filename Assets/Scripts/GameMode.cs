using UnityEngine;
using UnityEngine.Audio;

public class GameMode : MonoBehaviour {
    public virtual void Start() {
        Utils.SetFramerate();
        Time.timeScale = 1;
        AudioMixer mixer = Resources.Load<AudioMixer>("Main");
        mixer.SetFloat("Music Pitch", 1);
        mixer.SetFloat("Music Pitch Shift", 1);
        mixer.SetFloat("Music Volume", 0);
        mixer.SetFloat("SFX Volume", 0);
    }
}