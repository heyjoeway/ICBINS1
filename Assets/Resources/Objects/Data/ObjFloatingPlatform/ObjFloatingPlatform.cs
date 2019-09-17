using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjFloatingPlatform : MonoBehaviour {

    Transform moveDestinationLocation;
    Transform platformTransform;
    CharacterGroundedDetector groundedDetector;

    void InitReferences() {
        groundedDetector = platformColliderObj.GetComponent<CharacterGroundedDetector>();
        moveDestinationLocation = transform.Find("Move Destination");
        platformTransform = transform.Find("Platform");
    }

    public enum PlatformType {
        stationary,
        falling,
        moving,
        trigger
    }
    public PlatformType type = PlatformType.falling;
    public GameObject platformColliderObj;

    public float fallWaitTime = 0.5F; 
    float fallTimer;
    bool touched {
        get { return groundedDetector.characters.Count > 0; }
    }
    bool touchedEver = false;

    bool falling = false;
    float fallSpeed = 0;
    public float gravity = -0.007292F;

    // ======================================

    Vector3 offsetNudge;
    Vector3 offsetOriginal;
    Vector3 offsetMove;

    Vector3 positionPrev;
    Vector3 position {
        get { return offsetNudge + offsetOriginal + offsetMove; }
    }

    float nudgeDistance = -3F/32F;
    float nudgeTime = 0F;
    float nudgeTimeMax = 0.5F;

    void Start() {
        InitReferences();
        offsetOriginal = platformTransform.position;
        offsetNudge = Vector3.zero;
        offsetMove = Vector3.zero;
        positionPrev = position;
    }

    void UpdateNudge() {
        if (groundedDetector.characters.Count == 0)
            nudgeTime -= Utils.cappedDeltaTime;
        else
            nudgeTime += Utils.cappedDeltaTime;

        nudgeTime = Mathf.Max(0, Mathf.Min(nudgeTimeMax, nudgeTime));
        offsetNudge.y = EasingFunction.EaseOutSine(
            0,
            nudgeDistance,
            nudgeTime / nudgeTimeMax
        );
    }

    float moveTime = 0F;
    public float moveTimeMax = 5F;
    Vector3 moveDestination { get {
        return moveDestinationLocation.position;
    } }

    void UpdateType() {
        switch(type) {
            case PlatformType.falling:
                if (falling) {
                    fallSpeed += gravity;
                    offsetMove.y += fallSpeed;
                }

                if (touchedEver) {
                    fallTimer -= Utils.cappedDeltaTime;
                    falling = fallTimer <= 0; 
                } else if (touched) {
                    touchedEver = true;
                    fallTimer = fallWaitTime;
                }
                break;
            case PlatformType.moving:
                moveTime += Utils.cappedDeltaTime;
                moveTime %= moveTimeMax;
                float movePercentage = 1 - Mathf.Abs(((moveTime / moveTimeMax) - 0.5F) * 2);
                
                offsetMove.x = EasingFunction.EaseInOutSine(
                    0,
                    moveDestination.x - offsetOriginal.x,
                    movePercentage
                );
                offsetMove.y = EasingFunction.EaseInOutSine(
                    0,
                    moveDestination.y - offsetOriginal.y,
                    movePercentage
                );
                offsetMove.z = EasingFunction.EaseInOutSine(
                    0,
                    moveDestination.z - offsetOriginal.z,
                    movePercentage
                );
                break;
        }
    }

    void Update() {
        UpdateType();
        UpdateNudge();

        foreach (var character in groundedDetector.characters)
            character.position = character.position + (position - positionPrev);

        platformTransform.position = position;
        positionPrev = position;
    }
}
