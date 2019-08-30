using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour {
    public HashSet<CharacterPackage> characterPackages = new HashSet<CharacterPackage>();

    void Start() {
        foreach (CharacterPackage characterPackage in FindObjectsOfType<CharacterPackage>()) {
            characterPackages.Add(characterPackage);
        }
    }

    public void UnloadUnpopulatedLevels() {
        foreach (Level level in FindObjectsOfType<Level>()) {
            bool levelPopulated = false;
            foreach (CharacterPackage characterPackage in characterPackages) {
                if (characterPackage.character.currentLevel != level) continue;
                levelPopulated = true;
                break;   
            }
            if (levelPopulated) continue;
            SceneManager.UnloadSceneAsync(
                level.gameObject.scene,
                UnloadSceneOptions.UnloadAllEmbeddedSceneObjects
            );            
        }
    }
}