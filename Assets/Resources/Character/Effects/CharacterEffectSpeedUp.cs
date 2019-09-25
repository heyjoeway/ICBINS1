using UnityEngine;
using System;

public class CharacterEffectSpeedUp : CharacterEffect {
    MusicManager musicManager;

    public CharacterEffectSpeedUp(Character character, float duration = 20F)
        : base(character, "speedUp", duration) {
        musicManager = Utils.GetMusicManager();
    }
    
    public override void Update(float deltaTime) {
        musicManager.tempo = 1.25F;
    }

    public override void Destroy() {
        musicManager.tempo = 1F;
    }
}