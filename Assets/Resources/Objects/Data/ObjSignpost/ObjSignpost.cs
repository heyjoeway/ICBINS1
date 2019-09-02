using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjSignpost : MonoBehaviour {
    Animator animator;
    AudioSource audioSource;
    public SceneReference nextLevelRef;

    void InitReferences() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start() {
        InitReferences();
    }

    HashSet<Character> charactersTouched = new HashSet<Character>();

    void Touch(Character character) {
        if (charactersTouched.Contains(character)) return;
        charactersTouched.Add(character);
        animator.Play("Spin");
        audioSource.Play();
        ObjLevelClear levelClearObj = Instantiate(
            Resources.Load<GameObject>("Objects/Level Clear"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ObjLevelClear>();
        levelClearObj.character = character;
        levelClearObj.sceneReference = nextLevelRef;
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        Touch(character);
    }
}
