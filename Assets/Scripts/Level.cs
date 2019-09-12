using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {
    public GameObject background;
    public AudioClip musicIntro;
    public AudioClip musicLoop;
    public LevelManager levelManager;
    public Vector3 spawnPosition { get {
        return transform.Find("Spawn Position").position;
    }}

    public virtual int act { get { return 0; }}
    public virtual string zone { get { return "Unknown"; }}

    void Start() {
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
                    Debug.Log(character.currentLevel == this);
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
