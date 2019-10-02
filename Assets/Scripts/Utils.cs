using System.Collections.Generic;
using System.Collections;
using System;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public static class Utils {
    public static float deltaTimeScale { get {
        return 60F * Utils.cappedDeltaTime;
    }}

    public static float physicsScale = 1.875F; // 60 (framerate) / 32 (pixels per unit)

    // ========================================================================

    public enum AxisType {
        X, // Distance to object only calculated on x axis
        Y, // ... y axis
        XY // ... both axis
    }

    // ========================================================================

    public enum DistanceType {
        Camera, // Distance to object originates from screen position
        Character, // ... player position
        Closest // ... whichever is closest
    }

    // ========================================================================
    public static CharacterPackage CheckIfCharacterInRange(
        Vector2 thisPos,
        float triggerDistance,
        AxisType axisType,
        DistanceType distanceType,
        HashSet<CharacterPackage> characterPackages
    ) {
        foreach(CharacterPackage characterPackage in characterPackages) {            
            Vector2 cameraPos = characterPackage.camera.position;
            Vector2 charPos = characterPackage.character.position;

            float cameraDist = Mathf.Infinity;
            float charDist = Mathf.Infinity;

            switch(axisType) {
                case AxisType.XY:
                    cameraDist = Vector2.Distance(thisPos, cameraPos);
                    charDist = Vector2.Distance(thisPos, charPos);
                    break;
                case AxisType.X:
                    cameraDist = Mathf.Abs(thisPos.x - cameraPos.x);
                    charDist = Mathf.Abs(thisPos.x - charPos.x);
                    break;
                case AxisType.Y:
                    cameraDist = Mathf.Abs(thisPos.y - cameraPos.y);
                    charDist = Mathf.Abs(thisPos.y - charPos.y);
                    break;
            }

            float otherDist = Mathf.Infinity;

            switch(distanceType) {
                case DistanceType.Character:
                    otherDist = charDist;
                    break;
                case DistanceType.Camera:
                    otherDist = cameraDist;
                    break;
                case DistanceType.Closest:
                    otherDist = Mathf.Min(cameraDist, charDist);
                    break;
            }

            if (otherDist <= triggerDistance) return characterPackage;
        }

        return null;
    }

    static LevelManager _levelManager;
    public static LevelManager GetLevelManager() {
        if (_levelManager == null) _levelManager = GameObject.FindObjectOfType<LevelManager>();
        return _levelManager;
    }

    public static MusicManager GetMusicManager() {
        return GameObject.FindObjectOfType<MusicManager>();
    }

    // Start hack because fuck unity; "SceneManager.sceneLoaded -=" DOESN'T. FUCKING. WORK.
    // I'm not even kidding. try it yourself. fuck this shitty engine.
    static bool _LoadLevelAsyncEverUsed = false;
    static Level _LoadLevelAsyncLevel;
    static void _LoadLevelAsyncOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        foreach (Level level in GameObject.FindObjectsOfType<Level>()) {
            if (level.gameObject.scene != scene) continue;
            _LoadLevelAsyncLevel = level;
            return;
        }
    }
    // End hack, mostly

    public static IEnumerator LoadLevelAsync(string scenePath, Action<Level> callback = null, bool ignoreDuplicates = false) {
        if (!_LoadLevelAsyncEverUsed) {
            SceneManager.sceneLoaded += _LoadLevelAsyncOnSceneLoaded;
            _LoadLevelAsyncEverUsed = true;
        }

        Scene nextLevelScene = SceneManager.GetSceneByPath(scenePath);

        if (ignoreDuplicates || !nextLevelScene.IsValid()) { // If scene isn't already loaded
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
                scenePath,
                LoadSceneMode.Additive
            );
            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone) yield return null;
            if (callback != null) {
                callback(_LoadLevelAsyncLevel);
                _LoadLevelAsyncLevel = null;
            }
        } else {
            if (callback == null) yield break;
            foreach (Level level in GameObject.FindObjectsOfType<Level>()) {
                if (level.gameObject.scene != nextLevelScene) continue;
                callback(level);
                break;
            }
        }
    }

    public static LayerMask? _IgnoreRaycastMask = null;
    public static LayerMask IgnoreRaycastMask {
        get {
            if (_IgnoreRaycastMask != null) return (LayerMask)_IgnoreRaycastMask;
            _IgnoreRaycastMask = LayerMask.GetMask(
                "Ignore Raycast",
                "Player - Ignore Top Solid and Raycast",
                "Player - Ignore Top Solid",
                "Player - Rolling",
                "Player - Rolling and Ignore Top Solid",
                "Object - Ignore Other Objects",
                "Object - Top Solid Only and Ignore Other Objects",
                "Object - Monitor Solidity",
                "Object - Monitor Trigger"
            );
            return (LayerMask)_IgnoreRaycastMask;
        }
    }

    public static void SetFramerate() {
        // Application.targetFrameRate = Screen.currentResolution.refreshRate;
        // Application.targetFrameRate = 120;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1F / Application.targetFrameRate;
        Time.maximumDeltaTime = 0.25F;
    }

    public static float cappedUnscaledDeltaTime { get {
        return Mathf.Min(
            Time.unscaledDeltaTime,
            Time.maximumDeltaTime
        );
    }}

    public static float cappedDeltaTime { get {
        return Mathf.Min(
            Time.deltaTime,
            Time.maximumDeltaTime
        );
    }}

    public static Tuple<int, int> CalculateFauxTransparencyFrameCount(float alpha) {
        // Tuple format is (off frames, on frames)
        if (alpha == 0) return Tuple.Create(Int32.MaxValue, 0);
        if (alpha == 1) return Tuple.Create(0, Int32.MaxValue);

        float onFramesDivisor = alpha * 2;
        float offFramesDivisor = 1 - onFramesDivisor;

        return Tuple.Create(
           (int)(1 / onFramesDivisor),
           (int)(1 / offFramesDivisor)
        );
    }
}