using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour {
    void OnTriggerStay(Collider other) {
        Character[] characters = other.transform.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        if (character.inDeadState) return;
        character.stateCurrent = Character.CharacterState.dying;
    }
}
