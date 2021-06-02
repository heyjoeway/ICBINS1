using UnityEngine;
using System;

public class CharacterEffectShield : CharacterEffect {
    ObjShield shield;

    public CharacterEffectShield(Character character)
        : base(character, "shield") {

        if (!character.HasEffect("shield")) {
            shield = GameObject.Instantiate(
                Constants.Get<GameObject>("prefabShieldNormal")
            ).GetComponent<ObjShield>();
            shield.character = character;
        }

        SFX.Play(character.audioSource, "sfxShieldNormal");
    }

    public override void Destroy() { GameObject.Destroy(shield); }
}