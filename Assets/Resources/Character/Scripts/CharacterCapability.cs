using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterCapability : MonoBehaviour {
    [HideInInspector]
    public string name;
    [HideInInspector]
    public Character character;
    [HideInInspector]
    public Transform transform;

    public void Start() {
        character = GetComponent<Character>();
        character.capabilities.Add(this);
        transform = character.transform;
        Init();
        StateInit(character.stateCurrent, "");
    }

    public virtual void Init() { }
    public virtual void StateInit(string stateName, string prevStateName) { }
    public virtual void StateDeinit(string stateName, string nextStateName) { }
    public virtual void CharUpdate(float deltaTime) { }
    public virtual void OnCharCollisionEnter(Collision collision) { }
    public virtual void OnCharCollisionStay(Collision collision) { }
    public virtual void OnCharCollisionExit(Collision collision) { }
    public virtual void OnCharTriggerEnter(Collider other) { }
    public virtual void OnCharTriggerStay(Collider other) { }
    public virtual void OnCharTriggerExit(Collider other) { }
}