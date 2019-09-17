using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBuzzBomber : MonoBehaviour {
    Animator animator;
    new Rigidbody rigidbody;
    LevelManager levelManager;
    Transform bulletLocationTransform;

    void InitReferences() {
        animator = GetComponent<Animator>();
        levelManager = Utils.GetLevelManager();
        rigidbody = GetComponent<Rigidbody>();
        bulletLocationTransform = transform.Find("Bullet Location");
    }

    void Start() {
        InitReferences();
    }

    const float speed = 4F;
    const float moveTimerMax = 2.122F;
    const float pauseTimerTurnMax = 0.983F;
    const float pauseTimerFireMax = 1.5F;

    bool hasFired = false;
    float pauseTimer = 0;
    float moveTimer = moveTimerMax;

    public void FireBullet() {
        GameObject bullet = Instantiate(
            (GameObject)Resources.Load("Objects/Data/ObjBuzzBomber/Bullet"),
            bulletLocationTransform.position,
            Quaternion.identity
        );
        bullet.GetComponent<Rigidbody>().velocity = new Vector2(
            2F * -transform.localScale.x,
            -2F
        ) * Utils.physicsScale;
        bullet.transform.position = bulletLocationTransform.position;
    }

    // Update is called once per frame
    void Update() {
        rigidbody.velocity = Vector3.zero;
        if (pauseTimer > 0) {
            pauseTimer -= Utils.cappedDeltaTime;
            moveTimer = moveTimerMax;
            return;
        }
        pauseTimer = 0;

        animator.Play("Flying");

        if (moveTimer > 0) {
            moveTimer -= Utils.cappedDeltaTime;

            if (!hasFired) {
                // Check if character is near
                CharacterPackage characterInRange;
                characterInRange = Utils.CheckIfCharacterInRange(
                    transform.position,
                    3,
                    Utils.AxisType.X,
                    Utils.DistanceType.Character,
                    levelManager.characterPackages
                );

                if (characterInRange != null) {
                    animator.Play("Firing");
                    hasFired = true;
                    pauseTimer = pauseTimerFireMax;
                    return;
                }
            }
            rigidbody.velocity = new Vector2(
                -transform.localScale.x * speed,
                0
            ) * Utils.physicsScale;
            // transform.position += transform.right * -transform.localScale.x * speed / 30F;
            return;
        }

        hasFired = false;
        moveTimer = 0;
        Vector3 scaleTemp = transform.localScale;
        scaleTemp.x *= -1;
        transform.localScale = scaleTemp;
        pauseTimer = pauseTimerTurnMax;
        animator.Play("Still");
    }
}
