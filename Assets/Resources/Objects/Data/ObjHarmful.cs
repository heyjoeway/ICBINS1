using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjHarmful : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        if (!enabled) return;

        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        character.Hurt(character.position.x <= transform.position.x);
    }
}
