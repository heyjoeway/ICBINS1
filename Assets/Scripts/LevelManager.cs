using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : GameMode {
    static LevelManager _current;
    public static LevelManager current { get {
        if (_current == null)
            _current = GameObject.FindObjectOfType<LevelManager>();
        return _current;
    }}

    public SceneReference sceneDefault;
    public HashSet<Character> characters = new HashSet<Character>();


    void InitCharacters() {
        Character character;
        if (characters.Count == 0) {
            character = Instantiate(
                Resources.Load<GameObject>("Character/Character"),
                transform
            ).GetComponent<Character>();
            character.transform.SetParent(null); // Trick to instantiate in specific scene
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

    public override void Update() {
        base.Update();

        if (SceneManager.GetActiveScene().name != "Disposables") {
            Scene disposablesCurrent = SceneManager.GetSceneByName("Disposables");

            if (disposablesCurrent.isLoaded)
                SceneManager.SetActiveScene(disposablesCurrent);
        }

        UpdateStartJoin();
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

    public int GetFreePlayerId() {
        int id = -1;
        foreach (Character character in characters)
            id = Mathf.Max(id, character.playerId);
        return id + 1;
    }

    public void UpdateStartJoin() {
        for(int controllerId = 1; controllerId <= 4; controllerId++) {
            if (InputCustom.GetButtonsDown(controllerId, "Pause")) {
                bool alreadySpawned = false;
                foreach (Character character in characters) {
                    if (character.input.controllerId == controllerId) {
                        alreadySpawned = true;
                        break;
                    }
                }
                if (alreadySpawned) continue;

                Character characterNew = Instantiate(
                    Resources.Load<GameObject>("Character/Character"),
                    transform
                ).GetComponent<Character>();
                characterNew.transform.SetParent(null); // Trick to instantiate in specific scene
                characterNew.input.controllerId = controllerId;
            }
        }
    }
}