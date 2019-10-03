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

    public class RaycastHitHybrid {
        public Vector3 normal;
        public Vector3 point;
        public Transform transform;
        public float distance;

        public bool is2D;
        public bool isValid;

        // 3D
        public Rigidbody rigidbody3D;
        public Vector3 barycentricCoordinate;
        public Collider collider3D;
        public Vector2 lightmapCoord;
        public Vector2 textureCoord;
        public Vector2 textureCoord2;
        public int triangleIndex;

        // 2D
        public Rigidbody2D rigidbody2D;
        public Vector2 centroid;
        public Collider2D collider2D;
        public float fraction;

        public RaycastHitHybrid(RaycastHit raycastHit3D) {
            normal = raycastHit3D.normal;
            point = raycastHit3D.point;
            transform = raycastHit3D.transform;
            distance = raycastHit3D.distance;
            rigidbody3D = raycastHit3D.rigidbody;
            barycentricCoordinate = raycastHit3D.barycentricCoordinate;
            collider3D = raycastHit3D.collider;
            lightmapCoord = raycastHit3D.lightmapCoord;
            textureCoord = raycastHit3D.textureCoord;
            textureCoord2 = raycastHit3D.textureCoord2;
            triangleIndex = raycastHit3D.triangleIndex;

            is2D = false;
            isValid = collider3D != null;
        }

        public RaycastHitHybrid(RaycastHit2D raycastHit2D) {
            normal = raycastHit2D.normal;
            point = raycastHit2D.point;
            transform = raycastHit2D.transform;
            distance = raycastHit2D.distance;
            rigidbody2D = raycastHit2D.rigidbody;
            centroid = raycastHit2D.centroid;
            collider2D = raycastHit2D.collider;
            fraction = raycastHit2D.fraction;

            is2D = true;
            isValid = collider2D != null;
        }

        public RaycastHitHybrid() {
            isValid = false;
        }
    }

    public static RaycastHitHybrid RaycastHybrid(
        Vector3 origin, Vector3 direction, float maxDistance, int layerMask = 0,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
    ) {
        RaycastHit hit3D; 
        Physics.Raycast(
            origin,
            direction,
            out hit3D,
            maxDistance,
            layerMask,
            queryTriggerInteraction
        );
        if (hit3D.collider != null) return new RaycastHitHybrid(hit3D);

        RaycastHit2D hit2D = Physics2D.Raycast(
            origin,
            direction,
            maxDistance,
            layerMask
        );
        if (hit2D.collider != null) return new RaycastHitHybrid(hit2D);

        return new RaycastHitHybrid();
    }
}