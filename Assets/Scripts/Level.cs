using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {
    public GameObject background;
    public AudioClip musicIntro;
    public AudioClip musicLoop;
    public CameraZone cameraZoneStart;

    LevelManager levelManager;
    public Vector3 spawnPosition { get {
        return transform.Find("Spawn Position").position;
    }}

    public int act = 0;
    public string zone = "Unknown";

    void Start() {
        Utils.SetFramerate();
        levelManager = Utils.GetLevelManager();
    }

    void Update() {
        foreach(CharacterPackage characterPackage in levelManager.characterPackages) {
            if (characterPackage.character.currentLevel != this) continue;
            
            DLEUpdateCharacter(characterPackage);
            characterPackage.camera.backgroundObj = background;
        }
    }

    public void Unload() {
        foreach(CharacterPackage characterPackage in levelManager.characterPackages) {
            if (characterPackage.character.currentLevel == this) return;
        }
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    public void Reload() {
        StartCoroutine(Utils.LoadLevelAsync(
            gameObject.scene.path,
            (Level nextLevel) => {
                foreach(CharacterPackage characterPackage in levelManager.characterPackages) {
                    if (characterPackage.character.currentLevel != this) continue;
                    Character character = characterPackage.character;
                    character.currentLevel = nextLevel;
                    MakeTitleCard(character);
                    character.Respawn();
                }
                SceneManager.UnloadSceneAsync(gameObject.scene);
            },
            true
        ));
    }

    public ObjTitleCard MakeTitleCard(Character character) {
        ObjTitleCard titleCard = Instantiate(
            Resources.Load<GameObject>("Objects/Title Card"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ObjTitleCard>();
        titleCard.character = character;
        titleCard.Init();
        return titleCard;
    }

    public virtual void DLEUpdateCharacter(CharacterPackage characterPackage) { }
}
