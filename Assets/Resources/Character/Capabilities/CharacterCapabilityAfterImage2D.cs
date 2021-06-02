using UnityEngine;
using ActionCode2D.Renderers;

public class CharacterCapabilityAfterImage2D : CharacterCapability {
    SpriteGhostTrailRenderer spriteGhostTrail;
    public override void Init() {
        spriteGhostTrail = character.sprite.GetComponent<SpriteGhostTrailRenderer>();
    }

    public override void CharUpdate(float deltaTime) {
        spriteGhostTrail.enabled = (
            character.HasEffect("afterImage") ||
            character.HasEffect("speedUp")
        );
    }
}