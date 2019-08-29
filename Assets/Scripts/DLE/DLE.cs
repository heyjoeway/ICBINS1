using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLE : MonoBehaviour {
    LevelManager levelManager;

    void Start() {
        levelManager = Utils.GetLevelManager();
    }

    void Update() {
        foreach(CharacterPackage characterPackage in levelManager.characterPackages) {
            UpdateCharacter(characterPackage);
        }
    }

    public virtual void UpdateCharacter(CharacterPackage characterPackage) { }
}
