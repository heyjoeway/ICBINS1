using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    public SceneReference sceneDefault;
    public HashSet<CharacterPackage> characterPackages = new HashSet<CharacterPackage>();

    void InitCharacterPackage(CharacterPackage characterPackage) {
        Level levelDefault = FindObjectOfType<Level>();
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

    void InitCharacters() {
        foreach (CharacterPackage characterPackage in FindObjectsOfType<CharacterPackage>())
            InitCharacterPackage(characterPackage);

        if (characterPackages.Count == 0) {
            InitCharacterPackage(Instantiate(
                Resources.Load<GameObject>("Character/Character Package")
            ).GetComponent<CharacterPackage>());
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

    void Update() {
        float deltaTime = Utils.cappedUnscaledDeltaTime;
        while (deltaTime > 0) {
            float modDeltaTime = deltaTime > 1F / 60F ? 1F / 60F : deltaTime;
            Physics.Simulate(modDeltaTime * Time.timeScale);
            foreach (CharacterPackage characterPackage in characterPackages) {
                if (deltaTime == Utils.cappedUnscaledDeltaTime)
                    InputCustom.PreventRepeatReset();
    
                characterPackage.character.UpdateDelta(modDeltaTime);
                characterPackage.camera.UpdateDelta(modDeltaTime);
            }
            deltaTime -= modDeltaTime;
        }
    }

    public void UnloadUnpopulatedLevels() {
        foreach (Level level in FindObjectsOfType<Level>())
            level.Unload();          
    }
}