using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(CharacterCapabilityGround))]
public class CharacterCapabilityBoostMode : CharacterCapability {
    public float boostModeTime = 3F;
    public float boostModeLowerThreshold = 6F;
    public float boostModeUpperThreshold = 10F;

    // ========================================================================

    CharacterEffect afterImageEffect = null;
    CharacterEffect speedUpEffect = null;

    void ExitBoostMode() {
        if (afterImageEffect != null) {
            afterImageEffect.DestroyBase();
            afterImageEffect = null;
        }

        if (speedUpEffect != null) {
            speedUpEffect.DestroyBase();
            speedUpEffect = null;
            boostModeTimer = 0;
        }
    }

    void EnterBoostMode(bool visualEffect = true) {
        if (afterImageEffect == null) {
            afterImageEffect = new CharacterEffect(character, "afterImage");
            character.effects.Add(afterImageEffect);
        }

        if (speedUpEffect == null) {
            speedUpEffect = new CharacterEffect(character, "speedUp");
            character.effects.Add(speedUpEffect);
            if (character.characterCamera != null)
                character.characterCamera.lagTimer = 0.26667F / 2;

            // SFX.PlayOneShot(character.audioSource, "sfxBoostMode");
        }
    }

    float boostModeTimer = 0;
    public override void CharUpdate(float deltaTime) {
        if (!character.InStateGroup("ground")) return;
        if (Mathf.Abs(character.groundSpeed) < boostModeLowerThreshold * character.physicsScale)
            ExitBoostMode();

        if (character.HasEffect("boosting"))
            EnterBoostMode(false);

        if (Mathf.Abs(character.groundSpeed) >= boostModeUpperThreshold * character.physicsScale) {
            boostModeTimer += deltaTime;

            if (boostModeTimer >= boostModeTime)
                EnterBoostMode();
        } else boostModeTimer = 0;
    }
}