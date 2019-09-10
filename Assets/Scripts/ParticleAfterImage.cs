using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAfterImage : MonoBehaviour {
    new ParticleSystem particleSystem;
    ParticleSystemRenderer particleSystemRenderer;
    SpriteRenderer spriteRenderer;
    public bool enableAfterImage = false;

    void Start() {
        particleSystem = GetComponent<ParticleSystem>();
        particleSystemRenderer = GetComponent<ParticleSystemRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update() {
        if (enableAfterImage) {
            particleSystem.Play();
            particleSystem.textureSheetAnimation.SetSprite(0, spriteRenderer.sprite);
            particleSystemRenderer.flip = new Vector3(
                (spriteRenderer.flipX ^ (transform.lossyScale.x < 0)) ? 1 : 0,
                (spriteRenderer.flipY ^ (transform.lossyScale.y < 0)) ? 1 : 0,
                0
            );
        } else particleSystem.Pause();
    }
}
