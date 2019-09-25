using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollZone : MonoBehaviour {
    public bool lockLeft = false;

    void OnTriggerStay(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();

        foreach (Character character in characters) {
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
