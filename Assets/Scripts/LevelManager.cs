using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : GameMode {
    public SceneReference sceneDefault;
    public HashSet<Character> characters = new HashSet<Character>();

    void InitCharacter(Character character) {
        Level levelDefault = FindObjectOfType<Level>();
        characters.Add(character);

        if (character.currentLevel == null) {
            character.currentLevel = levelDefault;
            character.respawnData.position = levelDefault.spawnPosition;
            character.Respawn();
        }

        ObjTitleCard titleCard = character.currentLevel.MakeTitleCard(character);

        if (GlobalOptions.Get<bool>("tinyMode"))
            character.sizeScale = 0.5F;
    }

    void InitCharacters() {
        foreach (Character character in FindObjectsOfType<Character>())
            InitCharacter(character);

        if (characters.Count == 0) {
            InitCharacter(
                Instantiate(
                    Resources.Load<GameObject>("Character/Character")
                ).GetComponent<Character>()
            );
        }

        Time.timeScale = 0;

        ReloadDisposablesScene();
    }

    public override void Start() {
        base.Start();
        Level levelDefault = FindObjectOfType<Level>();
        if (levelDefault == null) {
            StartCoroutine(Utils.LoadLevelAsync(
                sceneDefault.ScenePath,
                (Level level) => InitCharacters()
            ));
        } else InitCharacters();
    }

    void Update() {
        if (SceneManager.GetActiveScene().name != "Disposables") {
            Scene disposablesCurrent = SceneManager.GetSceneByName("Disposables");

            if (disposablesCurrent.isLoaded)
                SceneManager.SetActiveScene(disposablesCurrent);
        }

        float deltaTime = Utils.cappedUnscaledDeltaTime;

        InputCustom.preventRepeatLock = false;
        while (deltaTime > 0) {
            float modDeltaTime = deltaTime > 1F / 60F ? 1F / 60F : deltaTime;
            if (GlobalOptions.Get<bool>("gbaMode"))
                modDeltaTime += (Random.value * 0.032F) - 0.016F;

            if (modDeltaTime < 1F / (Application.targetFrameRate * 2)) break;
            
            foreach (Character character in characters)
                character.UpdateDelta(modDeltaTime);
            
            Physics.Simulate(modDeltaTime * Time.timeScale);

            foreach (Character character in characters) {
                character.UpdateSpritePosition();

                if (character.characterCamera != null)
                    character.characterCamera.UpdateDelta(modDeltaTime);
            }

            InputCustom.preventRepeatLock = true;    
            deltaTime -= modDeltaTime;
        }
    }

    public void ReloadDisposablesScene() {
        Scene disposablesCurrent = SceneManager.GetSceneByName("Disposables");
        if (disposablesCurrent.isLoaded) {
            foreach(GameObject obj in disposablesCurrent.GetRootGameObjects())
                Destroy(obj);
        } else {
            SceneManager.LoadScene("Scenes/Disposables", LoadSceneMode.Additive);
        }

    }

    public void UnloadUnpopulatedLevels() {
        foreach (Level level in FindObjectsOfType<Level>())
            level.Unload();          
    }
}