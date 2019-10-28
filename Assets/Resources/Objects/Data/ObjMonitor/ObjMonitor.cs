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
            if (!character.InStateGroup("rolling")) return;

            Explode(character);

            if (!character.InStateGroup("airCollision")) return;
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
            if (!(character.InStateGroup("rolling") && character.InStateGroup("airCollision"))) return;
            Vector3 velocityTemp = character.velocityPrev;
            velocityTemp.y = -Mathf.Abs(velocityTemp.y);
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
            Constants.Get<GameObject>("prefabExplosionEnemy"),
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

        Destroy(GetComponent<HomingAttackTarget>());
    }

    float kinematicTimer = 0;

    void Update() {
        if (Mathf.Abs(rigidbody.velocity.y) < 0.01) {
            kinematicTimer += Utils.cappedUnscaledDeltaTime;
            if (kinematicTimer > 0.5F) rigidbody.isKinematic = true;
        } else kinematicTimer = 0;
    }
}
