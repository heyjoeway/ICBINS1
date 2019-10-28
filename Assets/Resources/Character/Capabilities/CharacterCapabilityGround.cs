using UnityEngine;

public class CharacterCapabilityGround : CharacterCapability {
    const float accelerationGroundNormal = 0.046875F;
    const float accelerationGroundSpeedUp = 0.09375F;
    float accelerationGround { get {
        return (
            character.HasEffect("speedUp") ?
                accelerationGroundSpeedUp :
                accelerationGroundNormal
        ) * character.physicsScale;
    }}

    const float frictionGroundNormal = 0.046875F;
    const float frictionGroundSpeedUp = 0.09375F;
    float frictionGround { get {
        return (
            character.HasEffect("speedUp") ?
                frictionGroundSpeedUp :
                frictionGroundNormal
        ) * character.physicsScale;
    }}

    float decelerationGround { get {
        return 0.5F * character.physicsScale;
    }}

    float slopeFactorGround { get {
        return 0.125F * character.physicsScale;
    }}

    float skidThreshold { get {
        return 4.5F * character.physicsScale;
    }}

    float fallThreshold { get {
        return 2.5F * character.physicsScale;
    }}

    // ========================================================================
    

    public CharacterCapabilityGround(Character character) : base(character) { }

    bool pushing = false;

    public override void Init() {
        name = "ground";
        character.AddStateGroup("ground", "ground");
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

    // 3D-Ready: Sorta
    void UpdateGroundMove(float deltaTime) {
        float accelerationMagnitude = 0F;

        int inputDir = 0;
        if (character.horizontalInputLockTimer <= 0) {
            // ORDER MATTERS!
            if (character.input.GetAxesPositive("Horizontal")) inputDir = 1;
            if (character.input.GetAxesNegative("Horizontal")) inputDir = -1;
        } else character.horizontalInputLockTimer -= deltaTime;

        if (inputDir == 1) {
            if (character.groundSpeed < 0) {
                accelerationMagnitude = decelerationGround;
            } else if (character.groundSpeed < character.topSpeed) {
                accelerationMagnitude = accelerationGround;
            }
        } else if (inputDir == -1) {
            if (character.groundSpeed > 0) {
                accelerationMagnitude = -decelerationGround;
            } else if (character.groundSpeed > -character.topSpeed) {
                accelerationMagnitude = -accelerationGround;
            }
        } else {
            if (Mathf.Abs(character.groundSpeed) > 0.05F * character.physicsScale) {
                accelerationMagnitude = -Mathf.Sign(character.groundSpeed) * frictionGround;
            } else {
                character.groundSpeed = 0;
                accelerationMagnitude = 0;
            }
        }

        float slopeFactorAcc = slopeFactorGround * Mathf.Sin(character.forwardAngle * Mathf.Deg2Rad);
        if (Mathf.Abs(slopeFactorAcc) > 0.04)
            accelerationMagnitude -= slopeFactorAcc;

        character.groundSpeed += accelerationMagnitude * deltaTime * 60F;
    }

    // Updates the character's animation while they're on the ground
    // 3D-Ready: NO
    void UpdateGroundAnim(float deltaTime) {
        bool ignoreFlipX = false;
        character.spriteAnimatorSpeed = 1;

        // Check if we are transitioning to a rolling air state. If so, set the speed of it
        if (character.InStateGroup("rolling") && character.InStateGroup("air")) {
            character.spriteAnimatorSpeed = 1 + (
                (
                    Mathf.Abs(character.groundSpeed) /
                    character.topSpeedNormal
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
            bool skidding = (
                (character.pressingRight && character.groundSpeed < 0) ||
                (character.pressingLeft && character.groundSpeed > 0)
            );

            // You can only trigger a skid state if:
            // - Your angle (a) is <= 45d or >= 270d and your absolute speed is above the threshhold
            // - OR you're already skidding
            bool canSkid = (
                (
                    (
                        (character.forwardAngle <= 45F) ||
                        (character.forwardAngle >= 270F)
                    ) && (
                        Mathf.Abs(character.groundSpeed) >= skidThreshold
                    )
                ) || character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Skid")
            );

            // Standing still, looking up/down, idle animation
            // ======================
            if (character.groundSpeed == 0) {
                if (character.input.GetAxesNegative("Vertical"))
                    character.AnimatorPlay("Look Down");
                else if (character.input.GetAxesPositive("Vertical"))
                    character.AnimatorPlay("Look Up");
                else if (character.balanceState != Character.BalanceState.None) {
                    ignoreFlipX = true;
                    character.flipX = character.balanceState == Character.BalanceState.Right;
                    character.AnimatorPlay("Balancing");
                } else {
                    if (
                        !character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Tap") &&
                        !character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")
                    ) character.AnimatorPlay("Idle");
                }
            // Pushing anim
            // ======================
            } else if (pushing) {
                character.AnimatorPlay("Push");
                character.spriteAnimatorSpeed = 1 + (Mathf.Abs(character.groundSpeed) / character.topSpeedNormal);
            // Skidding, again
            // ======================
            } else if (skidding && canSkid) {
                if (!character.spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Skid"))
                    SFX.Play(character.audioSource, "sfxSkid");

                character.AnimatorPlay("Skid");
            // Walking
            // ======================
            } else if (Mathf.Abs(character.groundSpeed) < character.topSpeedNormal) {
                character.AnimatorPlay("Walk");
                character.spriteAnimatorSpeed = 1 + (Mathf.Abs(character.groundSpeed) / character.topSpeedNormal);
            // Running Fast
            // ======================
            } else if (
                (Mathf.Abs(character.groundSpeed) >= 10F * character.physicsScale) &&
                GlobalOptions.Get<bool>("peelOut")
             ) {
                character.AnimatorPlay("Fast");
                character.spriteAnimatorSpeed = Mathf.Abs(character.groundSpeed) / character.topSpeedNormal;
            } else {
            // Running
            // ======================
                character.AnimatorPlay("Run");
                character.spriteAnimatorSpeed = Mathf.Abs(character.groundSpeed) / character.topSpeedNormal;
            }
        }

        // Final value application
        // ======================
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);
        if (!ignoreFlipX) character.flipX = !character.facingRight;

        pushing = false;
    }


    // 3D-Ready: YES
    void UpdateGroundFallOff() {
        // SPG says these angles should be 270 and 90... I don't really believe that
        // That results in some wicked Sonic 4 walk up walls shenanigans
        // I imagine this is because of the differences in how angles are
        // represented between pixel-based and vector-based collision

        if (character.horizontalInputLockTimer > 0) return;
        if (Mathf.Abs(character.groundSpeed) >= fallThreshold) return;

        if (!((character.forwardAngle <= 315) && (character.forwardAngle >= 45))) return;
        character.horizontalInputLockTimer = 0.5F;

        if (!((character.forwardAngle <= 270) && (character.forwardAngle >= 90))) {
            return;
        }

        if (character.stateCurrent == "rolling")
            character.stateCurrent = "rollingAir";
        else character.stateCurrent = "air";
    }

    // 3D-Ready: YES
    void UpdateGroundTerminalSpeed() {
        character.groundSpeed = Mathf.Min(
            Mathf.Abs(character.groundSpeed),
            character.terminalSpeed
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