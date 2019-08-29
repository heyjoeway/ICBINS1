
using UnityEngine;

public interface ColliderListener {
    void OnCollisionEnter(Collision collision);
    void OnCollisionExit(Collision collision);
    void OnCollisionStay(Collision collision);
    void OnTriggerEnter(Collider other);
    void OnTriggerExit(Collider other);
}