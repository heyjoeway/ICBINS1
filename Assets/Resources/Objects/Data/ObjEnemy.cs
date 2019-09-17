using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjEnemy : MonoBehaviour {
    public ObjAnimal.AnimalType[] animalTypes;
    bool _destroyed = false;

    void OnTriggerStay(Collider other) {
        OnTriggerEnter(other);
    }

    void OnTriggerEnter(Collider other) {
        if (_destroyed) return; // Prevents trigger from being fired multiple times
        if (!enabled) return;

        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        if (character.isHarmful) Explode(character);
        else character.Hurt(character.position.x <= transform.position.x);
    }

    static int[] pointsTable = new int[] {
        100,
        200,
        500,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        1000,
        10000
    };

    void Explode(Character sourceCharacter) {
        sourceCharacter.BounceEnemy(gameObject);

        int points = pointsTable[Mathf.Min(
            sourceCharacter.destroyEnemyChain,
            pointsTable.Length - 1
        )];

        GameObject pointsObj = Instantiate(
            (GameObject)Resources.Load("Objects/Points"),
            transform.position,
            Quaternion.identity
        );
        pointsObj.transform.Find("Text").GetComponent<Text>().text = points.ToString();

        sourceCharacter.score += points;

        sourceCharacter.destroyEnemyChain++;

        Instantiate(
            (GameObject)Resources.Load("Objects/Explosion (Enemy)"),
            transform.position,
            Quaternion.identity
        );
        
        if (animalTypes.Length > 0) {
            GameObject animalObj = Instantiate(
                (GameObject)Resources.Load("Objects/Animal"),
                transform.position,
                Quaternion.identity
            );
            ObjAnimal.AnimalType animalType = (
                animalTypes[Random.Range(0, animalTypes.Length)]
            );
            animalObj.GetComponent<ObjAnimal>().animalType = animalType;
        }

        _destroyed = true;
        Destroy(gameObject);
    }

    void Update() { }
}
