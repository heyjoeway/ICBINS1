using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class Character : GameBehaviour {
    public float topSpeedNormal { get {
        return 6F * physicsScale;
    }}
    public float topSpeedSpeedUp { get {
        return 12F * physicsScale;
    }}
    public float topSpeed { get {
        return HasEffect("speedUp") ? topSpeedSpeedUp : topSpeedNormal;
    }}

    public float terminalSpeed { get {
        return 16.5F * physicsScale;
    }}

    // ========================================================================

    public List<CharacterCapability> capabilities = new List<CharacterCapability>();

    public CharacterCapability GetCapability(string capabilityName) {
        foreach (CharacterCapability capability in capabilities)
            if (capability.name == capabilityName) return capability;
        return null;
    }

    public CharacterCapability TryGetCapability(string capabilityName, Action<CharacterCapability> callback) {
        CharacterCapability capability = GetCapability(capabilityName);
        if (capability != null) callback(capability);
        return capability;
    }

    // ========================================================================

    // [HideInInspector, SyncVar]
    [HideInInspector]
    public string statePrev = "ground";
    // [SyncVar]
    string _stateCurrent = "ground";
    public string stateCurrent {
        get { return _stateCurrent; }
        set { StateChange(value); }
    }

    void StateChange(string newState) {
        string stateCurrentCheck = stateCurrent;
        if (_stateCurrent == newState) return;
        foreach (CharacterCapability capability in capabilities)
            capability.StateDeinit(_stateCurrent, newState);

        if (stateCurrentCheck != stateCurrent) return; // Allows changing state in Deinit

        statePrev = _stateCurrent;
        _stateCurrent = newState;
        foreach (CharacterCapability capability in capabilities)
            capability.StateInit(_stateCurrent, statePrev);
    }

    Dictionary<string, List<string>> _stateGroups = new Dictionary<string, List<string>>();
    public bool InStateGroup(string groupName) {
        return InStateGroup(groupName, stateCurrent);
    }
    public bool InStateGroup(string groupName, string stateName) {
        if (!_stateGroups.ContainsKey(groupName)) return false;
        return _stateGroups[groupName].Contains(stateName);
    }

    public void AddStateGroup(string groupName, string stateName) {
        if (!_stateGroups.ContainsKey(groupName)) {
            _stateGroups[groupName] = new List<string> { stateName };
        } else {
            _stateGroups[groupName].Add(stateName);
        }
    }

    // ========================================================================

    public List<CharacterEffect> effects = new List<CharacterEffect>();

    public void UpdateEffects(float deltaTime) {
        // iterate backwards to prevent index from shifting
        // (effects remove themselves once complete)
        for (int i = effects.Count - 1; i >= 0; i--) {
            CharacterEffect effect = effects[i];
            effect.UpdateBase(deltaTime);

        }
    }

    public CharacterEffect GetEffect(string effectName) {
        foreach (CharacterEffect effect in effects) {
            if (effectName == effect.name) return effect;
        }
        return null;
    }

    public bool HasEffect(string effectName) {
        return GetEffect(effectName) != null;
    }

    public void ClearEffects() {
        // iterate backwards to prevent index from shifting
        for (int i = effects.Count - 1; i >= 0; i--) {
            CharacterEffect effect = effects[i];
            effect.DestroyBase();
        }
    }

    // ========================================================================
    
    public Vector3 position {
        get { return transform.position; }
        set { transform.position = value; }
    }

    
    public Vector3 eulerAngles {
        get { return transform.eulerAngles; }
        set { transform.eulerAngles = value; }
    }

    public float forwardAngle {
        get { return eulerAngles.z; }
        set { 
            Vector3 angle = eulerAngles;
            angle.z = value;
            eulerAngles = angle;
        }
    }

    public bool movingUphill { get {
        return Mathf.Sign(groundSpeed) == Mathf.Sign(Mathf.Sin(forwardAngle * Mathf.Deg2Rad));
    }}

    // ========================================================================

    [HideInInspector]
    public new Rigidbody rigidbody;
    public Vector3 velocity {
        get { return rigidbody.velocity; }
        set { rigidbody.velocity = value; }
    }
    // [HideInInspector, SyncVar]
    [HideInInspector]
    public Vector3 velocityPrev;

    // ========================================================================

    // [HideInInspector, SyncVar]
    [HideInInspector]
    public float groundSpeedPrev = 0;
    // [SyncVar]
    float _groundSpeed = 0;
    public float groundSpeed {
        get { return _groundSpeed; }
        set {
            _groundSpeed = value;
            groundSpeedRigidbody = _groundSpeed;
        }
    }

    // 3D-Ready: NO
    public float groundSpeedRigidbody {
        get {
            return (
                (velocity.x * Mathf.Cos(forwardAngle * Mathf.Deg2Rad)) +
                (velocity.y * Mathf.Sin(forwardAngle * Mathf.Deg2Rad))
            );
        }
        set {
            velocity = new Vector3(
                Mathf.Cos(forwardAngle * Mathf.Deg2Rad),
                Mathf.Sin(forwardAngle * Mathf.Deg2Rad),
                velocity.z
            ) * value;
        }
    }

    public void GroundSpeedSync() { _groundSpeed = groundSpeedRigidbody; }

    // ========================================================================
    
    public static LayerMask? _solidRaycastMask = null;
    public static LayerMask solidRaycastMask {
        get {
            if (_solidRaycastMask != null) return (LayerMask)_solidRaycastMask;
            _solidRaycastMask = LayerMask.GetMask(
                "Ignore Raycast",
                "Player - Ignore Top Solid and Raycast",
                "Player - Ignore Top Solid",
                "Player - Rolling",
                "Player - Rolling and Ignore Top Solid",
                "Player - Rolling and Ignore Top Solid",
                "Object - Player Only and Ignore Raycast",
                "Object - Ring"
            );
            return (LayerMask)_solidRaycastMask;
        }
    }

    // ========================================================================
    
    public Utils.RaycastHitHybrid GetGroundRaycast() {
        return GetSolidRaycast(-transform.up);
    }

    // 3D-Ready: YES
    public Utils.RaycastHitHybrid GetSolidRaycast(Vector3 direction, float maxDistance = 0.8F) {
        return Utils.RaycastHybrid(
            position, // origin
            direction.normalized, // direction,
            maxDistance * sizeScale, // max distance
            ~solidRaycastMask // layer mask
        );
    }

    bool GetIsGrounded() {
        Utils.RaycastHitHybrid hit = GetGroundRaycast();
        return GetIsGrounded(hit);
    }

    // 3D-Ready: NO
    bool GetIsGrounded(Utils.RaycastHitHybrid hit) { // Potentially avoid recomputing raycast
        if (!hit.isValid) return false;

        float angleDiff = Mathf.DeltaAngle(
            Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles.z,
            forwardAngle
        );
        return angleDiff < 67.5F;
    }

    // ========================================================================

    CharacterGroundedDetector _groundedDetectorCurrent;
    public CharacterGroundedDetector groundedDetectorCurrent {
        get { return _groundedDetectorCurrent; }
        set {
            if (value == _groundedDetectorCurrent) return;
            if (_groundedDetectorCurrent != null) _groundedDetectorCurrent.Leave(this);
            _groundedDetectorCurrent = value;
            if (_groundedDetectorCurrent != null) _groundedDetectorCurrent.Enter(this);
        }
    }

    public enum BalanceState {
        None,
        Left, // platform is to the left
        Right // platform is to the right
    }

    // [HideInInspector, SyncVar]
    [HideInInspector]
    public BalanceState balanceState = BalanceState.None;

    // Keeps character locked to ground while in ground state
    // 3D-Ready: No, but pretty close, actually.
    public bool GroundSnap() {
        Utils.RaycastHitHybrid hit = GetGroundRaycast();
        balanceState = BalanceState.None;

        if (GetIsGrounded(hit)) {
            transform.eulerAngles = new Vector3(
                0,
                0,
                Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles.z
            );

            Vector3 newPos = hit.point + (transform.up * 0.5F * sizeScale);
            newPos.z = position.z; // Comment this for 3D movement
            position = newPos;
            groundedDetectorCurrent = hit.transform.GetComponentInChildren<CharacterGroundedDetector>();
            return true;
        } else {
            // Didn't find the ground from the player center?
            // We might be on a ledge. Better check to the left and right of
            // the character to be sure.
            for (int dir = -1; dir <= 1; dir += 2) {
                Utils.RaycastHitHybrid hitLedge = Utils.RaycastHybrid(
                    position + (dir * transform.right * 0.375F * sizeScale * sizeScale), // origin
                    -transform.up, // direction,
                    0.8F * sizeScale, // max distance
                    ~solidRaycastMask // layer mask
                );
                if (hitLedge.isValid) {
                    balanceState = dir < 0 ? BalanceState.Left : BalanceState.Right;
                    groundedDetectorCurrent = hitLedge.transform.GetComponentInChildren<CharacterGroundedDetector>();

                    Vector3 newPos = (
                        hitLedge.point -
                        (dir * transform.right * 0.375F * sizeScale) +
                        (transform.up * 0.5F * sizeScale)
                    );
                    newPos.x = position.x;
                    newPos.z = position.z;
                    position = newPos;
                    return true;
                }
            }

            if (stateCurrent == "rolling") stateCurrent = "rollingAir";
            else stateCurrent = "air";
        }
        return false;
    }

    // ========================================================================
    
    [HideInInspector]
    public AudioSource audioSource;
    [HideInInspector]
    public Animator spriteAnimator;

    [HideInInspector]
    string spriteAnimatorStatePrev;
    // [HideInInspector, SyncVar]
    [HideInInspector]
    public string spriteAnimatorState;
    // [HideInInspector, SyncVar]
    [HideInInspector]
    public float spriteAnimatorSpeed;

    public void AnimatorPlay(string stateName, float normalizedTime = float.NegativeInfinity) {
        spriteAnimatorStatePrev = spriteAnimatorState;
        spriteAnimator.Play(stateName, -1, normalizedTime);
        spriteAnimatorState = stateName;
    }

    // ========================================================================

    // [SyncVar]
    public float sizeScale = 1F;

    public float physicsScale {
        get { return sizeScale * Utils.physicsScale; }
    }

    // [SyncVar]
    public bool flipX = false;

    // ========================================================================

    // [SyncVar]
    public bool facingRight = true;

    // ========================================================================
    
    [HideInInspector]
    public SpriteRenderer sprite;
    [HideInInspector]
    public Transform spriteContainer;

    // Gets rotation for sprite
    // 3D-Ready: No
    public Vector3 GetSpriteRotation(float deltaTime) {
        if (!GlobalOptions.Get<bool>("smoothRotation"))
            return (transform.eulerAngles / 45F).Round(0) * 45F;
    
        Vector3 currentRotation = sprite.transform.eulerAngles;
        bool shouldRotate = Mathf.Abs(Mathf.DeltaAngle(0, forwardAngle)) > 45;
        
        Vector3 targetAngle;
        if (shouldRotate) {
            targetAngle = transform.eulerAngles;
            if (forwardAngle > 180 && currentRotation.z == 0)
                currentRotation.z = 360;
        } else {
            targetAngle = currentRotation.z > 180 ?
                new Vector3(0,0,360) : Vector3.zero;
        }

        return Vector3.RotateTowards(
            currentRotation, // current
            targetAngle, // target // TODO: 3D
            10F * deltaTime * 60F, // max rotation
            10F * deltaTime * 60F // magnitude
        );
    }

    public float opacity = 1;

    // ========================================================================

    public ObjShield shield;

    // 3D-Ready: YES

    public void RemoveShield() {
        if (shield == null) return;
        Destroy(shield.gameObject);
        shield = null;
    }

    // ========================================================================

    public Level currentLevel;
    public float timer = 0;
    public bool timerPause = false;

    int _score = 0;
    public int score {
        get { return _score; }
        set {
            if (Mathf.Floor(value / 50000F) > Mathf.Floor(_score / 50000F))
                lives++;

            _score = value;
        }
    }
    public int destroyEnemyChain = 0;

    int _ringLivesMax = 0;
    int _rings;
    public int rings { 
        get { return _rings; }
        set {
            _rings = value;
            int livesPrev = lives;
            lives += Mathf.Max(0, (int)Mathf.Floor(_rings / 100F) - _ringLivesMax);
            _ringLivesMax = Mathf.Max(_ringLivesMax, (int)Mathf.Floor(_rings / 100F));
        }
    }
    int _lives = 3;
    public int lives {
        get { return _lives; }
        set {
            if (value > _lives) {
                MusicManager.current.Add(new MusicManager.MusicStackEntry{
                    introPath = "Music/1-Up",
                    disableSfx = true,
                    fadeInAfter = true,
                    priority = 10,
                    ignoreClear = true
                });
            }
            _lives = value;
        }
    }

    // ========================================================================

    [HideInInspector]
    public CharacterCamera characterCamera;

    public class RespawnData {
        public Vector3 position = Vector3.zero;
        public float timer = 0;
    }
    public RespawnData respawnData = new RespawnData();

    public int checkpointId = 0;

    public void Respawn() {
        SoftRespawn();
        if (checkpointId == 0) {
            if (currentLevel.cameraZoneStart != null)
                currentLevel.cameraZoneStart.Set(this);
        }

        if (characterCamera != null) {
            characterCamera.MinMaxPositionSnap();
            characterCamera.position = transform.position;
        }
    }

    public void SoftRespawn() { // Should only be used in multiplayer; for full respawns reload scene
        _rings = 0;
        _ringLivesMax = 0;
        effects.Clear();
        position = respawnData.position;
        timer = respawnData.timer;
        stateCurrent = "ground";
        velocity = Vector3.zero;
        _groundSpeed = 0;
        transform.eulerAngles = Vector3.zero;
        facingRight = true;

        timerPause = false;
        TryGetCapability("victory", (CharacterCapability capability) => {
            ((CharacterCapabilityVictory)capability).victoryLock = false;
        });
    }

    // ========================================================================

    public Vector2 positionMin = new Vector2(
        -Mathf.Infinity,
        -Mathf.Infinity
    );
    public Vector2 positionMax = new Vector2(
        Mathf.Infinity,
        Mathf.Infinity
    );

    public void LimitPosition() {
        Vector2 positionNew = Vector2.Min(
            Vector2.Max(
                position,
                positionMin
            ),
            positionMax
        );

        if ((Vector2)position != positionNew) {
            position = positionNew;
            _groundSpeed = 0;
        }
    }

    // ========================================================================

    [HideInInspector]
    public float horizontalInputLockTimer = 0;
    public bool controlLockManual = false;
    public bool controlLock { get { return (
        controlLockManual ||
        Time.timeScale == 0 ||
        InStateGroup("noControl") ||
        !isLocalPlayer
    ); }}

    // ========================================================================

    Transform _modeGroupCurrent;
    [HideInInspector]
    public Collider colliderCurrent;
    public Transform modeGroupCurrent {
        get { return _modeGroupCurrent; }
        set {
            if (_modeGroupCurrent == value) return;
  
            if (_modeGroupCurrent != null)
                _modeGroupCurrent.gameObject.SetActive(false);
  
            _modeGroupCurrent = value;
            colliderCurrent = null;
    
            if (_modeGroupCurrent != null) {
                _modeGroupCurrent.gameObject.SetActive(true);
                colliderCurrent = _modeGroupCurrent.Find("Collider").GetComponent<Collider>();
            }
        }
    }

    // ========================================================================

    public bool isInvulnerable { get {
        return (
            InStateGroup("ignore") ||
            HasEffect("invulnerable") ||
            HasEffect("invincible")
        );
    } }

    public bool isHarmful { get {
        return(
            InStateGroup("harmful") ||
            HasEffect("invincible")
        );
    }}

    // 3D-Ready: NO
    public void Hurt(bool moveLeft = true, bool spikes = false) {
        if (isInvulnerable) return;
        
        if (shield != null) {
            SFX.Play(audioSource, "sfxHurt");
            RemoveShield();
        } else if (rings == 0) {
            if (spikes) SFX.Play(audioSource, "sfxDieSpikes");
            else SFX.Play(audioSource, "sfxDie");
            stateCurrent = "dying";
            return;
        } else {
            ObjRing.ExplodeRings(transform.position, Mathf.Min(rings, 32));
            rings = 0;
            SFX.Play(audioSource, "sfxHurtRings");
        }

        stateCurrent = "hurt";
        velocity = new Vector3( // TODO: 3D
            2 * (moveLeft ? -1 : 1)  * physicsScale,
            4 * physicsScale,
            velocity.z
        );
        position += velocity / 30F; // HACK
    }

    // ========================================================================

    [HideInInspector]
    public Transform groundModeGroup;
    [HideInInspector]
    public Transform rollingModeGroup;
    [HideInInspector]
    public Transform airModeGroup;
    [HideInInspector]
    public Transform rollingAirModeGroup;
    [HideInInspector]
    public Collider rollingAirModeCollider;
    [HideInInspector]
    public Collider airModeCollider;

    [HideInInspector]
    HUD hud;

    // ========================================================================
    public virtual void Start() {
        rollingModeGroup.gameObject.SetActive(false);
        groundModeGroup.gameObject.SetActive(false);
        rollingAirModeGroup.gameObject.SetActive(false);
        airModeGroup.gameObject.SetActive(false);
        
        foreach (CharacterCapability capability in capabilities)
            capability.StateInit(stateCurrent, "");

        if (isLocalPlayer) {
            characterCamera = Instantiate(cameraPrefab).GetComponent<CharacterCamera>();
            characterCamera.character = this;
            characterCamera.UpdateDelta(0);

            ObjTitleCard titleCard = ObjTitleCard.Make(this);

            hud = Instantiate(hudPrefab).GetComponent<HUD>();
            hud.character = this;
            hud.Update();
        }

        respawnData.position = position;
        Respawn();
    }

    // ========================================================================
    public bool pressingLeft { get {
        return input.GetAxesNegative("Horizontal");
    }}
    public bool pressingRight { get {
        return input.GetAxesPositive("Horizontal") && !pressingLeft;
    }}

    // ========================================================================

    public bool isGhost = false;
    public bool isLocalPlayer = true;

    public override void UpdateDelta(float deltaTime) {
        UpdateEffects(deltaTime);

        if (!isLocalPlayer) {
            isGhost = true;
            if (spriteAnimatorState != spriteAnimatorStatePrev) {
                spriteAnimator.Play(spriteAnimatorState);
                spriteAnimatorStatePrev = spriteAnimatorState;
            }
        } else {
            groundSpeedPrev = groundSpeed;
            if (!timerPause) timer += deltaTime * Time.timeScale;
            if (!isHarmful) destroyEnemyChain = 0;

            foreach (CharacterCapability capability in capabilities) {
                capability.Update(deltaTime);
                input.enabled = !controlLock;
            }

            velocityPrev = velocity;
        }

        spriteAnimator.speed = spriteAnimatorSpeed;
        transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);
        spriteContainer.localScale = new Vector3( // Hacky
            sizeScale * (flipX ? -1 : 1),
            sizeScale * Mathf.Sign(spriteContainer.localScale.y),
            sizeScale * Mathf.Sign(spriteContainer.localScale.z)
        );
        Color colorTemp = sprite.color;
        colorTemp.a = opacity * (isGhost ? 0.5F : 1);
        sprite.color = colorTemp;

        LimitPosition();
    }

    public override void LateUpdateDelta(float deltaTime) {
        spriteContainer.transform.position = position;

        if (characterCamera != null)
            characterCamera.UpdateDelta(deltaTime);
    }

    // ========================================================================

    public void OnCollisionEnter(Collision collision) {
        foreach (CharacterCapability capability in capabilities)
            capability.OnCollisionEnter(collision);
    }

    public void OnCollisionStay(Collision collision) {
        foreach (CharacterCapability capability in capabilities)
            capability.OnCollisionStay(collision);
    }

    public void OnCollisionExit(Collision collision) {
        foreach (CharacterCapability capability in capabilities)
            capability.OnCollisionExit(collision);
    }

    public void OnTriggerEnter(Collider other) {
        foreach (CharacterCapability capability in capabilities)
            capability.OnTriggerEnter(other);
    }

    public void OnTriggerStay(Collider other) {
        foreach (CharacterCapability capability in capabilities)
            capability.OnTriggerStay(other);
    }

    public void OnTriggerExit(Collider other) {
        foreach (CharacterCapability capability in capabilities)
            capability.OnTriggerExit(other);
    }

    // ========================================================================

    public override void OnDestroy() {
        base.OnDestroy();

        LevelManager.current.characters.Remove(this);
        Destroy(characterCamera.gameObject);
        Destroy(spriteContainer.gameObject);
        Destroy(hud.gameObject);
        if (playerId < 0) return;
        foreach (Character character in LevelManager.current.characters) {
            if (character.playerId > playerId)        
                character.playerId--;
        }
    }

    public GameObject cameraPrefab;
    public GameObject spriteContainerPrefab;
    public GameObject hudPrefab;

    void InitReferences() {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        spriteContainer = Instantiate(spriteContainerPrefab).transform;
        sprite = spriteContainer.Find("Sprite").GetComponent<SpriteRenderer>();
        spriteAnimator = sprite.GetComponent<Animator>();

        groundModeGroup = transform.Find("Ground Mode");
        rollingModeGroup = transform.Find("Rolling Mode");
        airModeGroup = transform.Find("Air Mode");
        rollingAirModeGroup = transform.Find("Rolling Air Mode");

        rollingAirModeCollider = rollingAirModeGroup.Find("Collider").GetComponent<Collider>();
        airModeCollider = airModeGroup.Find("Collider").GetComponent<Collider>();
    }

    public InputCustom input;
    public int playerId = -1;

    public override void Awake() {
        base.Awake();

        LevelManager.current.characters.Add(this);
        playerId = LevelManager.current.GetFreePlayerId();
        input = new InputCustom(1);

        InitReferences();

        Level levelDefault = FindObjectOfType<Level>();

        if (currentLevel == null) {
            currentLevel = levelDefault;
            respawnData.position = levelDefault.spawnPosition;
            Respawn();
        }

        if (GlobalOptions.Get<bool>("tinyMode"))
            sizeScale = 0.5F;
    }

}