using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using System.Collections.Dictionary;

public class ObjectCuller : MonoBehaviour
{
    public enum EnableType {
        Normal, // Enable object when it's onscreen
        Reset, // Enable and reset object when its origin comes onscreen
        Never, // Object should be permanently destroyed
        NeverAggressive, // Same as Never but immediately destroys object if not on screen
        Ignore // Do nothing, object always active
    }
    public EnableType enableType;

    public enum PositionType {
        Current, // calculate distance from current position
        Initial // ... from initial position
    }
    public PositionType positionType;

    // ========================================================================

    public Utils.AxisType axisType;
    public Utils.DistanceType distanceType;

    // ========================================================================

    public float triggerDistance = 8F;
    public bool runEveryOtherFrame = false;

    // ========================================================================

    Dictionary<Behaviour, bool> behaviourEnabledInitial = new Dictionary<Behaviour, bool>(); // Stores whether each component should be reenabled on visible

    void DisableBehaviours(bool force = false) {
        foreach(var behaviour in GetComponents<Behaviour>()) {
            if (behaviour == this) continue;
            if (behaviourEnabledInitial.ContainsKey(behaviour)) continue;
            if (behaviour is Animator && !force) continue;
            behaviourEnabledInitial.Add(behaviour, behaviour.enabled);
            behaviour.enabled = false;
        }
    }

    void EnableBehaviours() {
        foreach(var behaviour in GetComponents<Behaviour>()) {
            if (!behaviourEnabledInitial.ContainsKey(behaviour)) continue;
            behaviour.enabled = behaviourEnabledInitial[behaviour];
            behaviourEnabledInitial.Remove(behaviour);
        }
    }

    // ========================================================================

    Dictionary<GameObject, bool> childrenActiveInitial = new Dictionary<GameObject, bool>(); // Stores whether each component should be reenabled on visible

    void DisableChildren(bool force = false) {
        foreach(Transform child in transform) {
            GameObject childObj = child.gameObject;
            if ((childObj.GetComponent<IgnoreObjectCuller>() != null) && !force) continue;
            if (childrenActiveInitial.ContainsKey(childObj)) continue;
            childrenActiveInitial.Add(childObj, childObj.activeSelf);
            childObj.SetActive(false);
        }
    }

    void EnableChildren() {
        foreach(Transform child in transform) {
            GameObject childObj = child.gameObject;
            if (!childrenActiveInitial.ContainsKey(childObj)) continue;
            childObj.SetActive(childrenActiveInitial[childObj]);
            childrenActiveInitial.Remove(childObj);
        }
    }

    // ========================================================================

    Dictionary<Renderer, bool> rendererEnabledInitial = new Dictionary<Renderer, bool>(); // Stores whether each component should be reenabled on visible

    void DisableRenderers() {
        foreach(var renderer in GetComponents<Renderer>()) {
            if (rendererEnabledInitial.ContainsKey(renderer)) continue;
            rendererEnabledInitial.Add(renderer, renderer.enabled);
            renderer.enabled = false;
        }
    }

    void EnableRenderers() {
        foreach(var renderer in GetComponents<Renderer>()) {
            if (!rendererEnabledInitial.ContainsKey(renderer)) continue;
            renderer.enabled = rendererEnabledInitial[renderer];
            rendererEnabledInitial.Remove(renderer);
        }
    }

    // ========================================================================

    Dictionary<Collider, bool> colliderEnabledInitial = new Dictionary<Collider, bool>(); // Stores whether each component should be reenabled on visible

    void DisableColliders() {
        foreach(var collider in GetComponents<Collider>()) {
            if (colliderEnabledInitial.ContainsKey(collider)) continue;
            colliderEnabledInitial.Add(collider, collider.enabled);
            collider.enabled = false;
        }
    }

    void EnableColliders() {
        foreach(var collider in GetComponents<Collider>()) {
            if (!colliderEnabledInitial.ContainsKey(collider)) continue;
            collider.enabled = colliderEnabledInitial[collider];
            colliderEnabledInitial.Remove(collider);
        }
    }

    // ========================================================================

    struct RigidbodyState {
        public bool isKinematic;
        public bool detectCollisions;
        public Vector3 velocity;
    }
    Dictionary<Rigidbody, RigidbodyState> rigidbodyStatesInitial = new Dictionary<Rigidbody, RigidbodyState>();

