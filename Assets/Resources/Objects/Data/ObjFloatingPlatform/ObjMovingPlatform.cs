using UnityEngine;

public class ObjMovingPlatform : MonoBehaviour {
    CharacterGroundedDetector groundedDetector;

    void Awake() {
        groundedDetector = GetComponent<CharacterGroundedDetector>();
        positionPrev = transform.position;
    }

    Vector3 positionPrev;

    void Update() {
        foreach (Character character in groundedDetector.characters)
            character.position = character.position + (transform.position - positionPrev);

        positionPrev = transform.position;
    }
}