using UnityEngine;

[RequireComponent(typeof(CharacterCapabilityGround))]
[RequireComponent(typeof(CharacterCapabilityAir))]
[RequireComponent(typeof(CharacterCapabilityRolling))]
public class CharacterCapabilityRollingAir : CharacterCapability {
    public override void Init() {
        name = "rollingAir";
        character.AddStateGroup("airCollision", "rollingAir");
        character.AddStateGroup("harmful", "rollingAir");
        character.AddStateGroup("air", "rollingAir");
        character.AddStateGroup("rolling", "rollingAir");
    }

    public override void StateInit(string stateName, string prevStateName) {       
        if (!character.InStateGroup("air") || !character.InStateGroup("rolling")) return;
        float topSpeedNormal = 0;
        character.WithCapability("ground", (CharacterCapability capability) => {
            topSpeedNormal = ((CharacterCapabilityGround)capability).topSpeedNormal;
        });

        character.spriteAnimatorSpeed = 1 + ((Mathf.Abs(character.groundSpeed) / topSpeedNormal * character.physicsScale) * 2F);
    }

    public override void CharUpdate(float deltaTime) {
        if (character.InStateGroup("air") && character.InStateGroup("rolling")) {
            character.AnimatorPlay("Roll", "Roll");
        }
        
        if (!character.InStateGroup("air") || !character.InStateGroup("rolling")) {
            if (!GlobalOptions.GetBool("airCurling")) return;
            if (character.InStateGroup("air") && character.input.GetButtonDownPreventRepeat("Primary"))
                character.stateCurrent = "jump";
            else return;
        };
        character.eulerAngles = Vector3.zero;
    }
}