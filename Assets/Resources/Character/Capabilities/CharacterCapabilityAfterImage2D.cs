using UnityEngine;
using ActionCode2D.Renderers;

public class CharacterCapabilityAfterImage2D : CharacterCapability {
    SpriteGhostTrailRenderer spriteGhostTrail;

    public CharacterCapabilityAfterImage2D(Character character) : base(character) { }

    public override void Init() {
        spriteGhostTrail = character.sprite.GetComponent<SpriteGhostTrailRenderer>();
    }

    public override void Update(float deltaTime) {
        spriteGhostTrail.enabled = (
            character.HasEffect("afterImage") ||
            character.HasEffect("speedUp")
        );
    }
}