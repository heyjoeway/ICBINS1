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
    
    int physicsStepsPerFrame = 2;

    public virtual void Update() {
        float deltaTime = Utils.cappedUnscaledDeltaTime;
        // Limit small fluctuations in deltatime
        deltaTime = Mathf.Round(deltaTime * Application.targetFrameRate) / Application.targetFrameRate;

        if (deltaTime <= 2F / Application.targetFrameRate)
            deltaTime = 1F / Application.targetFrameRate;

        InputCustom.preventRepeatLock = false;

        // We update the frame in a loop over the current delta time to allow
        // the game to catch up if it lags.
        while (deltaTime > 0) {
            float modDeltaTime = deltaTime > 1F / 60F ? 1F / 60F : deltaTime;

            // (meme)
            if (GlobalOptions.Get<bool>("gbaMode"))
                modDeltaTime += (Random.value * 0.032F) - 0.016F;
            
            foreach (GameBehaviour gameBehaviour in GameBehaviour.allGameBehvaiours)
                gameBehaviour.UpdateDelta(modDeltaTime);
            
            // Run multiple simulations per frame to avoid clipping.
            // Without this, the character sometimes clips into the ground slightly before popping out.
            for (int i = 0; i < physicsStepsPerFrame; i++)
                Physics.Simulate(modDeltaTime * Time.timeScale * (1F / physicsStepsPerFrame));

            foreach (GameBehaviour gameBehaviour in GameBehaviour.allGameBehvaiours)
                gameBehaviour.LateUpdateDelta(modDeltaTime);

            // Since we're updating the character multiple times per frame, we
            // need to prevent Input.GetButtonDown from firing multiple times.
            InputCustom.preventRepeatLock = true;
            deltaTime -= modDeltaTime;
        }
    }
}