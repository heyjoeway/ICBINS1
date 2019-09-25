using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjRing : MonoBehaviour {
    static GameObject ringGameObject;

    static bool _initStaticDone = false;
    static void _InitStatic() {
        if (_initStaticDone) return;

        ringGameObject = Resources.Load<GameObject>("Objects/Ring");

        _initStaticDone = true;
    }


    // Shamelessly taken line-for-line from https://info.sonicretro.org/SPG:Ring_Loss
    public static void ExplodeRings(Vector2 origin, int count) {
        _InitStatic();

        count = Mathf.Min(count, 256);
        float angle = 101.25F + 90F; // assuming 0=right, 90=up, 180=left, 270=down
        bool flipHSpeed = false;
        float speed = 0F;
        
        for (int t = 0; t < count; t++) {
            if (t % 16 == 0) {
                switch (speed) {
                    case 0F:
                        speed = 4F;
                        break;
                    case 4F:
                        speed = 2F;
                        break;
                    case 2F:
                        speed = 6F;
                        break;
                    default:
                        speed += 2F;
                        break;
                }
                angle = 101.25F + 90F; // and reset the angle
            }

            // create a bouncing ring object
            ObjRing ring = Instantiate(
                ringGameObject,
                origin,
                Quaternion.identity
            ).GetComponent<ObjRing>();
            ring.InitReferences();
            ring.falling = true;

            // set the ring's vertical speed to sine(angle)*speed
            // set the ring's horizontal speed to -cosine(angle)*speed
            ring.initialVelocity = new Vector2(
                Mathf.Sin(angle * Mathf.Deg2Rad) * speed,
                -Mathf.Cos(angle * Mathf.Deg2Rad) * speed
            ) * Utils.physicsScale;

            if (flipHSpeed) {
                // multiply the ring's horizontal speed by -1
                ring.initialVelocity.x *= -1;
                // increase angle by 22.5
                angle += 22.5F;
            }
            flipHSpeed = !flipHSpeed; // if n is false, n becomes true and vice versa
        }
    }


    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================
    Animator animator;
    new Rigidbody rigidbody;
    AudioSource audioSource;
    SpriteRenderer spriteRenderer;

    bool _initReferencesDone = false;
    void InitReferences() {
        if (_initReferencesDone) return;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        _initReferencesDone = true;
    }

    public Vector3 initialVelocity = Vector3.zero;

    void Start() {
        StaticInit();
        InitReferences();

    }

    // ========================================================================

    const float gravity = -0.09375F;
    static Sprite[] sprites;
    static int[] spriteIdSpin = {
        5, 6, 7, 8
    };
    static float spinFrameTime = 8F / 60F;

    // ========================================================================

    static bool _staticInitDone = false;
    void StaticInit() {
        if (_staticInitDone) return;
        sprites = Resources.LoadAll<Sprite>("Objects/Data/ObjRing/Rings");
        _staticInitDone = true;
    }

    // ========================================================================

    static int panStereo = 1;
    public bool falling = false;
    float fallingTimerMax = 4.27F;
    float fallingTimer = 4.27F;
    bool collected = false;

    void OnTriggerEnter(Collider other) {
        if (collected) return;
        Character[] characters = other.gameObject.GetComponentsInParent<Character>();
        if (characters.Length == 0) {
            if (!falling) return;
            if (other.isTrigger) return;

            Vector3 velocity = rigidbody.velocity;
            velocity.y = Mathf.Max(1F * Utils.physicsScale, Mathf.Abs(velocity.y) * 0.75F);
            rigidbody.velocity = velocity;
        } else {
            Character character = characters[0];
            if (character.InStateGroup("ignore")) return;
            // if (character.invulnTimer > 1.5F) return;

            character.rings++;
            panStereo = -panStereo;
            audioSource.panStereo = panStereo;
            audioSource.Play();
            animator.enabled = true;
            animator.Play("Sparkle");
            collected = true;
            rigidbody.isKinematic = true;
        }
    }

    void OnAnimationDone() {
        Destroy(gameObject);
    }

    void Update() {
        if (collected) return;

        if (initialVelocity != Vector3.zero) { // Hack
            rigidbody.velocity = initialVelocity;
            initialVelocity = Vector3.zero;
        }

        rigidbody.isKinematic = !falling;
        if (falling) {
            animator.enabled = true;
            animator.Play("Spin");
            rigidbody.velocity += new Vector3(0, gravity * Utils.physicsScale, 0);

            fallingTimer -= Utils.cappedDeltaTime;
            animator.speed = (fallingTimer / fallingTimerMax) * 4;

            if (fallingTimer <= 0)
                Destroy(gameObject);
        } else {
            // Make all rings spin at the same speed/frame
            animator.enabled = false;
            float timeConstrained = Time.time % (spinFrameTime * spriteIdSpin.Length);
            int frameIndex = (int)Mathf.Floor(timeConstrained / spinFrameTime);
            int frame = spriteIdSpin[frameIndex];
            Sprite sprite = sprites[frame];
            spriteRenderer.sprite = sprite;
        }
    }
}
