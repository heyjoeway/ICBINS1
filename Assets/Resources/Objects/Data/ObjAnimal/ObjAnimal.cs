using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjAnimal : MonoBehaviour {
    public enum AnimalType {
        Flicky,
        Seal,
        Chicken,
        Rabbit,
        Penguin,
        Squirrel,
        Pig
    }

    // ========================================================================

    public enum AnimalAnimationType {
        Fly,
        Jump
    }
    
    struct AnimalParams {
        public AnimalAnimationType animalAnimationType;
        public string animInitial;
        public string animJump;
        public string animFall; // Unused depending on animation type
        public float jumpGravity; // Gravity after first jump
        public float jumpStrength;
        public float moveSpeed;
    }

    static Dictionary<AnimalType, AnimalParams> AnimalData = new Dictionary<AnimalType, AnimalParams>() {
        [AnimalType.Flicky] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Fly,
            animInitial = "FlickyJump",
            animJump = "FlickyFly",
            jumpGravity = -0.09375F,
            jumpStrength = 4,
            moveSpeed = 4
        },
        [AnimalType.Seal] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Jump,
            animInitial = "SealJump",
            animJump = "SealRun1",
            animFall = "SealRun2",
            jumpGravity = -0.21875F,
            jumpStrength = 3,
            moveSpeed = 4
        },
        [AnimalType.Chicken] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Jump,
            animInitial = "ChickenJump",
            animJump = "ChickenRun1",
            animFall = "ChickenRun2",
            jumpGravity = -0.21875F,
            jumpStrength = 3,
            moveSpeed = 4
        },
        [AnimalType.Rabbit] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Jump,
            animInitial = "RabbitJump",
            animJump = "RabbitRun2",
            animFall = "RabbitRun1",
            jumpGravity = -0.21875F,
            jumpStrength = 4,
            moveSpeed = 4
        },
        [AnimalType.Penguin] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Jump,
            animInitial = "PenguinJump",
            animJump = "PenguinRun1",
            animFall = "PenguinRun2",
            jumpGravity = -0.21875F,
            jumpStrength = 3,
            moveSpeed = 4
        },
        [AnimalType.Squirrel] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Jump,
            animInitial = "SquirrelJump",
            animJump = "SquirrelRun1",
            animFall = "SquirrelRun2",
            jumpGravity = -0.21875F,
            jumpStrength = 3,
            moveSpeed = 4
        },
        [AnimalType.Pig] = new AnimalParams{
            animalAnimationType = AnimalAnimationType.Jump,
            animInitial = "PigJump",
            animJump = "PigRun1",
            animFall = "PigRun2",
            jumpGravity = -0.21875F,
            jumpStrength = 3,
            moveSpeed = 4
        }
    };

    // ========================================================================

    new Rigidbody rigidbody { get { return GetComponent<Rigidbody>(); }}
    Animator animator { get { return GetComponent<Animator>(); }}
    SpriteRenderer spriteRenderer { get { return GetComponent<SpriteRenderer>(); }}

    // ========================================================================

    float initialGravity = -0.21875F;
    float initialJumpStrength = 4;
    float gravity;

    // ========================================================================

    AnimalAnimationType animalAnimationType { get { 
        return AnimalData[animalType].animalAnimationType;
    }}
    
    string animInitial { get { 
        return AnimalData[animalType].animInitial;
    }}
    
    string animJump { get { 
        return AnimalData[animalType].animJump;
    }}
    
    string animFall { get { 
        return AnimalData[animalType].animFall;
    }}
    
    float jumpGravity { get { 
        return AnimalData[animalType].jumpGravity;
    }}
    
    float jumpStrength { get { 
        return AnimalData[animalType].jumpStrength;
    }}

    float moveSpeed { get { 
        return AnimalData[animalType].moveSpeed;
    }}

    // ========================================================================

    public AnimalType animalType;
    public bool stayInPlace = false;
    bool moveRight;
    bool hitGround = false;


    // Start is called before the first frame update
    void Start() {
        moveRight = Random.value >= 0.5F;
        gravity = initialGravity;
        rigidbody.velocity = new Vector3(0, initialJumpStrength * Utils.physicsScale, 0);
        animator.Play(animInitial);
    }

    // Update is called once per frame
    void Update() {
        rigidbody.velocity += new Vector3(0, gravity * Utils.physicsScale * Utils.deltaTimeScale);
        if (
            hitGround &&
            (animalAnimationType == AnimalAnimationType.Jump) &&
            (rigidbody.velocity.y < 0)
        ) animator.Play(animFall);
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger) return;
        hitGround = true;
        animator.Play(animJump);
        gravity = jumpGravity;
        if (stayInPlace) moveRight = !moveRight;
        spriteRenderer.flipX = !moveRight;
        rigidbody.velocity = new Vector3(
            (moveRight ? 1 : -1) * moveSpeed,
            jumpStrength * Utils.physicsScale,
            0
        );
    }
}
