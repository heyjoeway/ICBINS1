using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjNewtronGreen : MonoBehaviour {
    Transform bulletPosition;
    new Collider collider;
    Animator animator;
    LevelManager levelManager;
    SpriteRenderer spriteRenderer;

    bool _initReferencesDone = false;
    void InitReferences() {
        if (_initReferencesDone) return;
        bulletPosition = transform.Find("Bullet Position");
        collider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        levelManager = Utils.GetLevelManager();
        _initReferencesDone = true;
    }

    const float triggerDistance = 4F;
    const float bulletSpeed = 3.75F;

    // Start is called before the first frame update
    void Start() {
        InitReferences();
    }

    bool firing = false;

    void OnEnable() {
        InitReferences();

        collider.enabled = false;
        animator.Play("Blank");
        firing = false;
    }

    void Fire() {
        GameObject bullet = Instantiate(
            (GameObject)Resources.Load("Objects/Data/ObjNewtron/Bullet"),
            bulletPosition.position,
            Quaternion.identity
        );
        bullet.GetComponent<Rigidbody>().velocity = new Vector3(
            bulletSpeed * -transform.localScale.x,
            0, 0
        );
    }

    // Update is called once per frame
    void Update() {
        if (firing) return;

        CharacterPackage characterInRange = Utils.CheckIfCharacterInRange(
            transform.position,
            triggerDistance,
            Utils.AxisType.X,
            Utils.DistanceType.Camera,
            levelManager.characterPackages
        );

        if (characterInRange == null) return;

        collider.enabled = true;
        animator.Play("Newtron");
        transform.localScale = new Vector3(
            characterInRange.character.position.x < transform.position.x ? 1 : -1,
            1, 1
        );

        firing = true;
    }
}
