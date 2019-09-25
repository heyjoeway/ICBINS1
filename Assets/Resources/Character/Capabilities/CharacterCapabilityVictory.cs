
using UnityEngine;

public class CharacterCapabilityVictory : CharacterCapability {
    public bool victoryLock = false;

    public CharacterCapabilityVictory(Character character) : base(character) { }

    public override void Init() {
        name = "victory";
    }

    public override void Update(float deltaTime) {
        if (!victoryLock) {
            if (character.stateCurrent == "victory")
                character.stateCurrent = "ground";
        } else {
            character.facingRight = true;
            character.stateCurrent = "victory";
            character.spriteAnimator.Play("Victory");
            character.spriteAnimator.speed = 1;
            character.velocity = Vector3.zero;
            character.groundSpeed = 0;
        }
    }
}