using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSpring : MonoBehaviour {
    CharacterGroundedDetector characterGroundedDetector;
    // ========================================================================

    GameObject topPositionObj => transform.Find("Top Position").gameObject;
    float topAngle => transform.eulerAngles.z;

    // ========================================================================

    public void TryActionAngle(ObjSpringable character, float collisionAngle) {
        if (Mathf.Abs(collisionAngle - topAngle) > 0.1) return;
        DoAction(character);
    }

    public void TryActionCooldown(ObjSpringable character) {
        DoAction(character);
    }

    // ========================================================================

    public void OnCollisionEnter(Collision collision) {
        GameObject other = collision.gameObject;
        ObjSpringable[] characters = other.GetComponentsInParent<ObjSpringable>();
        if (characters.Length == 0) return;
        ObjSpringable character = characters[0];

        if (character.GetComponent<Character>() != null && topAngle == 0) return;

        ContactPoint hit = collision.GetContact(0);
        float collisionAngle = (
            Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles.z
            + 180
        ) % 360;

        TryActionAngle(character, collisionAngle);
    }
    public void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }
    public void OnCollisionExit(Collision collision) {}

    // ========================================================================

    public void OnTriggerEnter(Collider collider) {
        GameObject other = collider.gameObject;
        ObjSpringable[] characters = other.GetComponentsInParent<ObjSpringable>();
        if (characters.Length == 0) return;
        ObjSpringable character = characters[0];

        TryActionCooldown(character);
    }
    // public void OnTriggerStay(Collider collider) {
    //     OnTriggerEnter(collider);
    // }

    public void OnTriggerExit(Collider collider) {}

    // ========================================================================

    void Update() {
        foreach(Character character in characterGroundedDetector.characters)
            TryActionAngle(
                character.GetComponent<ObjSpringable>(),
                character.transform.eulerAngles.z
            );
    }

    // ========================================================================

    Animator animator => transform.Find("Object").GetComponent<Animator>();

    public AudioSource audioSource => GetComponent<AudioSource>();

    public enum SpringType {
        Yellow,
        Red
    }

    public bool keepGrounded = false;

    public SpringType type = SpringType.Yellow;

    // Start is called before the first frame update
    void Start() {
        characterGroundedDetector = transform.Find("Object").GetComponent<CharacterGroundedDetector>();

        // switch(type) {
        //     case SpringType.Yellow:
        //         animator.Play("Yellow Normal");
        //         break;
        //     case SpringType.Red:
        //         animator.Play("Red Normal");
        //         break;
        // }
    }

    public float springPower = 16F;

    public void DoAction(ObjSpringable obj) {
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();

        Vector3 velocityRaw = (topPositionObj.transform.position - transform.position).normalized * springPower;
        rigidbody.velocity = new Vector3(
            Mathf.Abs(velocityRaw.x) > springPower / 3 ? velocityRaw.x * Utils.physicsScale : rigidbody.velocity.x,
            Mathf.Abs(velocityRaw.y) > springPower / 3 ? velocityRaw.y * Utils.physicsScale : rigidbody.velocity.y,
            Mathf.Abs(velocityRaw.z) > springPower / 3 ? velocityRaw.z * Utils.physicsScale : rigidbody.velocity.z
        );

        // rigidbody.position += rigidbody.velocity / 60F;

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

        Character character = obj.GetComponent<Character>();
        if (character != null) {
            if (!keepGrounded) {
                character.stateCurrent = "air";
                character.AnimatorPlay("Spring", "", 0);
                character.spriteAnimatorSpeed = 1;
            } else character.GroundSpeedSync();
        }
    }
}
