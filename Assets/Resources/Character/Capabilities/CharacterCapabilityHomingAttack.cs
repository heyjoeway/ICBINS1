using UnityEngine;

public class CharacterCapabilityHomingAttack : CharacterCapability {
    float homingAttackSpeed { get {
        return 9F * character.physicsScale;
    }}

    float homingAttackBounceSpeed { get {
        return 6.5F * character.physicsScale;
    }}

    public CharacterCapabilityHomingAttack(Character character) : base(character) { }

    Transform target;
    CharacterEffect afterImageEffect;

    public override void Init() {
        name = "homingAttack";
        character.AddStateGroup("rolling", "homingAttack");
        character.AddStateGroup("airCollision", "homingAttack");
        character.AddStateGroup("harmful", "homingAttack");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != "homingAttack") return;
        
        SFX.PlayOneShot(character.audioSource, "SFX/Megamix/Thok");
        
        target = FindClosestTarget();

        if (target == null) {
            character.velocity = new Vector2(
                homingAttackSpeed * (character.facingRight ? 1 : -1),
                0
            );
            character.stateCurrent = "rollingAir";
            character.effects.Add(new CharacterEffect(character, "afterImage", 0.25F));
        } else {
            character.velocity = Vector3.zero;
            character.modeGroupCurrent = character.rollingAirModeGroup;
            afterImageEffect = new CharacterEffect(character, "afterImage");
            character.effects.Add(afterImageEffect);
        }
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != "homingAttack") return;
        if (afterImageEffect != null)
            afterImageEffect.DestroyBase();
    }

    public override void Update(float deltaTime) {
        if (character.stateCurrent == "jump") {
            if (character.input.GetButtonsDownPreventRepeat("Secondary", "Tertiary"))
                character.stateCurrent = "homingAttack";
        }

        if (character.stateCurrent != "homingAttack") return;

        if (target == null) {
            character.stateCurrent = "rollingAir";
        } else {
            character.position = Vector3.MoveTowards(
                character.position,
                target.position,
                homingAttackSpeed * deltaTime * 2
            );

        }
    }

    public override void OnCollisionEnter(Collision collision) {
        if (character.stateCurrent != "homingAttack") return;
        if (collision.collider.isTrigger) return;
        character.stateCurrent = "rollingAir";        
    }

    public override void OnTriggerEnter(Collider other) {
        if (character.stateCurrent != "homingAttack") return;
        HomingAttackTarget[] targets = other.gameObject.GetComponentsInParent<HomingAttackTarget>();
        if (targets.Length == 0) return;

        character.velocity = new Vector2(
            0,
            homingAttackBounceSpeed
        );
        character.stateCurrent = "jump";
    }

    // https://forum.unity.com/threads/clean-est-way-to-find-nearest-object-of-many-c.44315/
    Transform FindClosestTarget(float distanceLimit = 24F) {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach(HomingAttackTarget target in GameObject.FindObjectsOfType<HomingAttackTarget>()) {
            Transform potentialTarget = target.transform;
            if (!potentialTarget.gameObject.activeSelf) continue;
            if (
                (potentialTarget.position.x <= character.position.x) &&
                character.facingRight
            ) continue;
            if (
                (potentialTarget.position.x >= character.position.x) &&
                !character.facingRight
            ) continue;
            if (!target.enabled) continue;

            Vector3 directionToTarget = potentialTarget.position - character.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if(dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }

        if (closestDistanceSqr > distanceLimit) return null;
        return bestTarget;
    }
}