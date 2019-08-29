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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        if (charactersHit.Contains(character)) return;
        if (id > 0) {
            if (character.checkpointId >= id) return;
            character.checkpointId = id;

            foreach (ObjCheckpoint checkpoint in FindObjectsOfType<ObjCheckpoint>()) {
                if (checkpoint.id == 0) continue;
                if (checkpoint.id > id) continue;
                checkpoint.animator.Play("Active");
            }
        }

        charactersHit.Add(character);
        character.respawnPosition = respawnPosition;
        animator.Play("Hit");
        audioSource.time = 0;
        audioSource.Play();
    }
}
