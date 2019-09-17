using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBreakableWall : MonoBehaviour {
    new Collider collider;
    SpriteRenderer spriteRenderer;
    AudioSource audioSource;

    void InitReferences() {
        collider = GetComponent<Collider>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponentInParent<AudioSource>();
    }
    void Start() { InitReferences(); }

    // Update is called once per frame
    void Update() {
        if (!hit) return;
        destroyTimer -= Utils.cappedDeltaTime;
        if (destroyTimer <= 0) Destroy(gameObject);
    }

    bool hit = false;
    float destroyTimer = 3F;

    void BeginBreak(int direction) {
        Sprite sprite = spriteRenderer.sprite;
        Texture2D texture = sprite.texture;
        Vector2 spriteSmashCenter = new Vector2(
            direction > 0 ? (int)sprite.rect.x : (int)sprite.rect.x + (int)sprite.rect.width,
            sprite.rect.y + (sprite.rect.height / 2F)
        );

        for (int x = (int)sprite.rect.x; x < (int)sprite.rect.x + (int)sprite.rect.width; x += 16) {
            for (int y = (int)sprite.rect.y; y < (int)sprite.rect.y + (int)sprite.rect.height; y += 16) {
                Sprite blockSprite = Sprite.Create(
                    texture,
                    new Rect(x, y, 16, 16),
                    Vector2.zero, // pivot
                    32 // pixels per unit
                );

                GameObject block = Instantiate(
                    Resources.Load<GameObject>("Objects/Data/ObjBreakablePlatform/Breakable Platform Block"),
                    transform.position + new Vector3(
                        (x - (int)sprite.rect.x) * transform.lossyScale.x / 32F,
                        (y - (int)sprite.rect.y) * transform.lossyScale.y / 32F,
                        0
                    ),
                    Quaternion.identity
                );
                block.transform.localScale = transform.lossyScale;
                block.GetComponent<SpriteRenderer>().sprite = blockSprite;
                Rigidbody rigidbody = block.GetComponent<Rigidbody>();
                rigidbody.useGravity = true;
                
                rigidbody.velocity = new Vector2(
                    (
                        2F + // base speed
                        (2F * Mathf.Abs(spriteSmashCenter.x - x + 8) / sprite.rect.width) + // speed from x position
                        (2F * (1 - (Mathf.Abs(y - spriteSmashCenter.y + 8) / sprite.rect.height))) // speed from y position
                    ) * direction,
                    (10F * (y - spriteSmashCenter.y + 8) / sprite.rect.height) // speed from y position
                ) * Utils.physicsScale;
            }
        }

        spriteRenderer.enabled = false;
        collider.enabled = false;
        hit = true;
        if (audioSource != null) audioSource.Play();
    }

    void OnCollisionEnter(Collision collision) {
        if (hit) return;

        GameObject other = collision.gameObject;
        Character[] characters = other.GetComponentsInParent<Character>();
        if (characters.Length == 0) return;
        Character character = characters[0];

        if (!character.inRollingState || !character.inGroundedState) return;
        if (Mathf.Abs(character.groundSpeedPrev) < 4.5F) return;
        character.groundSpeed = character.groundSpeedPrev;

        BeginBreak(character.position.x < transform.position.x ? -1 : 1);
    }
}
