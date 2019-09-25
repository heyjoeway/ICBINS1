using UnityEngine;

public class CharacterEffect {
    public string name;
    public float duration;

    public CharacterEffect(string name, float duration = Mathf.Infinity) {
        this.name = name;
        this.duration = duration;
    }

    public void Update(float deltaTime) {
        this.duration -= deltaTime;
    }

    public override string ToString() => name;

    
}