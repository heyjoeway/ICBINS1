using UnityEngine;

public class CharacterCapabilityPeelOut : CharacterCapability {
    float peelOutTimer;

    // ========================================================================

    Transform dustLocation;
    GameObject dust = null;

    // ========================================================================

    public CharacterCapabilityPeelOut(Character character) : base(character) { }

    public override void Init() {
        name = "peelOut";
        character.AddStateGroup("noJump", "spindash");
        character.AddStateGroup("ground", "spindash");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;

        peelOutTimer = 0.5F;
        SFX.Play(character.audioSource, "SFX/Sonic CD/SCD_FM_11");
        character.modeGroupCurrent = character.groundModeGroup;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;
        character.groundedDetectorCurrent = null;
    }

    // See: https://info.sonicretro.org/SPG:Special_Abilities#Spindash_.28Sonic_2.2C_3.2C_.26_K.29
    public override void Update(float deltaTime) {
        if (character.stateCurrent == "ground") {
            // Switches the character to spindash state if connditions are met:
            // - Pressing spindash key combo
            // - Standing still
            if (character.controlLock) return;
            if (!InputCustom.GetAxesPositive("Vertical")) return;
            if (!InputCustom.GetButtonsDownPreventRepeat("Secondary", "Tertiary")) return;
            if (character.groundSpeed != 0) return;
            character.stateCurrent = name;
            return;
        } else if (character.stateCurrent != name) return;

        character.GroundSnap();
        peelOutTimer -= deltaTime;
        peelOutTimer = Mathf.Max(0, peelOutTimer);
        UpdateSpindashInput();
        UpdateSpindashAnim(deltaTime);
    }

    // 3D-Ready: YES
    void UpdateSpindashInput() {
        if (!InputCustom.GetAxesPositive("Vertical") || character.controlLock)
            SpindashRelease();
    }

    // 3D-Ready: YES
    void SpindashRelease() {
        if (peelOutTimer <= 0) {
            character.groundSpeed = 12F * character.physicsScale * (character.flipX ? -1 : 1);
            character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
            character.characterCamera.lagTimer = 0.26667F;
            SFX.Play(character.audioSource, "SFX/Sonic CD/SCD_FM_01");
        }
        character.stateCurrent = "ground";
    }

    // 3D-Ready: YES
    void UpdateSpindashAnim(float deltaTime) {
        character.spriteContainer.transform.position = character.position;
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);
        character.flipX = !character.facingRight;

        float runSpeed = (1F - (peelOutTimer / 0.5F)) * 12F;
        character.spriteAnimator.speed = runSpeed / character.topSpeedNormal;

        if (runSpeed < 6F) {
            character.spriteAnimator.Play("Walk");
            character.spriteAnimator.speed = 1 + (runSpeed / character.topSpeedNormal);
        } else if (runSpeed >= 12F)
            character.spriteAnimator.Play("Fast");
        else character.spriteAnimator.Play("Run");
    }
}