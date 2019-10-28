using UnityEngine;
using System;

public class CharacterEffectSpeedUp : CharacterEffect {
    public CharacterEffectSpeedUp(Character character, float duration = 20F)
        : base(character, "speedUp", duration) { }
    
    public override void Update(float deltaTime) {
        MusicManager.current.tempo = 1.25F;
    }

    public override void Destroy() {
        MusicManager.current.tempo = 1F;
    }
}