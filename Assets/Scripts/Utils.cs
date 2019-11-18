using System.Collections.Generic;
using System.Collections;
using System.Text;
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
    public static Character CheckIfCharacterInRange(
        Vector2 thisPos,
        float triggerDistance,
        AxisType axisType,
        DistanceType distanceType,
        HashSet<Character> characters
    ) {
        foreach(Character character in characters) {
            Vector2 cameraPos;
            if (character.characterCamera != null)
                cameraPos = character.characterCamera.position;
            else
                cameraPos = character.position;
            
            Vector2 charPos = character.position;

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

            if (otherDist <= triggerDistance) return character;
        }

        return null;
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
        Application.targetFrameRate = Screen.currentResolution.refreshRate;
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 180;
        // Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1F / Application.targetFrameRate;
        Time.maximumDeltaTime = 1F / 10F;
    }

    public static float cappedUnscaledDeltaTime { get {
        float deltaTime = Time.unscaledDeltaTime;
        if (deltaTime > Time.maximumDeltaTime)
            return 1F / Application.targetFrameRate;
        return deltaTime;
    }}

    public static float cappedDeltaTime { get {
        float deltaTime = Time.deltaTime;
        if (Time.deltaTime > Time.maximumDeltaTime)
            return 1F / Application.targetFrameRate;
        return deltaTime;
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

    // Store all case variants since str.ToLower() causes GC
    static HashSet<string> stringsPositive = new HashSet<string> {
        "true", "True", "TRUE",
        "on",   "On",   "ON",
        "yes",  "Yes",  "YES",
        "y",    "Y",
        "t",    "T",
        "ok",   "Ok",   "OK"
    };
    public static bool StringBool(string str) => stringsPositive.Contains(str);

    // ========================================================================

    static Dictionary<int, string> intStrCache = new Dictionary<int, string>();
    public static string IntToStrCached(int val) {
        if (!intStrCache.ContainsKey(val))
            intStrCache[val] = val.ToString();
        return intStrCache[val];
    }
    public static string IntToStrCached(float val) => IntToStrCached((int)val);
}