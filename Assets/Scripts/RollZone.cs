using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollZone : MonoBehaviour {
    public bool lockLeft = false;

    void OnTriggerStay(Collider other) {
        Character[] characters = other.transform.gameObject.GetComponentsInParent<Character>();

        foreach (Character character in characters) {
            if (character.position.x > transform.position.x)
                character.rollLock = !lockLeft;
            else
                character.rollLock = lockLeft;

            if (character.rollLock && character.inGroundedState)
                character.stateCurrent = Character.CharacterState.rolling;
        }
    }
}