    void DisableRigidbodies() {
        foreach(var rigidbody in GetComponents<Rigidbody>()) {
            if (rigidbodyStatesInitial.ContainsKey(rigidbody)) continue;
            rigidbodyStatesInitial.Add(
                rigidbody,
                new RigidbodyState {
                    isKinematic = rigidbody.isKinematic,
                    detectCollisions = rigidbody.detectCollisions,
                    velocity = rigidbody.velocity
                }
            );
            rigidbody.isKinematic = false;
            rigidbody.detectCollisions = false;
            rigidbody.velocity = Vector3.zero;
        }
    }

    void EnableRigidbodies() {
        foreach(var rigidbody in GetComponents<Rigidbody>()) {
            if (!rigidbodyStatesInitial.ContainsKey(rigidbody)) continue;
            rigidbody.isKinematic = rigidbodyStatesInitial[rigidbody].isKinematic;
            rigidbody.detectCollisions = rigidbodyStatesInitial[rigidbody].detectCollisions;
            rigidbody.velocity = rigidbodyStatesInitial[rigidbody].velocity;
            rigidbodyStatesInitial.Remove(rigidbody);
        }
    }

    // ========================================================================

    // Prevent iterating through all children multiple times by just keeping track of whether everything's disabled
    bool _selfEnabled = true;

    void DisableSelf(bool force = false) {
        if (!_selfEnabled) return;
        DisableRigidbodies();
        DisableBehaviours(force);
        DisableColliders();
        DisableChildren(force);
         if (force) DisableRenderers();
        _selfEnabled = false;
    }

    void EnableSelf() {
        if (_selfEnabled) return;
        EnableRigidbodies();
        EnableBehaviours();
        EnableRenderers();
        EnableColliders();
        EnableChildren();
        _selfEnabled = true;
    }

    // ========================================================================

    GameObject clone = null;
    LevelManager levelManager;
    bool everyOtherFrameCheck;
    Vector3 initialPosition;

    void Start() {
        initialPosition = transform.position;

        if (runEveryOtherFrame)
            everyOtherFrameCheck = Random.value > 0.5;

        // Level manager keeps track of all characters
        levelManager = Utils.GetLevelManager();

        if (enableType == EnableType.Reset) {
            clone = Instantiate(gameObject);
            clone.GetComponent<ObjectCuller>().enableWait = true;
            clone.SetActive(false);
            clone.transform.parent = transform.parent;
        }

        bool inRange = GetInRange();
        inRangePrev = inRange;

        if (!inRange || (enableType == EnableType.Reset)) DisableSelf();
    }

    bool GetInRange() {
        Vector3 position = Vector3.zero;

        switch (positionType) {
            case PositionType.Current:
                position = transform.position;
                break;
            case PositionType.Initial:
                position = initialPosition;
                break;
        }

        return Utils.CheckIfCharacterInRange(
            position,
            triggerDistance,
            axisType,
            distanceType,
            levelManager.characters
        ) != null;
    }

    // Update is called once per frame
    bool inRangePrev;
    public bool enableWait = false;
    bool readyForDestroy = false;

    void Update() {
        if (runEveryOtherFrame) {
            everyOtherFrameCheck = !everyOtherFrameCheck;
            if (everyOtherFrameCheck) return;
        }

        bool inRange = GetInRange();

        switch(enableType) {
            case EnableType.Normal:
                if (inRange) EnableSelf();
                else DisableSelf();
                break;
            case EnableType.Never:
                if (inRange) EnableSelf();
                else if (inRangePrev) DestroySelf();
                break;
            case EnableType.NeverAggressive:
                if (inRange) EnableSelf();
                else DestroySelf();
                break;
            case EnableType.Reset:
                if (readyForDestroy) {
                    bool inRangeInitial = Utils.CheckIfCharacterInRange(
                        initialPosition,
                        triggerDistance,
                        axisType,
                        distanceType,
                        levelManager.characters
                    ) != null;
                    if (inRangeInitial) break;
                    clone.SetActive(true);
                    DestroySelf();
                }else if (inRange && (!inRangePrev || !enableWait)) EnableSelf(); // Coming into range
                else if (!inRange && inRangePrev) { // Going out of range
                    readyForDestroy = true;
                    DisableSelf(true);
                } else if (!inRange) DisableSelf();
                break;
            default:
            case EnableType.Ignore:
                break;
        }

        inRangePrev = inRange;
    }

    bool _destroyedSelf = false;
    void DestroySelf() {
        _destroyedSelf = true;
        Destroy(gameObject);
    }

    void OnDestroy() {
        if ((clone != null) && !_destroyedSelf)
            Destroy(clone);
    }

    public void DestroyAll() {
        Destroy(gameObject);
    }
}