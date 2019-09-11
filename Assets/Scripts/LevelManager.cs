using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    public SceneReference sceneDefault;
    public HashSet<CharacterPackage> characterPackages = new HashSet<CharacterPackage>();

    void InitCharacters() {
        Level levelDefault = FindObjectOfType<Level>();
        foreach (CharacterPackage characterPackage in FindObjectsOfType<CharacterPackage>()) {
            characterPackages.Add(characterPackage);

            Character character = characterPackage.character;
            character.InitReferences();

            if (character.currentLevel == null) {
                character.currentLevel = levelDefault;
                character.respawnData.position = levelDefault.spawnPosition;
                character.Respawn();
            }

            character.currentLevel.MakeTitleCard(character);
        }
        Time.timeScale = 0;
    }

    void Start() {
        Level levelDefault = FindObjectOfType<Level>();
        if (levelDefault == null) {
            StartCoroutine(Utils.LoadLevelAsync(
                sceneDefault.ScenePath,
                (Level level) => InitCharacters()
            ));
        } else InitCharacters();
    }

    public void UnloadUnpopulatedLevels() {
        foreach (Level level in FindObjectsOfType<Level>())
            level.Unload();          
    }
}