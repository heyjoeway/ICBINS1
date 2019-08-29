using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPackage : MonoBehaviour {
    LevelManager levelManager;

    Character _character;
    public Character character { get {
        if (_character != null) return _character;
        _character = transform.Find("Character").GetComponent<Character>();
        return _character; 
    }}

    CharacterCamera _camera;
    new public CharacterCamera camera { get {
        if (_camera != null) return _camera;
        _camera = transform.Find("Camera").GetComponent<CharacterCamera>();
        return _camera; 
    }}

    void Start() {
        levelManager = Utils.GetLevelManager();
        levelManager.characterPackages.Add(this);
    }

    void OnDestroy() {
        levelManager.characterPackages.Remove(this);        
    }
}
