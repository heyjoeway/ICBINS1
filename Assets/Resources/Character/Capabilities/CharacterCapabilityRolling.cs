using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterCapabilityRolling : CharacterCapability {
    public CharacterCapabilityRolling(Character character) : base(character) { }

    public override void Init() {
        name = "rolling";
        character.AddStateGroup("harmful", "rolling");
        character.AddStateGroup("ground", "rolling");
        character.AddStateGroup("rolling", "rolling");

        character.AddStateGroup("harmful", "rollLock");
        character.AddStateGroup("ground", "rollLock");
        character.AddStateGroup("rolling", "rollLock");
        character.AddStateGroup("noJump", "rollLock");

        character.stats.Add(new Dictionary<string, object>() {
            ["frictionRollNormal"] =  0.0234375F,
            ["frictionRollSpeedUp"] =  0.046875F,
            ["decelerationRoll"] =  0.125F,
            ["slopeFactorRollUp"] =  0.078125F,
            ["slopeFactorRollDown"] =  0.3125F,
            ["rollThreshold"] =  1.03125F,
            ["unrollThreshold"] = .5F,
            ["frictionRoll"] = (Func<string>)(() =>
                character.HasEffect("speedUp") ?
                    "frictionRollSpeedUp" :
                    "frictionRollNormal"
            )
        });
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (!(
            character.InStateGroup("rolling") &&
            character.InStateGroup("ground")
        )) return;

        character.modeGroupCurrent = character.rollingModeGroup;
    }

    public override void Update(float deltaTime) {
        if (character.stateCurrent == "ground") {
            if (character.pressingLeft || character.pressingRight) return;
            if (!character.input.GetAxisNegative("Vertical")) return;
            if (Mathf.Abs(character.groundSpeed) < character.stats.Get("rollThreshold")) return;
            character.stateCurrent = "rolling";
            SFX.PlayOneShot(character.audioSource, "sfxRoll");
        }

        if (!(
            character.InStateGroup("rolling") &&
            character.InStateGroup("ground")
        )) return;
        UpdateRollingMove(deltaTime);
        UpdateRollingAnim();
        UpdateTerminalSpeed();
        character.GroundSnap();
    }

    void UpdateTerminalSpeed() {
        character.groundSpeed = Mathf.Min(
            Mathf.Abs(character.groundSpeed),
            character.stats.Get("terminalSpeed")
        ) * Mathf.Sign(character.groundSpeed);
    }


    // Handles movement while rolling
    // See: https://info.sonicretro.org/SPG:Rolling
    // 3D-Ready: NO
    void UpdateRollingMove(float deltaTime) {
        float accelerationMagnitude = 0F;

        if (character.input.GetAxisNegative("Horizontal")) {
            if (character.groundSpeed > 0)
                accelerationMagnitude = -character.stats.Get("decelerationRoll");
        } else if (character.input.GetAxisPositive("Horizontal")) {
            if (character.groundSpeed < 0)
                accelerationMagnitude = character.stats.Get("decelerationRoll");
        }

        if (Mathf.Abs(character.groundSpeed) > 0.05F * character.physicsScale)
            accelerationMagnitude -= (
                Mathf.Sign(character.groundSpeed) *
                character.stats.Get("frictionRoll")
            );

        float slopeFactor = (
            character.movingUphill ?
                character.stats.Get("slopeFactorRollUp") :
                character.stats.Get("slopeFactorRollDown")
        );
        accelerationMagnitude -= slopeFactor * Mathf.Sin(character.forwardAngle * Mathf.Deg2Rad);
        character.groundSpeed += accelerationMagnitude * deltaTime * 60F;

        // Unroll / roll lock boost
        if (Mathf.Abs(character.groundSpeed) < character.stats.Get("unrollThreshold")) {
            if (character.stateCurrent == "rollLock") {
                if ((character.forwardAngle < 270) && (character.forwardAngle > 180))
                    character.facingRight = true;
                if ((character.forwardAngle > 45) && (character.forwardAngle < 180))
                    character.facingRight = false;

                character.groundSpeed = (
                    character.stats.Get("rollLockBoostSpeed") *
                    (character.facingRight ? 1 : -1)
                );
            } else {
                character.groundSpeed = 0;
                character.stateCurrent = "ground";
            }
        }
    }

    // 3D-Ready: YES
    void UpdateRollingAnim() {
        character.AnimatorPlay("Roll");
        character.spriteAnimatorSpeed = 1 + (
            (
                Mathf.Abs(character.groundSpeed) /
                character.stats.Get("topSpeedNormal")
            ) * 2F
        );
        character.spriteContainer.eulerAngles = Vector3.zero;
        character.flipX = !character.facingRight;
    }
}