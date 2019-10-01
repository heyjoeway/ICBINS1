using UnityEngine;
using System;

public class CharacterEffectInvincible : CharacterEffect {
    MusicManager musicManager;
    MusicManager.MusicStackEntry musicStackEntry;

    public CharacterEffectInvincible(Character character, float duration = 20F)
        : base(character, "invincible", duration) {
        musicManager = Utils.GetMusicManager();

        if (!character.HasEffect("invincible")) {
            ObjShield stars = GameObject.Instantiate(Resources.Load<GameObject>(
                "Objects/Invincibility Stars"
            )).GetComponent<ObjShield>();
            stars.character = character;
        }

        musicStackEntry = new MusicManager.MusicStackEntry {
            introPath = "Music/Invincibility Intro",
            loopPath = "Music/Invincibility Loop",
            priority = 1
        };
        musicManager.Add(musicStackEntry);
    }

    public override void Destroy() {
        musicManager.Remove(musicStackEntry);
    }
}