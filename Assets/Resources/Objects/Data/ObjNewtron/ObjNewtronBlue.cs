using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjNewtronBlue : MonoBehaviour {
    ObjEnemy enemyBehaviour;
    new Rigidbody rigidbody;
    Animator animator;
    
    void InitReferences() {
        enemyBehaviour = GetComponent<ObjEnemy>();
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Start() {
        InitReferences();
    }

    float raycastDistance = 0.5F;
    const float triggerDistance = 4F;

    bool activated = false;
    bool hitGround = false;

    void Update() {
        if (!activated || !hitGround) {
            CharacterPackage characterPackage = Utils.CheckIfCharacterInRange(
                transform.position,
                triggerDistance,
                Utils.AxisType.X,
                Utils.DistanceType.Character,
                Utils.GetLevelManager().characterPackages
            );
            if (characterPackage != null) {
                if (!activated) {
                    animator.Play("Normal");
                    activated = true;
                }
                
                if (!hitGround) {
                    transform.localScale = new Vector3(
                        characterPackage.character.position.x < transform.position.x ? 1 : -1,
                        1, 1
                    );
                }
            }
        }

        RaycastHit hit;
        Physics.Raycast(
            transform.position, // origin
            Vector3.down, // direction,
            out hit,
            raycastDistance,
            ~Character.solidRaycastMask
        );
        if (hit.collider == null) return;

        Vector3 newPos = transform.position;
        newPos.y = hit.point.y + 0.5F;
        transform.position = newPos;

        if (!hitGround) {
            animator.Play("Moving");
            raycastDistance = 1.5F;
            enemyBehaviour.enabled = true;
            rigidbody.useGravity = false;
            rigidbody.velocity = new Vector3(
                2F * Utils.physicsScale * -Mathf.Sign(transform.localScale.x),
                0, 0
            );
            hitGround = true;
        }
    }
}
