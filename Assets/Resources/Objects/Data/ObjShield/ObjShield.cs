using UnityEngine;

public class ObjShield : MonoBehaviour {
    public Character character;
    SpriteRenderer spriteRenderer;


    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (character == null) return;

        transform.localScale = character.transform.localScale;

        transform.position = new Vector3(
            character.position.x,
            character.position.y,
            character.position.z - 0.2F
        );
        if (!character.InStateGroup("rolling")) transform.position += new Vector3(
            0, 0.125F, 0
        );
        spriteRenderer.enabled = !character.HasEffect("invincible");
    }
}