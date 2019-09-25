using UnityEngine;

public class CharacterCapabilityHurt : CharacterCapability {
    float hurtGravity { get {
        return -0.1875F * character.physicsScale;
    }}

    public CharacterCapabilityHurt(Character character) : base(character) { }

    public override void Init() {
        name = "hurt";
        character.AddStateGroup("airCollision", "hurt");
        character.AddStateGroup("ignore", "hurt");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;
        character.opacity = 1;
        character.modeGroupCurrent = character.airModeGroup;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;
        character.groundSpeed = 0;
        character.effects.Add(new CharacterEffect("invulnerable", 2));
    }

    public override void Update(float deltaTime) {
        if (character.stateCurrent != name) return;

        character.velocity += new Vector3(
            0,
            hurtGravity,
            0
        ) * deltaTime * 60F;

        character.spriteAnimator.Play("Hurt");
        character.spriteContainer.transform.position = character.position;
        character.spriteContainer.eulerAngles = Vector3.zero;
        character.flipX = !character.facingRight;
    }
}