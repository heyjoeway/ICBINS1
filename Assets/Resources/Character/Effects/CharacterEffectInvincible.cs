using UnityEngine;
using System;

public class CharacterEffectInvincible : CharacterEffect {
    ObjShield stars;
    MusicManager.MusicStackEntry musicStackEntry;

    public CharacterEffectInvincible(Character character, float duration = 20F)
        : base(character, "invincible", duration) {

        if (!character.HasEffect("invincible")) {
            stars = GameObject.Instantiate(
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

    public override void Update(float deltaTime) {
        MusicManager.current.tempo = 1.25F;
    }

    public override void Destroy() {
        MusicManager.current.Remove(musicStackEntry);
    }
}