using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {
    public GameObject background;
    LevelManager levelManager;

    public virtual int act { get { return 0; }}
    public virtual string zone { get { return "Unknown"; }}

    void Start() {
        levelManager = Utils.GetLevelManager();
    }

    void Update() {
        foreach(CharacterPackage characterPackage in levelManager.characterPackages) {
            if (characterPackage.character.currentLevel == null)
                characterPackage.character.currentLevel = this;
    
            if (characterPackage.character.currentLevel != this) continue;
            
            DLEUpdateCharacter(characterPackage);
            characterPackage.camera.backgroundObj = background;
        }
    }

    void Unload() {
        foreach(CharacterPackage characterPackage in levelManager.characterPackages) {
            if (characterPackage.character.currentLevel == this) return;
        }
        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    public virtual void DLEUpdateCharacter(CharacterPackage characterPackage) { }
}
