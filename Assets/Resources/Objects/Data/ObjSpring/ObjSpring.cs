using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSpring : MonoBehaviour {
    CharacterGroundedDetector characterGroundedDetector { get {
        return transform.Find("Object").GetComponent<CharacterGroundedDetector>();
    }}

    // ========================================================================

    GameObject topPositionObj { get {
        return transform.Find("Top Position").gameObject;
    }}

    float topAngle { get {
        return transform.rotation.eulerAngles.z;
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

    Animator animator { get {
        return transform.Find("Object").GetComponent<Animator>();
    }}

    public AudioSource audioSource { get {
        return GetComponent<AudioSource>();
    }}

    public enum SpringType {
        Yellow,
        Red
    }

    public bool keepGrounded = false;

    public SpringType type = SpringType.Yellow;

    // Start is called before the first frame update
    void Start() {
        switch(type) {
            case SpringType.Yellow:
                animator.Play("Yellow Normal");
                break;
            case SpringType.Red:
                animator.Play("Red Normal");
                break;
        }
    }

    float springPower { get{
        switch(type) {
            case SpringType.Red:
                return 16F;
        }
        return 10F; // Yellow is default, switch case exists in case more types are added
    }}

    public void DoAction(Character character) {
        // if (character.isDropDashing) return; // Allow drop dash on spring

        if (!keepGrounded) {
            character.stateCurrent = "air";
            character.spriteAnimator.Play("Spring");
            character.spriteAnimator.speed = 1;
        }

        Vector3 velocityRaw = (topPositionObj.transform.position - transform.position).normalized * springPower;
        character.velocity = new Vector3(
            Mathf.Abs(velocityRaw.x) > springPower / 3 ? velocityRaw.x * character.physicsScale : character.velocity.x,
            Mathf.Abs(velocityRaw.y) > springPower / 3 ? velocityRaw.y * character.physicsScale : character.velocity.y,
            Mathf.Abs(velocityRaw.z) > springPower / 3 ? velocityRaw.z * character.physicsScale : character.velocity.z
        );

        character.position += character.velocity / 60F;

        if (keepGrounded)
            character.GroundSpeedSync();

        audioSource.time = 0;
        audioSource.Play();

        switch(type) {
            case SpringType.Yellow:
                animator.Play("Yellow Hit");
                break;
            case SpringType.Red:
                animator.Play("Red Hit");
                break;
        }
    }
}
