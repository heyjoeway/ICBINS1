using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollZone : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        Character[] characters = other.transform.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        character.rollLock = true;
        character.stateCurrent = Character.CharacterState.rolling;
    }

    void OnTriggerExit(Collider other) {
        Character[] characters = other.transform.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        character.rollLock = false;
    }
}
