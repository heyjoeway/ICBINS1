using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterCapabilityGround : CharacterCapability {
    public float accelerationGroundNormal = 0.046875F;
    public float accelerationGroundSpeedUp = 0.09375F;
    public float frictionGroundNormal = 0.046875F;
    public float frictionGroundSpeedUp = 0.09375F;
    public float decelerationGround = 0.5F;
    public float slopeFactorGround = 0.125F;
    public float slopeFactorHInputLock = 0.125F;
    public float skidThreshold = 4.5F;
    public float fallThreshold = 2.5F;
    public float horizontalInputLockTime = 0.5F; // seconds
    public float topSpeedNormal = 6F;
    public float topSpeedSpeedUp = 12F;
    public float slopeFactorAccThreshold = 0.04F;
    public float animWalkThreshold = 2F;
    public float animRunThreshold = 6F;
    public float animPeeloutThreshold = 12F;
    public float animSkidFastThreshold = 8F;
    public Transform groundModeGroup;

    // ========================================================================

    public float accelerationGround => (
        character.HasEffect("speedUp") ?
            accelerationGroundSpeedUp :
            accelerationGroundNormal
    );

    public float frictionGround => (
        character.HasEffect("speedUp") ?
            frictionGroundSpeedUp :
            frictionGroundNormal
    );

    public float topSpeed => (
        character.HasEffect("speedUp") ?
            topSpeedSpeedUp :
            topSpeedNormal
    );

    // ========================================================================

    bool pushing = false;

    public override void Init() {
        name = "ground";
        character.AddStateGroup("ground", "ground");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (!character.InStateGroup("ground")) return;
        character.modeGroupCurrent = groundModeGroup;
        if (character.stateCurrent != name) return;
        character.destroyEnemyChain = 0;
        pushing = false;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;
        character.groundedDetectorCurrent = null;
    }

    public override void CharUpdate(float deltaTime) {
        if (character.stateCurrent != name) return;
        UpdateGroundMove(deltaTime);
        UpdateGroundTerminalSpeed();
        character.GroundSnap();
        UpdateGroundAnim(deltaTime);
        UpdateGroundFallOff();
    }

    float accelerationPrev = 0;

    // 3D-Ready: Sorta
    void UpdateGroundMove(float deltaTime) {
        float accelerationMagnitude = 0F;

        int inputDir = 0;
        if (!character.HasEffect("horizontalInputLock")) {
            // ORDER MATTERS!
            if (character.input.GetAxisPositive("Horizontal")) inputDir = 1;
            if (character.input.GetAxisNegative("Horizontal")) inputDir = -1;
        }

        if (inputDir == 1) {
            if (character.groundSpeed < 0) {
                accelerationMagnitude = decelerationGround * character.physicsScale;
            } else if (character.groundSpeed < topSpeed * character.physicsScale) {
                accelerationMagnitude = accelerationGround * character.physicsScale;
            }
        } else if (inputDir == -1) {
            if (character.groundSpeed > 0) {
                accelerationMagnitude = -decelerationGround * character.physicsScale;
            } else if (character.groundSpeed > -topSpeed * character.physicsScale) {
                accelerationMagnitude = -accelerationGround * character.physicsScale;
            }
        } else {
            if (Mathf.Abs(character.groundSpeed) > 0.25F * character.physicsScale) {
                accelerationMagnitude = -Mathf.Sign(character.groundSpeed) * frictionGround * character.physicsScale;
            } else {
                character.groundSpeed = 0;
                accelerationMagnitude = 0;
            }
        }

        // Used to make Rush physics less sticky
        float slopeFactor = slopeFactorGround * character.physicsScale;
        if (character.HasEffect("horizontalInputLock"))
            slopeFactor = slopeFactorHInputLock * character.physicsScale;

        float slopeFactorAcc = (
            slopeFactor *
            Mathf.Sin(
                character.forwardAngle *
                Mathf.Deg2Rad
            )
        );

        if (Mathf.Abs(slopeFactorAcc) > slopeFactorAccThreshold * character.physicsScale)
            accelerationMagnitude -= slopeFactorAcc;

        character.groundSpeed += accelerationMagnitude * deltaTime * 60F;
        accelerationPrev = accelerationMagnitude;
    }

    public void UpdateRunAnim(float speed) {
        // Slow Walking
        // ======================
        if (Mathf.Abs(speed) < animWalkThreshold * character.physicsScale) {
            character.AnimatorPlay("Slow Walk", "Slow Walk");
            character.spriteAnimatorSpeed = 1 + (Mathf.Abs(speed) / topSpeedNormal * character.physicsScale);

        // Walking
        // ======================
        } else if (Mathf.Abs(speed) < animRunThreshold * character.physicsScale) {
            character.AnimatorPlay("Walk", "Walk");
            character.spriteAnimatorSpeed = 1 + (Mathf.Abs(speed) / topSpeedNormal * character.physicsScale);
        // Running Fast
        // ======================
        } else if (
            (Mathf.Abs(speed) >= animPeeloutThreshold * character.physicsScale) &&
            GlobalOptions.GetBool("peelOut")
        ) {
            character.AnimatorPlay("Fast", "Fast");
            character.spriteAnimatorSpeed = Mathf.Abs(speed) / topSpeedNormal * character.physicsScale;
        } else {
        // Running
        // ======================
            character.AnimatorPlay("Run", "Run");
            character.spriteAnimatorSpeed = Mathf.Abs(speed) / topSpeedNormal * character.physicsScale;
        }
    }

    // Updates the character's animation while they're on the ground
    void UpdateGroundAnim(float deltaTime) {
        character.spriteAnimatorSpeed = 1;

        // Check if we are transitioning to a rolling air state. If so, set the speed of it
        if (character.InStateGroup("rolling") && character.InStateGroup("air")) {
            character.spriteAnimatorSpeed = 1 + (
                (
                    Mathf.Abs(character.groundSpeed) /
                    topSpeedNormal * character.physicsScale
                ) * 2F
            );
        } else {
            // Turning
            // ======================
            if (character.pressingLeft && (character.groundSpeed < 0))
                character.facingRight = false;
            else if (character.pressingRight && (character.groundSpeed > 0))
                character.facingRight = true;

            // Skidding
            // ======================
            bool alreadySkidding = character.AnimatorIsTag("Skid");

            bool notSkidding = (
                ((accelerationPrev > 0) && (character.groundSpeed > 0)) ||
                ((accelerationPrev < 0) && (character.groundSpeed < 0))
            );

            // You can only trigger a skid state if:
            // - Your angle (a) is <= 45d or >= 270d and your absolute speed is above the threshhold
            // - OR you're already skidding
            bool canSkid = (
                (
                    (character.pressingRight && character.groundSpeed < 0) ||
                    (character.pressingLeft && character.groundSpeed > 0)
                ) && (
                    (character.forwardAngle <= 45F) ||
                    (character.forwardAngle >= 270F)
                ) && (
                    Mathf.Abs(character.groundSpeed) >= skidThreshold * character.physicsScale
                )
            );

            // Standing still, looking up/down, idle animation
            // ======================
            if (character.groundSpeed == 0) {
                if (character.input.GetAxisNegative("Vertical"))
                    character.AnimatorPlay("Look Down");
                else if (character.input.GetAxisPositive("Vertical"))
                    character.AnimatorPlay("Look Up");
                else if (character.balanceState != Character.BalanceState.None) {                   
                    if (!character.AnimatorIsTag("Balancing")) {
                        if (
                            (character.facingRight && (character.balanceState == Character.BalanceState.Right)) ||
                            (!character.facingRight && (character.balanceState == Character.BalanceState.Left))
                        ) {
                            character.AnimatorPlay("Balancing Forwards");
                        } else {
                            character.AnimatorPlay("Balancing Backwards");
                        }
                    }
                } else character.AnimatorPlay("Idle", "Idle");
            // Pushing anim
            // ======================
            } else if (pushing) {
                character.AnimatorPlay("Push", "Push");
    
                character.spriteAnimatorSpeed = 1 + (Mathf.Abs(character.groundSpeed) / topSpeedNormal * character.physicsScale);
            // Skidding, again
            // ======================
            } else if (canSkid && !notSkidding && !alreadySkidding) {
                SFX.Play(character.audioSource, "sfxSkid");

                if (Mathf.Abs(character.groundSpeed) < animSkidFastThreshold * character.physicsScale)
                    character.AnimatorPlay("Skid");
                else
                    character.AnimatorPlay("Skid Fast");
            } else UpdateRunAnim(character.groundSpeed);
        }

        // Final value application
        // ======================
        // ORDER MATTERS! GetSpriteRotation may depend on flipX for rotation-based flipping
        character.flipX = !character.facingRight;
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);

        pushing = false;
    }


    // 3D-Ready: YES
    void UpdateGroundFallOff() {
        // SPG says these angles should be 270 and 90... I don't really believe that
        // That results in some wicked Sonic 4 walk up walls shenanigans
        // I imagine this is because of the differences in how angles are
        // represented between pixel-based and vector-based collision

        if (character.HasEffect("horizontalInputLock")) return;
        if (Mathf.Abs(character.groundSpeed) >= fallThreshold * character.physicsScale) return;

        if (!((character.forwardAngle <= 315) && (character.forwardAngle >= 45))) return;
        character.effects.Add(new CharacterEffect(
            character,
            "horizontalInputLock",
            horizontalInputLockTime * character.physicsScale
        ));

        if (!((character.forwardAngle < 271) && (character.forwardAngle > 89))) return;

        if (character.stateCurrent == "rolling")
            character.stateCurrent = "rollingAir";
        else character.stateCurrent = "air";
    }

    // 3D-Ready: YES
    void UpdateGroundTerminalSpeed() {
        character.groundSpeed = Mathf.Min(
            Mathf.Abs(character.groundSpeed),
            character.terminalSpeed * character.physicsScale
        ) * Mathf.Sign(character.groundSpeed);
    }


    public override void OnCharCollisionStay(Collision collision) {
        OnCharCollisionEnter(collision);
    }

    // 3D-Ready: NO
    public override void OnCharCollisionEnter(Collision collision) {
        if (!character.InStateGroup("ground")) return;

        // Prevent tight spaces from slowing down character
        // (tight spaces may cause collisions on top and bottom of char)
        // This is kinda a hack, but it works.
        Vector3 collisionPoint = collision.GetContact(0).point;
        float collisionAngle = Quaternion.FromToRotation(
            Vector3.up,
            collision.GetContact(0).normal
        ).eulerAngles.z - 90F;

        if (
            (
                Mathf.Abs(Mathf.DeltaAngle(
                    collisionAngle, character.forwardAngle
                )) > 30F
            ) && (
                Mathf.Abs(Mathf.DeltaAngle(
                    collisionAngle, character.forwardAngle + 180
                )) > 30F
            )
        ) return;

        bool pushLeft = character.pressingLeft && (collisionPoint.x < character.position.x);
        bool pushRight = character.pressingRight && (collisionPoint.x > character.position.x);

        pushing = pushLeft || pushRight;

        character.GroundSpeedSync();
    }

}