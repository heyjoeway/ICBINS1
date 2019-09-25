using UnityEngine;

public class CharacterCapabilityAir : CharacterCapability {
    const float accelerationAirNormal = 0.09375F;
    const float accelerationAirSpeedUp = 0.1875F;
    float accelerationAir { get {
        return (character.HasEffect("speedUp") ?
            accelerationAirSpeedUp :
            accelerationAirNormal
        ) * character.physicsScale;
    }}

    float airDragThreshold { get {
        return 4F * character.physicsScale;
    }}

    float gravity { get {
        return -0.21875F * character.physicsScale;
    }}
    
    // ========================================================================

    public CharacterCapabilityAir(Character character) : base(character) { }

    public override void Init() {
        name = "air";
        character.AddStateGroup("air", "air");
        character.AddStateGroup("airCollision", "air");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (!character.InStateGroup("airCollision")) return;
        UpdateAirTopSoild();
        character.groundedDetectorCurrent = null;
        if (character.InStateGroup("rolling")) return;
        character.modeGroupCurrent = character.airModeGroup;
    }

    public override void Update(float deltaTime) {
        if (!character.InStateGroup("airCollision")) return;
        UpdateAirTopSoild();
        if (!character.InStateGroup("air")) return;
        UpdateAirMove(deltaTime);
        UpdateAirRotation(deltaTime);
        UpdateAirGravity(deltaTime);
        UpdateAirAnim(deltaTime);
        character.GroundSpeedSync();
    }

    // 3D-Ready: YES
    void UpdateAirTopSoild() {
        if (character.velocity.y > 0) {
            character.rollingAirModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Rolling and Ignore Top Solid");
            character.airModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Ignore Top Solid");
        } else {
            character.rollingAirModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Rolling");
            character.airModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Default");
        }
    }

    // See: https://info.sonicretro.org/SPG:Jumping
    // 3D-Ready: NO
    void UpdateAirMove(float deltaTime) {
        Vector3 velocityTemp = character.velocity;
        float accelerationMagnitude = 0;

        // Acceleration
        if (Input.GetKey(KeyCode.LeftArrow) && !character.controlLock) {
            if (velocityTemp.x > -character.topSpeed) {
                accelerationMagnitude = -accelerationAir;
            }
        } else if (Input.GetKey(KeyCode.RightArrow) && !character.controlLock) {
            if (velocityTemp.x < character.topSpeed) {
                accelerationMagnitude = accelerationAir;
            }
        }

        Vector3 acceleration = new Vector3(
            accelerationMagnitude,
            0,
            0
        ) * deltaTime * 60F;
        velocityTemp += acceleration;

        // Air Drag
        if ((velocityTemp.y > 0 ) && (velocityTemp.y < airDragThreshold))
            velocityTemp.x -= (
                ((int)(velocityTemp.x / 0.125F)) / 256F
            ) * (deltaTime * 60F);

        character.velocity = velocityTemp;
    }

    // 3D-Ready: YES
    void UpdateAirRotation(float deltaTime) {
        transform.eulerAngles = Vector3.RotateTowards(
            transform.eulerAngles, // current
            character.forwardAngle <= 180 ? Vector3.zero : new Vector3(0, 0, 360), // target // TODO: 3D
            0.5F * deltaTime * 60F, // max rotation
            2F * deltaTime * 60F // magnitude
        );
    }
    
    // 3D-Ready: Yes
    void UpdateAirGravity(float deltaTime) {
        character.velocity += Vector3.up * gravity * deltaTime * 60F;
    }

    // Handle air collisions
    // Called directly from rigidbody component
    const float angleDistFlat = 20.5F;
    const float angleDistSteep = 45F;
    const float angleDistWall = 90F;

    public override void OnCollisionStay(Collision collision) {
        OnCollisionEnter(collision);
    }

    // Ready for hell?
    // Have fun
    // 3D-Ready: ABSO-FUCKING-LUTELY NOT
    public override void OnCollisionEnter(Collision collision) {
        if (!character.InStateGroup("airCollision")) return;

        CharacterCollisionModifier collisionModifier = collision.transform.GetComponentInParent<CharacterCollisionModifier>();
        if (collisionModifier != null) {
            switch (collisionModifier.type) {
                case CharacterCollisionModifier.CollisionModifierType.NoGrounding:
                    return;
                default:
                    break;
            }
        }

        // Set ground speed or ignore collision based on angle
        // See: https://info.sonicretro.org/SPG:Solid_Tiles#Reacquisition_Of_The_Ground

        // Wait a minute, why are we doing a raycast to get a normal/position that we already know??
        // BECAUSE, the normal/position from the collision is glitchy as fuck.
        // This helps smooth things out.
        RaycastHit potentialHit = character.GetSolidRaycast(
            collision.GetContact(0).point - transform.position
        );
        if (potentialHit.collider == null) return;
        RaycastHit hit = (RaycastHit)potentialHit;

        Vector3 hitEuler = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
        // Round this or any tiiiny deviation in angle can allow the character
        // to jump at walls and stick to them
        float hitAngle = Mathf.Round(hitEuler.z); // TODO: 3D

        // This looks like a mess, but honestly this is about as simple as it can be.
        // This is pretty much implemented 1:1 from the SPG, so read that for an explanation
        // See: https://info.sonicretro.org/SPG:Solid_Tiles#Reacquisition_Of_The_Ground
        if (character.velocityPrev.y <= 0) {
            if ((hitAngle <= angleDistFlat) || (hitAngle >= 360 - angleDistFlat)) {
                character.groundSpeed = character.velocityPrev.x;
            } else if ((hitAngle <= angleDistSteep) || (hitAngle >= 360 - angleDistSteep)) {

                if (Mathf.Abs(character.velocityPrev.x) > Mathf.Abs(character.velocityPrev.y))
                    character.groundSpeed = character.velocityPrev.x;
                else
                    character.groundSpeed = character.velocityPrev.y * Mathf.Sign(Mathf.Sin(Mathf.Deg2Rad * hitAngle)) * 0.5F;
            } else if ((hitAngle < angleDistWall) || (hitAngle > 360F - angleDistWall)) {

                if (Mathf.Abs(character.velocityPrev.x) > Mathf.Abs(character.velocityPrev.y))
                    character.groundSpeed = character.velocityPrev.x;
                else
                    character.groundSpeed = character.velocityPrev.y * Mathf.Sign(Mathf.Sin(Mathf.Deg2Rad * hitAngle));
            } else return;
        } else {
            if ((hitAngle <= 225F) && (hitAngle >= 135F)) {
                return;
            } else if ((hitAngle < 270F) && (hitAngle > 90F)) {
                character.groundSpeed = character.velocityPrev.y * Mathf.Sign(Mathf.Sin(Mathf.Deg2Rad * hitAngle));
            } else return;
        };

        // Set position and angle
        // -------------------------
        transform.eulerAngles = hitEuler;

        // If we don't snap to the ground, then we're still in the air and
        // should keep going the speed we were before.
        if (!character.GroundSnap()) {
            character.velocity = character.velocityPrev;
            return;
        }

        // Set state
        // -------------------------
        character.stateCurrent = "ground";
    }


    void UpdateAirAnimDirection() {
        if (Input.GetKey(KeyCode.LeftArrow) && !character.controlLock)
            character.facingRight = false;
        else if (Input.GetKey(KeyCode.RightArrow) && !character.controlLock)
            character.facingRight = true;
    }

    void UpdateAirAnim(float deltaTime) {
        UpdateAirAnimDirection();
        character.spriteContainer.transform.position = character.position;
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);
        character.flipX = !character.facingRight;
    }
}