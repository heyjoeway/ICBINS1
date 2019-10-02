using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour {
    public class MusicStackEntry {
        public string introPath;
        public string loopPath;
        public int priority = 0;
        public bool disableSfx = false;
        public bool resumeAfter = false;
        public bool fadeInAfter = false;
        public bool ignoreClear = false;
        // public string mixerGroup = "Music";

        // public float timeCurrent;
        public AudioClip introClip;
        public AudioClip loopClip;

        bool _initDone = false;
        public void Init() {
            if (_initDone) return;
            
            if ((introClip == null) && (introPath != null))
                introClip = Resources.Load<AudioClip>(introPath);

            if ((loopClip == null) && (loopPath != null))
                loopClip = Resources.Load<AudioClip>(loopPath);
            
            _initDone = true;
        }
    }
    List<MusicStackEntry> musicStack = new List<MusicStackEntry>();

    MusicStackEntry musicStackEntryPrev;
    public MusicStackEntry musicStackEntryCurrent { get {
        MusicStackEntry musicStackEntryMax = null;
        int priorityMax = int.MinValue;
        foreach(MusicStackEntry entry in musicStack) {
            if (entry.priority < priorityMax) continue;

            priorityMax = entry.priority;
            musicStackEntryMax = entry;
        }
        return musicStackEntryMax;
    }}

    public float tempo = 1F;
    float fadeVolume = 1F;
    float fadeSpeed = 0.333F;

    AudioSource audioSourceIntro;
    AudioSource audioSourceLoop;
    AudioMixer mixer;
    AudioMixerGroup mixerGroup;

    public void FadeIn() {
        fadeVolume = -40;
        fadeSpeed = 0.333F;
    }

    public void FadeOut() {
        fadeVolume = 0;
        fadeSpeed = -0.333F;
    }

    // Start is called before the first frame update
    void Start() {
        mixer = Resources.Load<AudioMixer>("Main");
        mixerGroup = mixer.FindMatchingGroups("Music")[0];

        audioSourceIntro = gameObject.AddComponent<AudioSource>();
        audioSourceLoop = gameObject.AddComponent<AudioSource>();
        audioSourceLoop.loop = true;
    }

    public void Add(MusicStackEntry musicStackEntry) {
        musicStack.Add(musicStackEntry);
    }

    public void Remove(MusicStackEntry musicStackEntry) {
        musicStack.Remove(musicStackEntry);
    }

    public void Play(MusicStackEntry musicStackEntry) {
        Clear();
        musicStack.Add(musicStackEntry);
        fadeSpeed = 0;
        fadeVolume = 0;
    }

    // Update is called once per frame
    void Update() {
        MusicStackEntry entry = musicStackEntryCurrent;
        MusicStackEntry entryPrev = musicStackEntryPrev;

        bool entryDone = (
            (audioSourceIntro.clip != null) &&
            (audioSourceLoop.clip == null) &&
            !audioSourceIntro.isPlaying
        );

        if (entryDone) {
            if (entry.fadeInAfter) FadeIn();
            musicStack.Remove(entry);
            audioSourceIntro.Stop();
            audioSourceLoop.Stop();
            audioSourceIntro.clip = null;
            audioSourceLoop.clip = null;
            return;
        }

        mixer.SetFloat("Music Pitch", tempo);
        mixer.SetFloat("Music Pitch Shift", 1F / tempo);
        mixer.SetFloat("Music Volume", fadeVolume);
        fadeVolume = Mathf.Max(
            -40,
            Mathf.Min(
                0,
                fadeVolume + fadeSpeed
            )
        );
    
        if (entry != null) {
            mixer.SetFloat("SFX Volume", entry.disableSfx ? -40 : 0);
        }

        if (entry == entryPrev) return;
        musicStackEntryPrev = entry;

        entry.Init();

        audioSourceIntro.Stop();
        audioSourceLoop.Stop();
        audioSourceIntro.clip = null;
        audioSourceLoop.clip = null;

        audioSourceIntro.outputAudioMixerGroup = mixerGroup;
        audioSourceLoop.outputAudioMixerGroup = mixerGroup;

        if (entry != null) {
            double dspTime = AudioSettings.dspTime;
            if (entry.introClip != null) {
                audioSourceIntro.clip = entry.introClip;
                audioSourceIntro.PlayScheduled(dspTime + 0.1);
            }

            if (entry.loopClip != null) {
                audioSourceLoop.clip = entry.loopClip;
                if (entry.introClip != null) {
                    double clipDuration = (double)audioSourceIntro.clip.samples / audioSourceIntro.clip.frequency;
                    audioSourceLoop.PlayScheduled(dspTime + 0.1 + clipDuration);
                } else audioSourceLoop.Play();
            }
        }
    }

    public void Clear() {
        for (int i = musicStack.Count - 1; i >= 0; i--) {
            if (musicStack[i].ignoreClear) continue;
            musicStack.RemoveAt(i);
        }
    }
}
