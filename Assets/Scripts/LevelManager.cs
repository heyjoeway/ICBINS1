using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour {
    public HashSet<CharacterPackage> characterPackages = new HashSet<CharacterPackage>();

    void Start() {
        foreach (CharacterPackage characterPackage in FindObjectsOfType<CharacterPackage>()) {
            characterPackages.Add(characterPackage);
        }
    }

    public float time;

    void Update() {
        time = Time.timeSinceLevelLoad;
    }
}