
using UnityEngine;

public class CharacterCapabilityVictory : CharacterCapability {
    [HideInInspector]
    public bool victoryLock = false;

    public override void Init() {
        name = "victory";
    }

    public override void CharUpdate(float deltaTime) {
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