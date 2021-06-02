using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterCapabilityGround))]
public class CharacterCapabilityPeelOut : CharacterCapability {
    float peelOutSpeed = 12F;
    public string[] buttonsPeelOut = new string[] { "Secondary", "Tertiary" };

    // ========================================================================

    float peelOutTimer;
    Transform dustLocation;
    GameObject dust = null;

    // ========================================================================

    public override void Init() {
        name = "peelOut";
        character.AddStateGroup("noJump", "spindash");
        character.AddStateGroup("ground", "spindash");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;

        peelOutTimer = 0.5F;
        SFX.Play(character.audioSource, "sfxPeelOutCharge");
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;
        character.groundedDetectorCurrent = null;
    }

    // See: https://info.sonicretro.org/SPG:Special_Abilities#Spindash_.28Sonic_2.2C_3.2C_.26_K.29
    public override void CharUpdate(float deltaTime) {
        if (character.stateCurrent == "ground") {
            // Switches the character to spindash state if connditions are met:
            // - Pressing spindash key combo
            // - Standing still
            if (!character.input.GetAxisPositive("Vertical")) return;
            if (!character.input.GetButtonsDownPreventRepeat(buttonsPeelOut)) return;
            if (character.groundSpeed != 0) return;
            character.stateCurrent = name;
            return;
        } else if (character.stateCurrent != name) return;

        character.GroundSnap();
        character.groundSpeed = 0;
        character.velocity = Vector3.zero;

        peelOutTimer -= deltaTime;
        peelOutTimer = Mathf.Max(0, peelOutTimer);
        UpdateSpindashInput();
        UpdateSpindashAnim(deltaTime);
    }

    // 3D-Ready: YES
    void UpdateSpindashInput() {
        if (!character.input.GetAxisPositive("Vertical"))
            SpindashRelease();
    }

    // 3D-Ready: YES
    void SpindashRelease() {
        if (peelOutTimer <= 0) {
            character.groundSpeed = peelOutSpeed * character.physicsScale * (character.flipX ? -1 : 1);
            character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
            if (character.characterCamera != null)
                character.characterCamera.lagTimer = 0.26667F;
            SFX.Play(character.audioSource, "sfxPeelOutRelease");
        }
        character.stateCurrent = "ground";
    }

    // 3D-Ready: YES
    void UpdateSpindashAnim(float deltaTime) {
        // ORDER MATTERS! GetSpriteRotation may depend on flipX for rotation-based flipping
        character.flipX = !character.facingRight;
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);

        float topSpeedNormal = 0;
        character.WithCapability("ground", (CharacterCapability capability) => {
            topSpeedNormal = ((CharacterCapabilityGround)capability).topSpeedNormal;
        });

        float runSpeed = (1F - (peelOutTimer / 0.5F)) * 12F;
        character.spriteAnimatorSpeed = runSpeed / topSpeedNormal * character.physicsScale;

        if (runSpeed < 6F) {
            character.AnimatorPlay("Walk");
            character.spriteAnimatorSpeed = 1 + (runSpeed / topSpeedNormal * character.physicsScale);
        } else if (runSpeed >= 12F)
            character.AnimatorPlay("Fast");
        else character.AnimatorPlay("Run");
    }
}