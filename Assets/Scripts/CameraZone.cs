using UnityEngine;
using System.Collections.Generic;

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

    HashSet<Character> charactersHit = new HashSet<Character>();

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;

        Set(characters[0]);

        if (charactersHit.Count < LevelManager.current.characters.Count) return;
        if (!unloadUnpopulatedLevels) return;
        LevelManager.current.UnloadUnpopulatedLevels();
    }

    public void Set(Character character) {
        if (character.currentLevel != level) return;
        if (character.characterCamera == null) return;
        CharacterCamera characterCamera = character.characterCamera;
        characterCamera.minPosition = cameraMin;
        characterCamera.maxPosition = cameraMax;
        if (positionMin != Vector2.zero) character.positionMin = positionMin;
        if (positionMax != Vector2.zero) character.positionMax = positionMax;
        charactersHit.Add(character);
    }
}