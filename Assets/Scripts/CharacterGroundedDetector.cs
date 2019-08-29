using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGroundedDetector : MonoBehaviour {
    public HashSet<Character> characters = new HashSet<Character>();
    public void Enter(Character character) { characters.Add(character); }
    public void Leave(Character character) { characters.Remove(character); }
}