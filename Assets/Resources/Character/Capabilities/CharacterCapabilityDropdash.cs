using UnityEngine;

public class CharacterCapabilityDropdash : CharacterCapability {
    public CharacterCapabilityDropdash(Character character) : base(character) { }
    string[] buttonsDropDash = new string[] { "Secondary", "Tertiary" };

    Transform dustLocation;

    public override void Init() {
        name = "dropdash";

        dustLocation = character.spriteContainer.Find("Spindash Dust Position");
    }

    public override void StateInit(string stateName, string prevStateName) {
        dropDashTimer = Mathf.Infinity;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (stateName != "jump") return;
        if (nextStateName != "ground") return;
        if (
            (dropDashTimer <= 0) &&
            character.input.GetButtons(buttonsDropDash)
        ) DropDashRelease();
    }

    float dropDashTimer;

    public override void Update(float deltaTime) {
        if (character.stateCurrent != "jump") return;

        if (!character.input.GetButtons(buttonsDropDash)) {
            character.AnimatorPlay("Roll");
            dropDashTimer = Mathf.Infinity;
        }

        if (character.input.GetButtonsDownPreventRepeat(buttonsDropDash))
            dropDashTimer = 0.33333F;

        if (character.input.GetButtons(buttonsDropDash) && dropDashTimer > 0) {
            dropDashTimer -= deltaTime;

            if (dropDashTimer <= 0) {
                SFX.Play(character.audioSource, "sfxDropDashCharge");
                character.AnimatorPlay("Drop Dash");
            }
        }
    }

    // 3D-Ready: YES
    void DropDashRelease() {
        SFX.Play(character.audioSource, "sfxDropDashRelease");
        character.stateCurrent = "rolling";
        if (character.characterCamera != null)
            character.characterCamera.lagTimer = 0.26667F;

        GameObject dust = GameObject.Instantiate(
            Constants.Get<GameObject>("prefabDropDashDust"),
            dustLocation.position,
            Quaternion.identity
        );
        dust.transform.localScale = character.spriteContainer.transform.localScale;

        float dashSpeed = 8F * character.physicsScale;
        float maxSpeed = 12F * character.physicsScale;
        if (!character.facingRight) {
            if (character.velocity.x <= 0) {
                character.groundSpeed = Mathf.Max(
                    -maxSpeed,
                    (character.groundSpeed / 4F) - dashSpeed
                );
                character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
            } else if (Mathf.Floor(transform.rotation.z) > 0) {
                character.groundSpeed = (character.groundSpeed / 2F) - dashSpeed;
                character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
            } else character.groundSpeed = -dashSpeed;
        } else {
            if (character.velocity.x >= 0) {
                character.groundSpeed = Mathf.Min(
                    dashSpeed + (character.groundSpeed / 4F),
                    maxSpeed
                );
                character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
            } else if (Mathf.Floor(transform.rotation.z) > 0) {
                character.groundSpeed = dashSpeed + (character.groundSpeed / 2F);
                character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
            } else character.groundSpeed = dashSpeed;
        }
 
        character.groundSpeedPrev = character.groundSpeed; // Hack for breakable walls
    }
}