using UnityEngine;
using System.Collections.Generic;

public class CharacterCapabilityDeath : CharacterCapability {
    public float gravityDying = -0.21875F;
    public float timeDying = 3F;
    public Vector3 speedDying = new Vector3(0, 7, 0);

    // ========================================================================
 
    float dyingTimer;

    public override void Init() {
        name = "dying";
        character.AddStateGroup("death", "dying");
        character.AddStateGroup("death", "drowning");
        character.AddStateGroup("death", "dead");
        character.AddStateGroup("noControl", "dying");
        character.AddStateGroup("noControl", "drowning");
        character.AddStateGroup("noControl", "dead");
        character.AddStateGroup("ignore", "dying");
        character.AddStateGroup("ignore", "drowning");
        character.AddStateGroup("ignore", "dead");
        character.AddStateGroup("dying", "dying");
        character.AddStateGroup("dying", "drowning");
        character.AddStateGroup("dead", "dead");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (!character.InStateGroup("death")) return;
        character.modeGroupCurrent = null;

        if (character.InStateGroup("dying")) {
            if (LevelManager.current.characters.Count == 1)
                Time.timeScale = 0;

            character.ClearEffects();
            dyingTimer = timeDying;

            if (character.stateCurrent == "drowning") {
                character.velocity = Vector3.zero;
                SFX.Play(character.audioSource, "sfxDrown");
                character.AnimatorPlay("Drowning");
            } else if (character.stateCurrent == "dying") {
                character.velocity = speedDying * character.physicsScale;
                SFX.Play(character.audioSource, "sfxDie");
                character.AnimatorPlay("Dying");
            }
        } else if (character.stateCurrent == "dead") {
            character.lives--;
            if (LevelManager.current.characters.Count == 1) {
                character.currentLevel.ReloadFadeOut(character);
            } else character.SoftRespawn();
        }
    }

    public override void CharUpdate(float deltaTime) {
        if (!character.InStateGroup("dying")) return;

        character.velocity += (
            Vector3.up *
            gravityDying *
            character.physicsScale *
            deltaTime * 60F
        );
        
        dyingTimer -= deltaTime;
        if (dyingTimer <= 0) {
            character.stateCurrent = "dead";
            return;
        }
        
        character.spriteContainer.position = character.position;
        character.spriteContainer.eulerAngles = Vector3.zero;
        character.flipX = false;

        if (Time.timeScale == 0)
            character.position += character.velocity * deltaTime;
    }
}