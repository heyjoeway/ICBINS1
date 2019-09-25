using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathZone : MonoBehaviour {
    void OnTriggerStay(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        if (character.InStateGroup("death")) return;
        character.stateCurrent = "dying";
    }
}
