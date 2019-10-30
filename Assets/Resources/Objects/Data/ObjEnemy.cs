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

    void CharacterBounce(Character character) {
        if (character.InStateGroup("ground")) return;

        bool shouldntRebound = (
            (character.position.y < transform.position.y) ||
            (character.velocity.y > 0)
        );

        Vector3 velocityTemp = character.velocity;
        if (shouldntRebound) {
            velocityTemp.y -= Mathf.Sign(velocityTemp.y) * character.physicsScale;
        } else {
            velocityTemp.y *= -1;
        }
        character.velocity = velocityTemp;
    }

    void Explode(Character sourceCharacter) {
        CharacterBounce(sourceCharacter);

        int points = pointsTable[Mathf.Min(
            sourceCharacter.destroyEnemyChain,
            pointsTable.Length - 1
        )];

        GameObject pointsObj = Instantiate(
            Constants.Get<GameObject>("prefabPoints"),
            transform.position,
            Quaternion.identity
        );
        pointsObj.transform.Find("Text").GetComponent<Text>().text = points.ToString();

        sourceCharacter.score += points;

        sourceCharacter.destroyEnemyChain++;

        Instantiate(
            Constants.Get<GameObject>("prefabExplosionEnemy"),
            transform.position,
            Quaternion.identity
        );
        
        if (animalTypes.Length > 0) {
            GameObject animalObj = Instantiate(
                Constants.Get<GameObject>("prefabAnimal"),
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
