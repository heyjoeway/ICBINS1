using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjCheckpoint : MonoBehaviour {
    List<Character> charactersHit;
    Vector3 respawnPosition => transform.Find("Respawn Location").position;

    Animator animator => GetComponent<Animator>();

    AudioSource audioSource => GetComponent<AudioSource>();

    public int id = 0;

    // Start is called before the first frame update
    void Start() {
        charactersHit = new List<Character>();
        RefreshAll();
    }

    void RefreshAll() {
        int maxId = 0;
        foreach (Character character in LevelManager.current.characters) {
            if (character.currentLevel == null) return;
            if (character.currentLevel.gameObject.scene != gameObject.scene) return;
            maxId = Mathf.Max(character.checkpointId, maxId);
        }

        foreach (ObjCheckpoint checkpoint in FindObjectsOfType<ObjCheckpoint>()) {
            if (gameObject.scene != checkpoint.gameObject.scene) continue;
            if (checkpoint.id == 0) continue;
            if (checkpoint.id > maxId) continue;
            checkpoint.animator.Play("Active");
        }
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        if (charactersHit.Contains(character)) return;
        if (id > 0) {
            if (character.checkpointId >= id) return;
            character.checkpointId = id;
            RefreshAll();
        }

        charactersHit.Add(character);
        character.respawnData.position = respawnPosition;
        character.respawnData.timer = character.timer;
        animator.Play("Hit");
        audioSource.time = 0;
        audioSource.Play();
    }
}
