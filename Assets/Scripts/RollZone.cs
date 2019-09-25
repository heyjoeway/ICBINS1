using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollZone : MonoBehaviour {
    public bool lockLeft = false;

    void OnTriggerStay(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();

        foreach (Character character in characters) {
            // if (character.velocity.x > 0.05) character.rollLock = !lockLeft;
            // if (character.velocity.x < 0.05) character.rollLock = lockLeft;

            // if (character.rollLock && character.inGroundedState)
            //     character.stateCurrent = Character.CharacterState.rolling;
        }
    }
}
