using UnityEngine;
using System.Collections.Generic;

public class CharacterCapabilityJump : CharacterCapability {
    string[] buttonsJump = new string[] { "Secondary", "Tertiary" };
    string[] buttonsJumpHold = new string[] { "Primary", "Secondary", "Tertiary" };

    // ========================================================================

    public CharacterCapabilityJump(Character character) : base(character) { }

    public override void Init() {
        name = "jump";
        character.AddStateGroup("airCollision", "jump");
        character.AddStateGroup("air", "jump");
        character.AddStateGroup("rolling", "jump");
        character.AddStateGroup("jump", "jump");
        character.AddStateGroup("harmful", "jump");

        character.stats.Add(new Dictionary<string, object>() {
            ["jumpSpeed"] = 6.5F
        });
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
        if (!character.input.GetButtonsDownPreventRepeat(buttonsJump)) return;

        character.velocity += transform.up * character.stats.Get("jumpSpeed");
        SFX.PlayOneShot(character.audioSource, "sfxJump");
        character.stateCurrent = "jump";
    }

    // 3D-Ready: YES
    void UpdateJumpHeight() {
        if (!character.input.GetButtons(buttonsJumpHold)) {
            if (character.velocity.y > 4 * character.physicsScale)
                character.velocity = new Vector3(
                    character.velocity.x,
                    4 * character.physicsScale,
                    character.velocity.z
                );
        }
    }
}