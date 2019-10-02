using UnityEngine;

public class CharacterCapabilityJump : CharacterCapability {
    const float jumpSpeedNormal = 6.5F;
    public float jumpSpeed { get { return jumpSpeedNormal * character.physicsScale; }}

    // ========================================================================

    public CharacterCapabilityJump(Character character) : base(character) { }

    public override void Init() {
        name = "jump";
        character.AddStateGroup("airCollision", "jump");
        character.AddStateGroup("air", "jump");
        character.AddStateGroup("rolling", "jump");
        character.AddStateGroup("jump", "jump");
        character.AddStateGroup("harmful", "jump");
    }

    public override void Update(float deltaTime) {
        if (character.InStateGroup("ground")) {
            UpdateGroundJump();
            return;
        }

        if (!character.InStateGroup("jump")) return;
        UpdateJumpHeight();
    }

    // Switches the character to jump state if connditions are met:
    // - Pressing jump key
    // See: https://info.sonicretro.org/SPG:Solid_Tiles
    // 3D-Ready: YES
    void UpdateGroundJump() {
        if (character.InStateGroup("noJump")) return;
        if (character.controlLock) return;
        if (!InputCustom.GetButtonsDownPreventRepeat("Secondary", "Tertiary")) return;

        character.velocity += transform.up * jumpSpeed;
        SFX.PlayOneShot(character.audioSource, "SFX/Sonic 1/S1_A0");
        character.stateCurrent = "jump";
    }

    // 3D-Ready: YES
    void UpdateJumpHeight() {
        if (!InputCustom.GetButtons("Primary", "Secondary", "Tertiary") || character.controlLock) {
            if (character.velocity.y > 4 * character.physicsScale)
                character.velocity = new Vector3(
                    character.velocity.x,
                    4 * character.physicsScale,
                    character.velocity.z
                );
        }
    }
}