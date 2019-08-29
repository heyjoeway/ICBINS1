using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBreakablePlatforms : MonoBehaviour {
    SpriteRenderer spriteRenderer;
    CharacterGroundedDetector groundedDetector;
    AudioSource audioSource;
    Transform collisionTransform;

    void InitReferences() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collisionTransform = transform.Find("Collision");
        groundedDetector = collisionTransform.GetComponent<CharacterGroundedDetector>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    float destroyDelay = 2F;
    float timerMax = 0.55F;
    float timer = 0;
    List<GameObject> blocks = new List<GameObject>();
    bool collapsing = false;

    void Start() {
        InitReferences();
    }

    void BeginCollapse() {
        Texture2D texture = spriteRenderer.sprite.texture;
        for (int x = 0; x < texture.width; x += 16) {
            for (int y = 0; y < texture.height; y += 16) {
                Sprite blockSprite = Sprite.Create(
                    texture,
                    new Rect(x, y, 16, 16),
                    Vector2.zero, // pivot
                    32 // pixels per unit
                );

                GameObject block = Instantiate(
                    Resources.Load<GameObject>("Objects/Data/ObjBreakablePlatform/Breakable Platform Block"),
                    transform.position + new Vector3(
                        x * transform.lossyScale.x / 32F,
                        y * transform.lossyScale.y / 32F,
                        0
                    ),
                    Quaternion.identity
                );
                block.transform.localScale = transform.lossyScale;
                block.GetComponent<SpriteRenderer>().sprite = blockSprite;

                blocks.Add(block);
            }
        }

        spriteRenderer.enabled = false;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update() {
        if (!collapsing && groundedDetector.characters.Count == 0) return;
        if (!collapsing) BeginCollapse();
        collapsing = true;

        if (timer <= timerMax) {
            for (int i = 0; i < blocks.Count * (timer / timerMax); i++) {
                GameObject block = blocks[i];
                if (block == null) continue;

                Rigidbody blockRigidbody = blocks[i].GetComponent<Rigidbody>();

                // Set gravity to false first otherwise unity bugs out
                blockRigidbody.useGravity = false;
                blockRigidbody.useGravity = true;
            }
        } else if (timer <= timerMax + destroyDelay) {
            collisionTransform.gameObject.SetActive(false);
        } else {
            Destroy(gameObject);
        }

        timer += Time.deltaTime;

    }
}
