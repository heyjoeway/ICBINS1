using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjMonitor : MonoBehaviour {
    new Collider collider;
    GameObject solidityObj;
    Animator animator;
    GameObject contentsObj;
    Animator contentsAnimator;
    ObjMonitorContents contents;
    new Rigidbody rigidbody;

    void InitReferences() {
        collider = GetComponent<Collider>();
        solidityObj = transform.Find("Solidity").gameObject;
        animator = GetComponent<Animator>();
        contentsObj = transform.Find("Contents").gameObject;
        contentsAnimator = contentsObj.GetComponent<Animator>();
        contents = contentsObj.GetComponent<ObjMonitorContents>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start() {
        InitReferences();
    }

    bool DidCharacterHitFromBottom(Character character) {
        return Mathf.Abs(Mathf.DeltaAngle(
            transform.position.AngleTowards(character.position) + 90,
            0
        )) < 30;
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        if (!DidCharacterHitFromBottom(character)) {
            Explode(character);

            if (!character.inRollingAirState) return;
            Vector3 velocityTemp = character.velocity;
            velocityTemp.y = Mathf.Abs(character.velocity.y);
            character.velocity = velocityTemp;
        }
    }

    void OnCollisionEnter(Collision collision) {
        Character[] characters = collision.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        if (DidCharacterHitFromBottom(character)) {
            if (!character.inRollingAirState) return;
            Vector3 velocityTemp = character.velocity;
            velocityTemp.y = -Mathf.Abs(character.velocity.y);
            character.velocity = velocityTemp;

            rigidbody.isKinematic = false;
            rigidbody.velocity = new Vector2(
                0,
                1.5F
            ) * Utils.physicsScale;
        }
    }

    void Explode(Character sourceCharacter) {
        Instantiate(
            (GameObject)Resources.Load("Objects/Explosion (Enemy)"),
            transform.position,
            Quaternion.identity
        );

        contents.recipient = sourceCharacter;
        contentsAnimator.Play("Monitor Contents Released");

        animator.Play("Monitor Breaking");
        collider.enabled = false;
        solidityObj.SetActive(false);
        rigidbody.isKinematic = true;
        rigidbody.velocity = Vector3.zero;
    }
}
