using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjSignpost : MonoBehaviour {
    Animator animator;
    AudioSource audioSource;
    public SceneReference nextLevelRef;
    public UnityEvent onNextLevel;

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
            Constants.Get<GameObject>("prefabLevelClear"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ObjLevelClear>();
        levelClearObj.character = character;
        levelClearObj.sceneReference = nextLevelRef;
        levelClearObj.onNextLevel = onNextLevel;

        character.timerPause = true;
        
        if (character.characterCamera != null) {
            character.characterCamera.LockHorizontal(transform.position.x);
            character.characterCamera.SetCharacterBoundsFromCamera();
        }
    }

    void OnTriggerEnter(Collider other) {
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];
        Touch(character);
    }
}
