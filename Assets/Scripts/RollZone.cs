using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollZone : MonoBehaviour {
    public bool lockLeft = false;

    void OnTriggerStay(Collider other) {
        OnTriggerEnter(other);
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        bool rollLock = false;
        if (character.velocity.x < 0.05) rollLock = lockLeft;
        if (character.velocity.x > 0.05) rollLock = !lockLeft;
        
        if (rollLock && character.InStateGroup("ground")) {
            if (!character.InStateGroup("rolling"))
                SFX.PlayOneShot(character.audioSource, "sfxRoll");

            character.stateCurrent = "rollLock";
        } else if (!rollLock && character.stateCurrent == "rollLock")
            character.stateCurrent = "rolling";
    }
}
