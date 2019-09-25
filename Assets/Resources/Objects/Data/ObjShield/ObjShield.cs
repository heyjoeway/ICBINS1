using UnityEngine;

public class ObjShield : MonoBehaviour {
    public Character character;
    public bool isInvincibility = false;

    SpriteRenderer spriteRenderer;


    void Start() {
        if (isInvincibility) {

        } else if (character != null) {
            character.shield = this;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (character == null) return;
        bool shouldDestroy = (
            (!isInvincibility && (character.shield != this)) ||
            (isInvincibility && !character.HasEffect("invincible"))
        );
        if (shouldDestroy) {
            character = null;
            Destroy(gameObject);
            return;
        }
        transform.position = new Vector3(
            character.position.x,
            character.position.y,
            transform.position.z
        );
        if (!character.InStateGroup("rolling")) transform.position += new Vector3(
            0, 0.125F, 0
        );
        spriteRenderer.enabled = !character.HasEffect("invincible");
    }
}