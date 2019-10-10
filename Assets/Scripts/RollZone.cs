using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollZone : MonoBehaviour {
    public bool lockLeft = false;

    new Renderer renderer;
    LevelManager levelManager;

    void Start() {
        renderer = GetComponent<Renderer>();
        levelManager = Utils.GetLevelManager();
    }

    void Update() {
        foreach (Character character in levelManager.characters) {
            if (!renderer.bounds.Contains(character.position)) continue;
            
            bool rollLock = false;
            if (character.velocity.x < 0.05) rollLock = lockLeft;
            if (character.velocity.x > 0.05) rollLock = !lockLeft;
           
            if (rollLock && character.InStateGroup("ground")) {
                if (!character.InStateGroup("rolling"))
                    SFX.PlayOneShot(character.audioSource, "SFX/Sonic 1/S1_BE");

                character.stateCurrent = "rollLock";
            } else if (!rollLock && character.stateCurrent == "rollLock")
                character.stateCurrent = "rolling";
        }
    }
}
