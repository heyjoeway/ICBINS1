using UnityEngine;

public class CharacterCapabilityDropdash : CharacterCapability {
    public CharacterCapabilityDropdash(Character character) : base(character) { }

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
            Input.GetKey(KeyCode.D) &&
            !character.controlLock
        ) DropDashRelease();
    }

    float dropDashTimer;

    public override void Update(float deltaTime) {
        if (character.stateCurrent != "jump") return;

        if (InputCustom.GetKeyDownPreventRepeat(KeyCode.D) && !character.controlLock)
            dropDashTimer = 0.33333F;

        if (Input.GetKey(KeyCode.D) && dropDashTimer > 0 && !character.controlLock) {
            dropDashTimer -= deltaTime;

            if (dropDashTimer <= 0)
                SFX.Play(character.audioSource, "SFX/Sonic 2/S2_60");
        }
    }

    // 3D-Ready: YES
    void DropDashRelease() {
        SFX.Play(character.audioSource, "SFX/Sonic 1/S1_BC");
        character.stateCurrent = "rolling";
        character.characterCamera.lagTimer = 0.26667F;

        GameObject dust = GameObject.Instantiate(
            (GameObject)Resources.Load("Objects/Dash Dust (Drop Dash)"),
            dustLocation.position,
            Quaternion.identity
        );
        dust.transform.localScale = character.spriteContainer.transform.localScale;

        float dashSpeed = 8F;
        float maxSpeed = 12F;
        if (!character.facingRight) {
            if (character.velocity.x <= 0) {
                character.groundSpeed = Mathf.Min(
                    -maxSpeed,
                    (character.groundSpeed / 4F) - dashSpeed
                );
                return;
            }
            if (Mathf.Floor(transform.rotation.z) > 0) {
                character.groundSpeed = (character.groundSpeed / 2F) - dashSpeed;
                return;
            }
            dashSpeed = -dashSpeed;
        } else {
            if (character.velocity.x >= 0) {
                float v5 = 
                character.groundSpeed = Mathf.Max(
                    dashSpeed + (character.groundSpeed / 4F),
                    maxSpeed
                );
                return;
            }
            if (Mathf.Floor(transform.rotation.z) > 0) {
                character.groundSpeed = dashSpeed + (character.groundSpeed / 2F);
                return;
            }
        }
        character.groundSpeed = dashSpeed * character.physicsScale;
    }
}