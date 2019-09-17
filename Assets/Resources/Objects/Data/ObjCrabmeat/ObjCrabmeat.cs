using UnityEngine;

public class ObjCrabmeat : MonoBehaviour {
    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================

    Transform bulletPositionL;
    Transform bulletPositionR;

    SpriteRenderer spriteRenderer;
    Animator animator;
    LevelManager levelManager;

    void InitReferences() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        bulletPositionL = transform.Find("Bullet Position L");
        bulletPositionR = transform.Find("Bullet Position R");

        levelManager = Utils.GetLevelManager();
    }

void Start() { InitReferences(); }

    // ========================================================================

    const float speed = 0.015625F;
    const float triggerDistance = 4F;

    // ========================================================================

    public bool moveRight = false;
    float turnTimer = 0;
    float walkTimer = 2.12F;
    bool hasFired = false;
    bool hasTriedToFireEver = false;
    Vector3 positionPrev = Vector3.zero;

    // ========================================================================

    int direction { get { return moveRight ? 1 : -1; }}

    // ========================================================================

    void Fire() {
        Instantiate(
            (GameObject)Resources.Load("Objects/Data/ObjCrabmeat/BulletL"),
            bulletPositionL.position,
            Quaternion.identity
        );

        Instantiate(
            (GameObject)Resources.Load("Objects/Data/ObjCrabmeat/BulletR"),
            bulletPositionR.position,
            Quaternion.identity
        );

        animator.Play("Fire");
        hasFired = true;
        turnTimer = 1F;
    }

    void Update() {
        if (turnTimer > 0) {
            turnTimer -= Utils.cappedDeltaTime;
            if (turnTimer > 0) return;

            if (!hasFired) {
                if (hasTriedToFireEver) {
                    Fire();
                    return;
                }

                hasTriedToFireEver = true;
            }

            turnTimer = 0;
            moveRight = !moveRight;
            transform.position = positionPrev;
            animator.Play("Walk");
            walkTimer = 2.17F;
            return;
        }

        RaycastHit hitLeft;
        Physics.Raycast(
            transform.position + (Vector3.left * 0.5F), // origin
            Vector3.down, // direction,
            out hitLeft,
            transform.localScale.y, // max distance
            ~Utils.IgnoreRaycastMask
        );

        RaycastHit hitRight;
        Physics.Raycast(
            transform.position + (Vector3.right * 0.5F), // origin
            Vector3.down, // direction,
            out hitRight,
            transform.localScale.y, // max distance
            ~Utils.IgnoreRaycastMask
        );
        
        Vector3 newPos = transform.position;

        walkTimer -= Utils.cappedDeltaTime;

        if ((walkTimer < 0) || (hitLeft.collider == null) || (hitRight.collider == null)) {
            turnTimer = 1F;
            animator.Play("Stand");
            hasFired = false;
            return;
        }

        newPos.x += direction * speed * Utils.deltaTimeScale;
        newPos.y = ((hitLeft.point.y + hitRight.point.y) / 2) + (transform.localScale.y / 2F);
        positionPrev = transform.position;
        transform.position = newPos;
    }
}
