using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {
    public AudioClip musicIntro;
    public AudioClip musicLoop;
    public CameraZone cameraZoneStart;

    public Vector3 spawnPosition { get {
        Transform spawnLocation = transform.Find("Spawn Position");
        if (spawnLocation == null) return Vector3.zero;
        return spawnLocation.position;
    }}

    public int act = 0;
    public string zone = "Unknown";

    void Awake() {
        Scene levelScene = SceneManager.GetSceneByName("Level");
        if (!levelScene.isLoaded)
            SceneManager.LoadScene("Scenes/Level", LoadSceneMode.Additive);
    }

    void Update() {
        foreach(Character character in LevelManager.current.characters) {
            if (character.currentLevel != this) continue;
            DLEUpdateCharacter(character);
        }
    }

    public void Unload() {
        foreach(Character character in LevelManager.current.characters) {
            if (character.currentLevel == this) return;
        }
        SceneManager.UnloadSceneAsync(gameObject.scene);
        Resources.UnloadUnusedAssets();
    }

    public void ReloadFadeOut(Character character = null) {
        if (LevelManager.current.characters.Count == 1) Time.timeScale = 0;

        ScreenFade screenFade = Instantiate(
            Constants.Get<GameObject>("prefabScreenFadeOut"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ScreenFade>();
    
        if (character != null)
            screenFade.canvas.worldCamera = character.characterCamera.camera;

        MusicManager.current.FadeOut();
        screenFade.onComplete = () => {
            if (LevelManager.current.characters.Count == 1) Reload();
            else {
                if (character == null) return;
                ObjTitleCard.Make(character);
                character.Respawn();
            }
        };
    }

    public void Reload() {
        StartCoroutine(Utils.LoadLevelAsync(
            gameObject.scene.path,
            (Level nextLevel) => {
                MusicManager.current.Clear();
                LevelManager.current.ReloadDisposablesScene();
                foreach(Character character in LevelManager.current.characters) {
                    if (character.currentLevel != this) continue;
                    character.currentLevel = nextLevel;
                    ObjTitleCard.Make(character);
                    character.Respawn();
                }
                SceneManager.UnloadSceneAsync(gameObject.scene);
            },
            true
        ));
    }

    public virtual void DLEUpdateCharacter(Character character) { }
}
