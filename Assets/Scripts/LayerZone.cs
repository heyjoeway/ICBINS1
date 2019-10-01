using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerZone : MonoBehaviour {
    public float zLeft = 0;
    public float zRight = 0;
    public bool groundedOnly = false;

    new Renderer renderer;
    LevelManager levelManager;

    void Start() {
        renderer = GetComponent<Renderer>();
        levelManager = Utils.GetLevelManager();
    }

    void Update() {
        foreach (CharacterPackage characterPackage in levelManager.characterPackages) {
            Character character = characterPackage.character;
            if (!renderer.bounds.Intersects(character.colliderCurrent.bounds)) continue;
            
            Vector3 characterPos = character.position;

            if (groundedOnly && !character.InStateGroup("ground"))
                return;

            if (characterPos.x > transform.position.x) characterPos.z = zRight;
            else characterPos.z = zLeft;

            character.position = characterPos;
        }
    }
}
