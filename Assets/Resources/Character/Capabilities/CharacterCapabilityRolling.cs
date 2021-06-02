using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterCapabilityGround))]
public class CharacterCapabilityRolling : CharacterCapability {
    public float frictionRollNormal =  0.0234375F;
    public float frictionRollSpeedUp =  0.046875F;
    public float decelerationRoll =  0.125F;
    public float slopeFactorRollUp =  0.078125F;
    public float slopeFactorRollDown =  0.3125F;
    public float rollThreshold =  1.03125F;
    public float unrollThreshold = .5F;
    public float rollLockBoostSpeed = 3F;
    public bool rollRotate = false;
    public Transform rollingModeGroup;

    // ========================================================================

    public float frictionRoll => (
        character.HasEffect("speedUp") ?
            frictionRollSpeedUp :
            frictionRollNormal
    );

    public override void Init() {
        name = "rolling";
        character.AddStateGroup("harmful", "rolling");
        character.AddStateGroup("ground", "rolling");
        character.AddStateGroup("rolling", "rolling");

        character.AddStateGroup("harmful", "rollLock");
        character.AddStateGroup("ground", "rollLock");
        character.AddStateGroup("rolling", "rollLock");
        character.AddStateGroup("noJump", "rollLock");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (!(
            character.InStateGroup("rolling") &&
            character.InStateGroup("ground")
        )) return;

        character.modeGroupCurrent = rollingModeGroup;
    }

    public override void CharUpdate(float deltaTime) {
        if (character.stateCurrent == "ground") {
            if (character.pressingLeft || character.pressingRight) return;
            if (!character.input.GetAxisNegative("Vertical")) return;
            if (Mathf.Abs(character.groundSpeed) < rollThreshold * character.physicsScale) return;
            character.stateCurrent = "rolling";
            SFX.PlayOneShot(character.audioSource, "sfxRoll");
        }

        if (!(
            character.InStateGroup("rolling") &&
            character.InStateGroup("ground")
        )) return;
        UpdateRollingMove(deltaTime);
        UpdateRollingAnim(deltaTime);
        UpdateTerminalSpeed();
        character.GroundSnap();
    }

    void UpdateTerminalSpeed() {
        character.groundSpeed = Mathf.Min(
            Mathf.Abs(character.groundSpeed),
            character.terminalSpeed * character.physicsScale
        ) * Mathf.Sign(character.groundSpeed);
    }


    // Handles movement while rolling
    // See: https://info.sonicretro.org/SPG:Rolling
    // 3D-Ready: NO
    void UpdateRollingMove(float deltaTime) {
        float accelerationMagnitude = 0F;

        if (character.input.GetAxisNegative("Horizontal")) {
            if (character.groundSpeed > 0)
                accelerationMagnitude = -decelerationRoll * character.physicsScale;
        } else if (character.input.GetAxisPositive("Horizontal")) {
            if (character.groundSpeed < 0)
                accelerationMagnitude = decelerationRoll * character.physicsScale;
        }

        if (Mathf.Abs(character.groundSpeed) > 0.05F * character.physicsScale)
            accelerationMagnitude -= (
                Mathf.Sign(character.groundSpeed) *
                frictionRoll * character.physicsScale
            );

        bool movingUphill = (
            Mathf.Sign(character.groundSpeed) ==
            Mathf.Sign(Mathf.Sin(character.forwardAngle * Mathf.Deg2Rad))
        );


        float slopeFactor = (
            movingUphill ?
                slopeFactorRollUp * character.physicsScale :
                slopeFactorRollDown * character.physicsScale
        );
        accelerationMagnitude -= slopeFactor * Mathf.Sin(character.forwardAngle * Mathf.Deg2Rad);
        character.groundSpeed += accelerationMagnitude * deltaTime * 60F;

        // Unroll / roll lock boost
        if (Mathf.Abs(character.groundSpeed) < unrollThreshold * character.physicsScale) {
            if (character.stateCurrent == "rollLock") {
                if ((character.forwardAngle < 270) && (character.forwardAngle > 180))
                    character.facingRight = true;
                if ((character.forwardAngle > 45) && (character.forwardAngle < 180))
                    character.facingRight = false;

                character.groundSpeed = (
                    rollLockBoostSpeed * character.physicsScale *
                    (character.facingRight ? 1 : -1)
                );
            } else {
                character.groundSpeed = 0;
                character.stateCurrent = "ground";
            }
        }
    }

    // 3D-Ready: YES
    void UpdateRollingAnim(float deltaTime) {
        if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Roll"))
            character.AnimatorPlay("Roll");

        float topSpeedNormal = 0;
        character.WithCapability("ground", (CharacterCapability capability) => {
            topSpeedNormal = ((CharacterCapabilityGround)capability).topSpeedNormal;
        });

        character.spriteAnimatorSpeed = 1 + (
            (
                Mathf.Abs(character.groundSpeed) /
                topSpeedNormal * character.physicsScale
            ) * 2F
        );
        // ORDER MATTERS! GetSpriteRotation may depend on flipX for rotation-based flipping
        character.flipX = !character.facingRight;
        if (rollRotate)
            character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);
        else
            character.spriteContainer.eulerAngles = Vector3.zero;
    }
}