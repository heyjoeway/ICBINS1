using UnityEngine;
using System.Collections.Generic;

public class CharacterCapabilitySpindash : CharacterCapability {
    public float spindashPowerMax = 8F;
    public string[] buttonsSpindash = new string[] { "Secondary", "Tertiary" };
    public GameObject prefabSpindashDust;

    // ========================================================================

    Transform dustLocation;
    GameObject dust = null;
    float spindashPower;

    // ========================================================================

    public override void Init() {
        name = "spindash";
        character.AddStateGroup("noJump", "spindash");
        character.AddStateGroup("ground", "spindash");
        character.AddStateGroup("harmful", "spindash");

        dustLocation = character.spriteContainer.Find("Spindash Dust Position");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;

        dust = GameObject.Instantiate(
            prefabSpindashDust,
            dustLocation.position,
            Quaternion.identity
        );
        UpdateSpindashDust();

        spindashPower = -2F;
        SpindashCharge();
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;

        if (dust != null) {
            dust.GetComponent<Animator>().Play("Spindash End");
            dust = null;
        }
        
        character.groundedDetectorCurrent = null;
    }

    // See: https://info.sonicretro.org/SPG:Special_Abilities#Spindash_.28Sonic_2.2C_3.2C_.26_K.29
    public override void CharUpdate(float deltaTime) {
        if (character.stateCurrent == "ground") {
            // Switches the character to spindash state if connditions are met:
            // - Pressing spindash key combo
            // - Standing still
            if (!character.input.GetAxisNegative("Vertical")) return;
            if (!character.input.GetButtonsDownPreventRepeat(buttonsSpindash)) return;
            if (character.groundSpeed != 0) return;
            character.stateCurrent = name;
            return;
        } else if (character.stateCurrent != name) return;

        character.GroundSnap();
        character.groundSpeed = 0;
        character.velocity = Vector3.zero;
        
        UpdateSpindashDrain(deltaTime);
        UpdateSpindashInput();
        UpdateSpindashAnim(deltaTime);
        UpdateSpindashDust();
    }

    // 3D-Ready: YES
    void UpdateSpindashDust() {
        if (dust == null) return;
        dust.transform.position = dustLocation.position;
        dust.transform.localScale = character.spriteContainer.transform.localScale;
    }

    // 3D-Ready: YES
    void UpdateSpindashInput() {
        if (!character.input.GetAxisNegative("Vertical")) {
            SpindashRelease();
            return;
        }

        if (character.input.GetButtonsDownPreventRepeat(buttonsSpindash))
            SpindashCharge();
    }

    // 3D-Ready: YES
    void UpdateSpindashDrain(float deltaTime) {
        spindashPower -= ((spindashPower / 0.125F) / 256F) * deltaTime * 60F;
        spindashPower = Mathf.Max(0, spindashPower);
    }

    // 3D-Ready: YES
    void SpindashCharge() {
        character.AnimatorPlay("Spindash", "", 0);
        spindashPower += 2;
        SFX.Play(
            character.audioSource,
            "sfxSpindashCharge",
            1 + ((
                    spindashPower /
                    (spindashPowerMax + 2)
                ) * 0.5F
            ) // pitch
        );
    }

    // 3D-Ready: YES
    void SpindashRelease() {
        character.groundSpeed = (
            (character.facingRight ? 1 : -1) * 
            (8F + (Mathf.Floor(spindashPower) / 2F)) *
            character.physicsScale
        );
        character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
        if (character.characterCamera != null)
            character.characterCamera.lagTimer = 0.26667F;
        SFX.Play(character.audioSource, "sfxSpindashRelease");
        character.stateCurrent = "rolling";
    }

    // 3D-Ready: YES
    void UpdateSpindashAnim(float deltaTime) {
        // ORDER MATTERS! GetSpriteRotation may depend on flipX for rotation-based flipping
        character.flipX = !character.facingRight;
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);
    }
}