using UnityEngine;

public class CharacterCollisionModifier : MonoBehaviour { 
    public enum CollisionModifierType {
        None,
        NoGrounding
    }
    public CollisionModifierType type;
}