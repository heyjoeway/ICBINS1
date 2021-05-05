using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterCapabilityBoostMode : CharacterCapability {
    public CharacterCapabilityBoostMode(Character character) : base(character) { }

    public override void Init() {
        character.stats.Add(new Dictionary<string, object>() {
            ["boostModeTime"] = 3F,
            ["boostModeLowerThreshold"] = 6F,
            ["boostModeUpperThreshold"] = 10F
        });
    }

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
    public override void Update(float deltaTime) {
        if (!character.InStateGroup("ground")) return;
        if (Mathf.Abs(character.groundSpeed) < character.stats.Get("boostModeLowerThreshold"))
            ExitBoostMode();

        if (character.HasEffect("boosting"))
            EnterBoostMode(false);

        // if (Mathf.Abs(character.groundSpeed) > character.stats.Get("boostModeUpperThreshold"))
            // EnterBoostMode();

        if (Mathf.Abs(character.groundSpeed) >= character.stats.Get("topSpeedNormal")) {
            boostModeTimer += deltaTime;

            if (boostModeTimer >= character.stats.GetRaw("boostModeTime"))
                EnterBoostMode();
        } else boostModeTimer = 0;
    }
}