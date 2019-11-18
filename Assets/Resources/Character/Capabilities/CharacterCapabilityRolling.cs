using UnityEngine;

public class CharacterCapabilityRolling : CharacterCapability {
    float frictionRollNormal { get { 
        return 0.0234375F * character.physicsScale;
    }}
    float frictionRollSpeedUp { get { 
        return 0.046875F * character.physicsScale;
    }}
    float frictionRoll { get {
        return character.HasEffect("speedUp") ?
            frictionRollSpeedUp :
            frictionRollNormal;
    }}
    float decelerationRoll { get { 
        return 0.125F * character.physicsScale;
    }}
    float slopeFactorRollUp { get { 
        return 0.078125F * character.physicsScale;
    }}
    float slopeFactorRollDown { get { 
        return 0.3125F * character.physicsScale;
    }}
    float rollThreshold { get { 
        return 1.03125F * character.physicsScale;
    }}
    float unrollThreshold { get {
        return 0.5F * character.physicsScale;
    }}

    float rollLockBoostSpeed { get { 
        return 3F * character.physicsScale;
    }}

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
            if (Mathf.Abs(character.groundSpeed) < rollThreshold) return;
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
            character.terminalSpeed
        ) * Mathf.Sign(character.groundSpeed);
    }


    // Handles movement while rolling
    // See: https://info.sonicretro.org/SPG:Rolling
    // 3D-Ready: NO
    void UpdateRollingMove(float deltaTime) {
        float accelerationMagnitude = 0F;

        if (character.input.GetAxisNegative("Horizontal")) {
            if (character.groundSpeed > 0)
                accelerationMagnitude = -decelerationRoll;
        } else if (character.input.GetAxisPositive("Horizontal")) {
            if (character.groundSpeed < 0)
                accelerationMagnitude = decelerationRoll;
        }

        if (Mathf.Abs(character.groundSpeed) > 0.05F * character.physicsScale)
            accelerationMagnitude -= Mathf.Sign(character.groundSpeed) * frictionRoll;

        float slopeFactor = character.movingUphill ? slopeFactorRollUp : slopeFactorRollDown;
        accelerationMagnitude -= slopeFactor * Mathf.Sin(character.forwardAngle * Mathf.Deg2Rad);
        character.groundSpeed += accelerationMagnitude * deltaTime * 60F;

        // Unroll / roll lock boost
        if (Mathf.Abs(character.groundSpeed) < unrollThreshold) {
            if (character.stateCurrent == "rollLock") {
                if ((character.forwardAngle < 270) && (character.forwardAngle > 180))
                    character.facingRight = true;
                if ((character.forwardAngle > 45) && (character.forwardAngle < 180))
                    character.facingRight = false;

                character.groundSpeed = rollLockBoostSpeed * (character.facingRight ? 1 : -1);
            } else {
                character.groundSpeed = 0;
                character.stateCurrent = "ground";
            }
        }
    }

    // 3D-Ready: YES
    void UpdateRollingAnim() {
        character.AnimatorPlay("Roll");
        character.spriteAnimatorSpeed = 1 + ((Mathf.Abs(character.groundSpeed) / character.topSpeedNormal) * 2F);
        character.spriteContainer.eulerAngles = Vector3.zero;
        character.flipX = !character.facingRight;
    }
}