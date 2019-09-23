using UnityEngine;

public class CharacterEffect {
    string effectName;
    float duration;

    public CharacterEffect(string effectName, float duration = Mathf.Infinity) {
        this.effectName = effectName;
        this.duration = Mathf.Infinity;
    }
}