using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : GameMode {
    // ========================================================================
    // Helper methods to get the currently loaded LevelManager.
    // If there are multiple LevelManagers loaded at once, this obviously won't work.
    // However, that shouldn't ever need to happen. I hope.

    static LevelManager _current;
    public static LevelManager current { get {
        if (_current == null)
            _current = GameObject.FindObjectOfType<LevelManager>();
        return _current;
    }}
    // ========================================================================
    // Inspector options
    public bool debugMutliplayer = true; // Allows P1 to spawn multiple players
    public SceneReference sceneDefault; // Default scene to load when no levels are loaded

    // ========================================================================

    public HashSet<Character> characters = new HashSet<Character>();

    void InitCharacter() {
        if (characters.Count == 0) {
            Character character = Instantiate(
                Resources.Load<GameObject>("Character/Characters/Sonic - Sonic Rush/Character")
            ).GetComponent<Character>();
            Utils.SetScene(character.transform, "Level");
        }
        ReloadDisposablesScene();
    }

    void Start() {
        // Must be run on Start rather than Awake to give any Levels time to spawn
        Utils.SetActiveScene("Level");

        Level levelDefault = FindObjectOfType<Level>();
        if (levelDefault == null) {
            StartCoroutine(Utils.LoadLevelAsync(
                sceneDefault.ScenePath,
                (Level level) => InitCharacter()
            ));
        } else InitCharacter();
    }

    public override void Update() {
        base.Update();

        // Ensure all temporary objects are loaded into "disposables"
        Scene disposablesCurrent = SceneManager.GetSceneByName("Disposables");

        if (disposablesCurrent.isLoaded)
            SceneManager.SetActiveScene(disposablesCurrent);

        UpdateStartJoin();
    }

    public void ReloadDisposablesScene() {
        Scene disposablesCurrent = SceneManager.GetSceneByName("Disposables");
        if (disposablesCurrent.isLoaded) {
            foreach(GameObject obj in disposablesCurrent.GetRootGameObjects())
                Destroy(obj);
        } else SceneManager.LoadScene("Scenes/Disposables", LoadSceneMode.Additive);

    }

    // Get the smallest available player Id
    // (Player IDs should always be kept in a sequence, even if a player leaves)
    // (See Character.OnDestroy)
    public int GetFreePlayerId() {
        int id = -1;
        foreach (Character character in characters)
            id = Mathf.Max(id, character.playerId);
        return id + 1;
    }

    // Allows players to press Start to join the game.
    public int maxPlayers = 4;
    public void UpdateStartJoin() {
        for(int controllerId = 1; controllerId <= maxPlayers; controllerId++) {
            if (InputCustom.GetButtonDown(controllerId, "Pause")) {
                bool alreadySpawned = false;
                foreach (Character character in characters) {
                    if (character.input.controllerId == controllerId) {
                        if (!debugMutliplayer) {
                            alreadySpawned = true;
                            break;
                        }
                    }
                }
                if (alreadySpawned) continue;

                Character characterNew = Instantiate(
                    Resources.Load<GameObject>("Character/Character"),
                    transform
                ).GetComponent<Character>();
                Utils.SetScene(characterNew.transform, "Level");
                characterNew.input.controllerId = controllerId;
            }
        }
    }
}