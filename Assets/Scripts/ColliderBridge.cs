using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBridge : MonoBehaviour {
    ColliderListener _listener;
    new Collider collider;

    public void Initialize(ColliderListener l) {
        _listener = l;
        collider = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision) {
        _listener.OnCollisionEnter(collision);
    }

    void OnCollisionStay(Collision collision) {
        _listener.OnCollisionStay(collision);
    }

    void OnTriggerEnter(Collider other) {
        _listener.OnTriggerEnter(other);
    }

    void OnCollisionExit(Collision collision) {
        _listener.OnCollisionExit(collision);
    }

    void OnTriggerExit(Collider other) {
        _listener.OnTriggerExit(other);
    }
}
