using UnityEngine;
using System.Collections.Generic;

public class CharacterCapabilityHurt : CharacterCapability {
    float failsafeTimer;

    public CharacterCapabilityHurt(Character character) : base(character) { }

    public override void Init() {
        name = "hurt";
        character.AddStateGroup("airCollision", "hurt");
        character.AddStateGroup("ignore", "hurt");

        character.stats.Add(new Dictionary<string, object>() {
            ["hurtGravity"] = -0.1875F
        });
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;
        character.opacity = 1;
        character.modeGroupCurrent = character.airModeGroup;
        character.forwardAngle = 0;
        failsafeTimer = 5F;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;
        character.groundSpeed = 0;
        character.effects.Add(new CharacterEffect(character, "invulnerable", 2));
    }

    public override void Update(float deltaTime) {
        if (character.stateCurrent != name) {
            character.opacity = 1;

            CharacterEffect effectInvuln = character.GetEffect("invulnerable");
            if (effectInvuln == null) return;
            float invulnTimer = effectInvuln.duration;

            int frame = (int)Mathf.Round(invulnTimer * 60);
            character.opacity = (frame % 8) > 3 ? 1 : 0;
            return;
        }

        character.velocity += new Vector3(
            0,
            character.stats.Get("hurtGravity"),
            0
        ) * deltaTime * 60F;

        character.AnimatorPlay("Hurt");
        character.spriteContainer.eulerAngles = Vector3.zero;
        character.flipX = !character.facingRight;

        failsafeTimer -= deltaTime;
        if (failsafeTimer <= 0)
            character.stateCurrent = "air";
    }
}