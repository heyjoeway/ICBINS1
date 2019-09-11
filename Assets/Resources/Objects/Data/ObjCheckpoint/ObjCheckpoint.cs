using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjCheckpoint : MonoBehaviour {
    List<Character> charactersHit;
    Vector3 respawnPosition { get {
        return transform.Find("Respawn Location").position;
    }}

    Animator animator { get { return GetComponent<Animator>(); }}

    AudioSource audioSource { get { return GetComponent<AudioSource>(); }}

    public int id = 0;

    // Start is called before the first frame update
    void Start() {
        charactersHit = new List<Character>();
        RefreshAll();
    }

    void RefreshAll() {
        foreach (ObjCheckpoint checkpoint in FindObjectsOfType<ObjCheckpoint>()) {
            if (gameObject.scene != checkpoint.gameObject.scene) continue;
            if (checkpoint.id == 0) continue;
            if (checkpoint.id > id) continue;
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
        animator.Play("Hit");
        audioSource.time = 0;
        audioSource.Play();
    }
}
