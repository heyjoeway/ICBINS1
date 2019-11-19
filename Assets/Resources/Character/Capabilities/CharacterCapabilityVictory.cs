
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
            if (character.stateCurrent == "victory") {
                character.velocity = Vector3.zero;
                character.groundSpeed = 0;
                character.GroundSnap();
                return;
            } else if (!character.InStateGroup("ground")) return;

            character.modeGroupCurrent = null;
            character.facingRight = true;
            character.stateCurrent = "victory";
            character.AnimatorPlay("Victory");
            character.spriteAnimatorSpeed = 1;
        }
    }
}