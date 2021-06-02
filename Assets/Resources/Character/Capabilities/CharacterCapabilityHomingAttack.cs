using UnityEngine;
using System.Collections.Generic;

public class CharacterCapabilityHomingAttack : CharacterCapability {
    public float homingAttackSpeed = 9F;
    public float homingAttackBounceSpeed = 6.5F;
    public bool homingAttackUncurl = false;
    public bool homingAttackUseUncurled = false;
    public string[] buttonsHomingAttack = new string[] { "Secondary", "Tertiary" };
    
    // ========================================================================

    float failsafeTimer;
    private bool used = false;
    Transform target;
    CharacterEffect afterImageEffect;

    public override void Init() {
        name = "homingAttack";
        character.AddStateGroup("rolling", "homingAttack");
        character.AddStateGroup("airCollision", "homingAttack");
        character.AddStateGroup("harmful", "homingAttack");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != "homingAttack") return;

        failsafeTimer = 5F;        
        SFX.PlayOneShot(character.audioSource, "sfxHomingAttack");
        
        target = FindClosestTarget();

        if (target == null) {
            character.velocity = new Vector2(
                (
                    homingAttackSpeed * character.physicsScale *
                    (character.facingRight ? 1 : -1)
                ),
                0
            );
            if (homingAttackUncurl) {
                character.stateCurrent = "air";
                character.AnimatorPlay("Air Dash");
            } else {
                character.stateCurrent = "rollingAir";
                character.effects.Add(new CharacterEffect(character, "afterImage", 0.25F));
            }
        } else {
            character.velocity = Vector3.zero;
            afterImageEffect = new CharacterEffect(character, "afterImage");
            character.effects.Add(afterImageEffect);
        }
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != "homingAttack") return;
        if (afterImageEffect != null)
            afterImageEffect.DestroyBase();
    }

    public void OnTargetLost() {
        if (homingAttackUncurl) {
            character.stateCurrent = "air";
        } else {
            character.stateCurrent = "rollingAir";
        }
    }

    public override void CharUpdate(float deltaTime) {
        if (!character.InStateGroup("air")) used = false;

        if (
            (
                (homingAttackUseUncurled && character.InStateGroup("air")) ||
                (character.stateCurrent == "jump")
            ) && !used
        ) {
            if (character.input.GetButtonsDownPreventRepeat(buttonsHomingAttack)) {
                character.stateCurrent = "homingAttack";
                used = true;
            }
        }

        if (character.stateCurrent != "homingAttack") return;
        if (target == null) {
            OnTargetLost();
        } else {
            character.position = Vector3.MoveTowards(
                character.position,
                target.position,
                homingAttackSpeed * character.physicsScale * deltaTime * 2
            );

            failsafeTimer -= deltaTime;
            if (failsafeTimer <= 0) OnTargetLost();
        }
    }

    public override void OnCharCollisionEnter(Collision collision) {
        if (character.stateCurrent != "homingAttack") return;
        if (collision.collider.isTrigger) return;
        OnTargetLost();     
    }

    public override void OnCharTriggerEnter(Collider other) {
        if (character.stateCurrent != "homingAttack") return;
        HomingAttackTarget[] targets = other.gameObject.GetComponentsInParent<HomingAttackTarget>();
        if (targets.Length == 0) return;

        character.velocity = new Vector2(
            0,
            homingAttackBounceSpeed * character.physicsScale
        );
        character.stateCurrent = "jump";
        used = false;
    }

    Transform FindClosestTarget(float distanceLimit = 24F) {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach(HomingAttackTarget target in GameObject.FindObjectsOfType<HomingAttackTarget>()) {
            Transform potentialTarget = target.transform;
            if (!potentialTarget.gameObject.activeSelf) continue;
            if (
                (potentialTarget.position.x <= character.position.x) &&
                character.facingRight
            ) continue;
            if (
                (potentialTarget.position.x >= character.position.x) &&
                !character.facingRight
            ) continue;
            if (!target.enabled) continue;

            Vector3 directionToTarget = potentialTarget.position - character.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        if (closestDistanceSqr > distanceLimit) return null;
        return bestTarget;
    }
}