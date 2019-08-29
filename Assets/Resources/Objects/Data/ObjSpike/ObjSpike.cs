using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSpike : MonoBehaviour, ColliderListener {

    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================

    CharacterGroundedDetector characterGroundedDetector;
    ColliderBridge colliderBridge;
    GameObject topPositionObj;

    void InitReferences() {
        characterGroundedDetector = transform.Find("Object").GetComponent<CharacterGroundedDetector>();
        colliderBridge = transform.Find("Object").GetComponent<ColliderBridge>();
        topPositionObj = transform.Find("Top Position").gameObject;
    }

    // ========================================================================

    float topAngle { get {
        Vector3 topPos = topPositionObj.transform.position;
        Vector3 centerPos = transform.position;
        return (360 - Vector3.Angle(
            (topPos - centerPos).normalized,
            Vector3.up
        )) % 360;
    }}

    // ========================================================================

    public void TryAction(Character character, float collisionAngle) {
        if (Mathf.Abs(collisionAngle - topAngle) > 0.1) return;
        DoAction(character);
    }

    public void OnCollisionEnter(Collision collision) {
        GameObject other = collision.gameObject;
        Character[] characters = other.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        ContactPoint hit = collision.GetContact(0);
        float collisionAngle = (
            Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles.z
            + 180
        ) % 360;

        TryAction(character, collisionAngle);
    }
    public void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }

    void Update() {
        foreach(Character character in characterGroundedDetector.characters) {
            TryAction(character, character.transform.eulerAngles.z);
            // Save script time by only processing one character
            // Triggering the action via the GroundedDetectors is a fallback anyways            
            break;
        }
    }

    public void OnCollisionExit(Collision collision) {}
    public void OnTriggerEnter(Collider other) {}
    public void OnTriggerExit(Collider other) {}

    // ========================================================================

    void Start() {
        InitReferences();
        colliderBridge.Initialize(this);
    }

    public void DoAction(Character character) {
        character.Hurt(character.position.x <= transform.position.x, true);
    }
}
