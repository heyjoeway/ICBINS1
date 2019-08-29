using System.Collections.Generic;

using UnityEngine;

public static class Utils {
    public static float deltaTimeScale { get {
        return 60F * Time.deltaTime;
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
            Vector2 cameraPos = characterPackage.camera.transform.position;
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

    public static LevelManager GetLevelManager() {
        return GameObject.FindObjectOfType<LevelManager>();
    }
}