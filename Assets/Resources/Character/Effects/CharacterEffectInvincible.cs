using UnityEngine;
using System;

public class CharacterEffectInvincible : CharacterEffect {
    MusicManager.MusicStackEntry musicStackEntry;

    public CharacterEffectInvincible(Character character, float duration = 20F)
        : base(character, "invincible", duration) {

        if (!character.HasEffect("invincible")) {
            ObjShield stars = GameObject.Instantiate(
                Constants.Get<GameObject>("prefabInvincibilityStars")
            ).GetComponent<ObjShield>();
            stars.character = character;
        }

        musicStackEntry = new MusicManager.MusicStackEntry {
            introPath = "Music/Invincibility Intro",
            loopPath = "Music/Invincibility Loop",
            priority = 1
        };
        MusicManager.current.Add(musicStackEntry);
    }

    public override void Destroy() {
        MusicManager.current.Remove(musicStackEntry);
    }
}