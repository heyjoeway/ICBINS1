using UnityEngine;

public class CameraZone : MonoBehaviour {
    Level level;
    public Vector2 cameraMin;
    public Vector2 cameraMax;
    public Vector2 positionMin;
    public Vector2 positionMax;
    public bool unloadUnpopulatedLevels = false;

    void Start() {
        level = GetComponentInParent<Level>();

        if (cameraMin == Vector2.zero) cameraMin = Vector2.one * -Mathf.Infinity;
        if (cameraMax == Vector2.zero) cameraMax = Vector2.one * Mathf.Infinity;
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;

        if (unloadUnpopulatedLevels)
            Utils.GetLevelManager().UnloadUnpopulatedLevels();
    }

    public void Set(Character character) {
        CharacterCamera characterCamera = character.characterCamera;
        characterCamera.minPosition = cameraMin;
        characterCamera.maxPosition = cameraMax;
        if (positionMin != Vector2.zero) character.positionMin = positionMin;
        if (positionMax != Vector2.zero) character.positionMax = positionMax;
    }

    void OnTriggerStay(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        Set(character);
    }
}