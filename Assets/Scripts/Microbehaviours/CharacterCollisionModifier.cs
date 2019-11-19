using UnityEngine;

public class CharacterCollisionModifier : MonoBehaviour { 
    public enum CollisionModifierType {
        None,
        NoGrounding,
        NoGroundingLRB,
        NoGroundingLRBHigher
    }
    public CollisionModifierType type;
}