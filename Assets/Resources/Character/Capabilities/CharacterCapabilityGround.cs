using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterCapabilityGround : CharacterCapability {
     public CharacterCapabilityGround(Character character) : base(character) { }

    bool pushing = false;

    public override void Init() {
        name = "ground";
        character.AddStateGroup("ground", "ground");

        character.stats.Add(new Dictionary<string, object>() {
            ["accelerationGroundNormal"] = 0.046875F,
            ["accelerationGroundSpeedUp"] = 0.09375F,
            ["accelerationGround"] = (Func<string>)(() => (
                character.HasEffect("speedUp") ?
                    "accelerationGroundSpeedUp" :
                    "accelerationGroundNormal"
            )),
            ["frictionGroundNormal"] = 0.046875F,
            ["frictionGroundSpeedUp"] = 0.09375F,
            ["frictionGround"] = (Func<string>)(() => (
                character.HasEffect("speedUp") ?
                    "frictionGroundSpeedUp" :
                    "frictionGroundNormal"
            )),
            ["decelerationGround"] = 0.5F,
            ["slopeFactorGround"] = 0.125F,
            ["slopeFactorHInputLock"] = 0.125F,
            ["skidThreshold"] = 4.5F,
            ["fallThreshold"] = 2.5F,
            ["horizontalInputLockTime"] = 0.5F, // seconds
            ["topSpeedNormal"] = 6F,
            ["topSpeedSpeedUp"] = 12F,
            ["topSpeed"] = (Func<string>)(() => (
                character.HasEffect("speedUp") ?
                    "topSpeedSpeedUp" :
                    "topSpeedNormal"
            )),
            ["slopeFactorAccThreshold"] = 0.04F,

            ["animWalkThreshold"] = 2F,
            ["animRunThreshold"] = 6F,
            ["animPeeloutThreshold"] = 12F,
            ["animSkidFastThreshold"] = 8F,
        });

    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;
        character.destroyEnemyChain = 0;
        pushing = false;
        character.modeGroupCurrent = character.groundModeGroup;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;
        character.groundedDetectorCurrent = null;
    }

    public override void Update(float deltaTime) {
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
        if (character.horizontalInputLockTimer <= 0) {
            // ORDER MATTERS!
            if (character.input.GetAxisPositive("Horizontal")) inputDir = 1;
            if (character.input.GetAxisNegative("Horizontal")) inputDir = -1;
        } else character.horizontalInputLockTimer -= deltaTime;

        if (inputDir == 1) {
            if (character.groundSpeed < 0) {
                accelerationMagnitude = character.stats.Get("decelerationGround");
            } else if (character.groundSpeed < character.stats.Get("topSpeed")) {
                accelerationMagnitude = character.stats.Get("accelerationGround");
            }
        } else if (inputDir == -1) {
            if (character.groundSpeed > 0) {
                accelerationMagnitude = -character.stats.Get("decelerationGround");
            } else if (character.groundSpeed > -character.stats.Get("topSpeed")) {
                accelerationMagnitude = -character.stats.Get("accelerationGround");
            }
        } else {
            if (Mathf.Abs(character.groundSpeed) > 0.25F * character.physicsScale) {
                accelerationMagnitude = -Mathf.Sign(character.groundSpeed) * character.stats.Get("frictionGround");
            } else {
                character.groundSpeed = 0;
                accelerationMagnitude = 0;
            }
        }

        // Used to make Rush physics less sticky
        float slopeFactor = character.stats.Get("slopeFactorGround");
        if (character.horizontalInputLockTimer > 0)
            slopeFactor = character.stats.Get("slopeFactorHInputLock");

        float slopeFactorAcc = (
            slopeFactor *
            Mathf.Sin(
                character.forwardAngle *
                Mathf.Deg2Rad
            )
        );

        if (Mathf.Abs(slopeFactorAcc) > character.stats.Get("slopeFactorAccThreshold"))
            accelerationMagnitude -= slopeFactorAcc;

        character.groundSpeed += accelerationMagnitude * deltaTime * 60F;
        accelerationPrev = accelerationMagnitude;
    }

    // Updates the character's animation while they're on the ground
    // 3D-Ready: NO
    void UpdateGroundAnim(float deltaTime) {
        character.spriteAnimatorSpeed = 1;

        // Check if we are transitioning to a rolling air state. If so, set the speed of it
        if (character.InStateGroup("rolling") && character.InStateGroup("air")) {
            character.spriteAnimatorSpeed = 1 + (
                (
                    Mathf.Abs(character.groundSpeed) /
                    character.stats.Get("topSpeedNormal")
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
            bool skidding = character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Skid");

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
                    Mathf.Abs(character.groundSpeed) >= character.stats.Get("skidThreshold")
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
                    if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Balancing")) {
                        if (
                            (character.facingRight && (character.balanceState == Character.BalanceState.Right)) ||
                            (!character.facingRight && (character.balanceState == Character.BalanceState.Left))
                        ) {
                            character.AnimatorPlay("Balancing Forwards");
                        } else {
                            character.AnimatorPlay("Balancing Backwards");
                        }
                    }
                    character.AnimatorPlay("Balancing");
                } else {
                    if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
                        character.AnimatorPlay("Idle");
                }
            // Pushing anim
            // ======================
            } else if (pushing) {
                if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Push"))
                    character.AnimatorPlay("Push");
    
                character.spriteAnimatorSpeed = 1 + (Mathf.Abs(character.groundSpeed) / character.stats.Get("topSpeedNormal"));
            // Skidding, again
            // ======================
            } else if ((canSkid || skidding) && !notSkidding) {
                if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Skid")) {
                    SFX.Play(character.audioSource, "sfxSkid");

                    if (Mathf.Abs(character.groundSpeed) < character.stats.Get("animSkidFastThreshold"))
                        character.AnimatorPlay("Skid");
                    else
                        character.AnimatorPlay("Skid Fast");
                }

            // Slow Walking
            // ======================
            } else if (Mathf.Abs(character.groundSpeed) < character.stats.Get("animWalkThreshold")) {
                if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Slow Walk"))
                    character.AnimatorPlay("Slow Walk");
    
                character.spriteAnimatorSpeed = 1 + (Mathf.Abs(character.groundSpeed) / character.stats.Get("topSpeedNormal"));

            // Walking
            // ======================
            } else if (Mathf.Abs(character.groundSpeed) < character.stats.Get("animRunThreshold")) {
                if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Walk"))
                    character.AnimatorPlay("Walk");
    
                character.spriteAnimatorSpeed = 1 + (Mathf.Abs(character.groundSpeed) / character.stats.Get("topSpeedNormal"));
            // Running Fast
            // ======================
            } else if (
                (Mathf.Abs(character.groundSpeed) >= character.stats.Get("animPeeloutThreshold")) &&
                GlobalOptions.GetBool("peelOut")
             ) {
                character.AnimatorPlay("Fast");
                character.spriteAnimatorSpeed = Mathf.Abs(character.groundSpeed) / character.stats.Get("topSpeedNormal");
            } else {
            // Running
            // ======================
                character.AnimatorPlay("Run");
                character.spriteAnimatorSpeed = Mathf.Abs(character.groundSpeed) / character.stats.Get("topSpeedNormal");
            }
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

        if (character.horizontalInputLockTimer > 0) return;
        if (Mathf.Abs(character.groundSpeed) >= character.stats.Get("fallThreshold")) return;

        if (!((character.forwardAngle <= 315) && (character.forwardAngle >= 45))) return;
        character.horizontalInputLockTimer = character.stats.Get("horizontalInputLockTime");

        if (!((character.forwardAngle < 271) && (character.forwardAngle > 89))) return;

        if (character.stateCurrent == "rolling")
            character.stateCurrent = "rollingAir";
        else character.stateCurrent = "air";
    }

    // 3D-Ready: YES
    void UpdateGroundTerminalSpeed() {
        character.groundSpeed = Mathf.Min(
            Mathf.Abs(character.groundSpeed),
            character.stats.Get("terminalSpeed")
        ) * Mathf.Sign(character.groundSpeed);
    }


    public override void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }

    // 3D-Ready: NO
    public override void OnCollisionEnter(Collision collision) {
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