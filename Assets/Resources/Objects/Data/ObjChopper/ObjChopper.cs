using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjChopper : MonoBehaviour
{
    new Rigidbody rigidbody { get { return GetComponent<Rigidbody>(); }}
    Vector3 positionOrig;
    public float jumpSpeed = 13.125F;
    public float gravity = -0.17578125F;

    
    void Start() { positionOrig = transform.position; }

    void Update() {
        if (transform.position.y <= positionOrig.y) {
            rigidbody.velocity = new Vector3(0, jumpSpeed * Utils.physicsScale);
            transform.position = positionOrig;
        }

        rigidbody.velocity += new Vector3(0, gravity * Utils.physicsScale * Utils.deltaTimeScale);
    }
}
