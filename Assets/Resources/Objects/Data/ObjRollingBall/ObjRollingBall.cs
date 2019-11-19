using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjRollingBall : MonoBehaviour {
    new Rigidbody rigidbody;
    CharacterGroundedDetector groundedDetector;

    void Awake() {
        rigidbody = GetComponent<Rigidbody>();
        groundedDetector = GetComponent<CharacterGroundedDetector>();
    }

    void Update() {
        // foreach(Character character in groundedDetector.characters) {
        //     Vector3 velocity = rigidbody.velocity;
        //     velocity.x = Mathf.Cos(character.forwardAngle * Mathf.Deg2Rad) * -character.groundSpeed;
        //     rigidbody.velocity = velocity;
        //     character.position = transform.position + Vector3.up * transform.lossyScale.y;
        //     break;
        // }
    }
}
