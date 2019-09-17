using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerZone : MonoBehaviour {
    public float zLeft = 0;
    public float zRight = 0;
    public bool groundedOnly = false;

    void OnTriggerStay(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();

        foreach (Character character in characters) {
            Vector3 characterPos = character.position;

            if (groundedOnly && !character.inGroundedState)
                return;

            if (characterPos.x > transform.position.x) characterPos.z = zRight;
            else characterPos.z = zLeft;

            character.position = characterPos;
        }
    }
}
