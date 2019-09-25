using UnityEngine;

public class ObjShield : MonoBehaviour {
    public Character character;
    public bool isInvincibility = false;

    SpriteRenderer spriteRenderer;

    MusicManager.MusicStackEntry musicStackEntry;

    void Start() {
        if (isInvincibility) {
            musicStackEntry = new MusicManager.MusicStackEntry {
                introPath = "Music/Invincibility Intro",
                loopPath = "Music/Invincibility Loop",
                priority = 1
            };
            Utils.GetMusicManager().Add(musicStackEntry);
        } else if (character != null) {
            character.shield = this;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (character == null) return;
        bool shouldDestroy = (
            (!isInvincibility && (character.shield != this)) ||
            (isInvincibility && character.HasEffect("invincible"))
        );
        if (shouldDestroy) {
            if (musicStackEntry != null)
                Utils.GetMusicManager().Remove(musicStackEntry);

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
        spriteRenderer.enabled = character.HasEffect("invincible");
    }
}