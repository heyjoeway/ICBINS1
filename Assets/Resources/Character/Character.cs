using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionCode2D.Renderers;

public class Character : MonoBehaviour {
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
                "Player - Rolling and Ignore Top Solid"
            );
            return (LayerMask)_solidRaycastMask;
        }
    }

    // ========================================================================
    // REFERENCES
    // ========================================================================
    // Objects
    public GameObject spriteObject;
    public GameObject cameraObject;
    Transform dashDustPosition;
    SpriteRenderer spriteRenderer;

    Transform groundModeGroup;
    Transform rollingModeGroup;
    Transform airModeGroup;
    Transform rollingAirModeGroup;
    Collider rollingAirModeCollider;
    Collider airModeCollider;
    
    Transform _modeGroupCurrent;
    Transform modeGroupCurrent {
        get { return _modeGroupCurrent; }
        set {
            if (_modeGroupCurrent == value) return;
  
            if (_modeGroupCurrent != null)
                _modeGroupCurrent.gameObject.SetActive(false);
  
            _modeGroupCurrent = value;
    
            if (_modeGroupCurrent != null)
              _modeGroupCurrent.gameObject.SetActive(true);
        }
    }
    Collider colliderCurrent {
        get { 
            if (_modeGroupCurrent == null) return null;
            return _modeGroupCurrent.Find("Collider").GetComponent<Collider>();
        }
    }

    // Components
    public Animator spriteAnimator;
    public CharacterCamera characterCamera;
    public CharacterPackage characterPackage;
    public new Rigidbody rigidbody;

    public AudioSource audioSource;

    bool _initReferencesDone = false;
    
    SpriteGhostTrailRenderer spriteGhostTrail;
    bool spriteGhostTrailEnabled { get {
        return (
            (speedUpTimer > 0) &&
            GlobalOptions.Get<bool>("afterImages")
        );
    }}

    public void InitReferences() {
        if (_initReferencesDone) return;

        characterPackage = GetComponentInParent<CharacterPackage>();

        spriteAnimator = spriteObject.transform.Find("Sprite").gameObject.GetComponent<Animator>();
        spriteRenderer = spriteObject.transform.Find("Sprite").gameObject.GetComponent<SpriteRenderer>();
        spriteGhostTrail = spriteObject.transform.Find("Sprite").gameObject.GetComponent<SpriteGhostTrailRenderer>();
        characterCamera = characterPackage.camera;
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        groundModeGroup = transform.Find("Ground Mode");
        rollingModeGroup = transform.Find("Rolling Mode");
        airModeGroup = transform.Find("Air Mode");
        rollingAirModeGroup = transform.Find("Rolling Air Mode");

        airModeCollider = airModeGroup.transform.Find("Collider").GetComponent<Collider>();
        rollingAirModeCollider = rollingAirModeGroup.transform.Find("Collider").GetComponent<Collider>();
        dashDustPosition = spriteObject.transform.Find("Spindash Dust Position");

        _initReferencesDone = true;
    }

    // ========================================================================
    // CONSTANTS
    // ========================================================================

    const float accelerationGroundNormal = 0.046875F;
    const float accelerationGroundSpeedUp = 0.09375F;
    float accelerationGround { get {
        return speedUpTimer > 0 ? accelerationGroundSpeedUp : accelerationGroundNormal;
    }}

    const float accelerationAirNormal = 0.09375F;
    const float accelerationAirSpeedUp = 0.1875F;
    float accelerationAir { get {
        return speedUpTimer > 0 ? accelerationAirSpeedUp : accelerationAirNormal;
    }}
    
    const float frictionGroundNormal = 0.046875F;
    const float frictionGroundSpeedUp = 0.09375F;
    float frictionGround { get {
        return speedUpTimer > 0 ? frictionGroundSpeedUp : frictionGroundNormal;
    }}

    const float frictionRollNormal = 0.0234375F;
    const float frictionRollSpeedUp = 0.046875F;
    float frictionRoll { get {
        return speedUpTimer > 0 ? frictionRollSpeedUp : frictionRollNormal;
    }}

    const float topSpeedNormal = 6F;
    const float topSpeedSpeedUp = 12F;
    float topSpeed { get {
        return speedUpTimer > 0 ? topSpeedSpeedUp : topSpeedNormal;
    }}

    const float terminalSpeed = 16.5F;
    const float decelerationGround = 0.5F;
    const float decelerationRoll = 0.125F;
    const float slopeFactorGround = 0.125F;
    const float slopeFactorRollUp = 0.078125F;
    const float slopeFactorRollDown = 0.3125F;
    const float jumpSpeed = 6.5F;
    const float gravity = -0.21875F;
    const float hurtGravity = -0.1875F;
    const float skidThreshold = 4.5F;
    const float unrollThreshold = 0.5F;
    const float fallThreshold = 2.5F;
    const float rollThreshold = 1.03125F;
    const float rollLockBoostSpeed = 3F;

    // ========================================================================
    // VARIABLES
    // ========================================================================

    GameObject spindashDust = null;
    public bool rollLock = false;
    public bool dropDashEnabled = true;
    public bool spinDashEnabled = true;
    public float invulnTimer = 0;

    // Dynamic Variables
    float sizeScale {
        get { return transform.localScale.x; }
        set { transform.localScale.Set(value, value, value); }
    }

    public float physicsScale {
        get { return sizeScale * Utils.physicsScale; }
    }

    public bool flipX {
        get { return spriteObject.transform.localScale.x < 0; }
        set {
            if (flipX == value) return;
            Vector3 newScale = spriteObject.transform.localScale;
            newScale.x *= -1;
            spriteObject.transform.localScale = newScale;
        }
    }

    public enum CharacterState {
        ground,
        air,
        rollingAir,
        jump,
        rolling,
        spindash,
        dying,
        drowning,
        dead,
        hurt,
        victory,
        obj, // behavior controlled by object
        custom // for custom behavior on extended classes
    }
    public CharacterState statePrev = CharacterState.ground;
    CharacterState _stateCurrent = CharacterState.ground;
    public CharacterState stateCurrent {
        get { return _stateCurrent; }
        set { StateChange(value); }
    }

    // ========================================================================

    int _score = 0;
    public int score {
        get { return _score; }
        set {
            if (Mathf.Floor(value / 50000F) > Mathf.Floor(_score / 50000F))
                lives++;

            _score = value;
        }
    }
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
                Utils.GetMusicManager().Add(new MusicManager.MusicStackEntry{
                    introPath = "Music/1-Up",
                    disableSfx = true,
                    fadeInAfter = true,
                    priority = 10
                });
            }
            _lives = value;
        }
    }

    // ========================================================================

    public int destroyEnemyChain = 0;

    // ========================================================================

    public bool controlLockManual = false;
    public bool controlLock { get { 
        return controlLockManual || Time.timeScale == 0;
    }}

    public float speedUpTimer = 0F;
    public float invincibilityTimer = 0F;

    // ========================================================================

    // Start is called before the first frame update
    void Start() {
        InitReferences();

        rollingModeGroup.gameObject.SetActive(false);
        groundModeGroup.gameObject.SetActive(false);
        rollingAirModeGroup.gameObject.SetActive(false);
        airModeGroup.gameObject.SetActive(false);

        StateInit(_stateCurrent);
        respawnData.position = position;
        Respawn();

        dropDashEnabled = GlobalOptions.Get<bool>("dropDash");
        spinDashEnabled = GlobalOptions.Get<bool>("spindash");
    }

    public Vector3 velocity {
        get { return rigidbody.velocity; }
        set { rigidbody.velocity = value; }
    }

    // Vector3 _position;
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

    // Update is called once per frame
    public bool timerPaused = false;
    public float timer = 0;
    public bool timerPause = false;
    public bool victoryLock = false;
    float deltaTime;
    float deltaTimeScaled;
    public void UpdateDelta(float deltaTime) {
        this.deltaTime = deltaTime; // hack
        this.deltaTimeScaled = deltaTime * Time.timeScale; // hack

        groundSpeedPrev = groundSpeed;
        if (!timerPause) timer += deltaTimeScaled;
        if (!isHarmful) destroyEnemyChain = 0;
        
        if (speedUpTimer > 0) speedUpTimer -= deltaTime;
        Utils.GetMusicManager().tempo = speedUpTimer > 0 ? 1.25F : 1;
        spriteGhostTrail.enabled = spriteGhostTrailEnabled;

        if (invincibilityTimer > 0) invincibilityTimer -= deltaTime;

        StateUpdate(stateCurrent);
        velocityPrev = velocity;
    }

    // ========================================================================

    public class RespawnData {
        public Vector3 position = Vector3.zero;
    }
    public RespawnData respawnData = new RespawnData();

    public int checkpointId = 0;

    public void Respawn() {
        timer = 0;
        SoftRespawn();
        if (checkpointId == 0)
            currentLevel.cameraZoneStart.Set(this);

        characterCamera.MinMaxPositionSnap();
        characterCamera.position = transform.position;
    }

    public void SoftRespawn() { // Should only be used in multiplayer; for full respawns reload scene
        _rings = 0;
        _ringLivesMax = 0;
        speedUpTimer = 0;
        invincibilityTimer = 0;
        invulnTimer = 0;
        position = respawnData.position;
        stateCurrent = CharacterState.ground;
        velocity = Vector3.zero;
        _groundSpeed = 0;
        transform.eulerAngles = Vector3.zero;
        facingRight = true;
    }

    // ========================================================================

    void StateInit(CharacterState state) {
        switch (state) {
            case CharacterState.spindash:
                spindashDust = Instantiate(
                    (GameObject)Resources.Load("Objects/Dash Dust (Spindash)"),
                    dashDustPosition.position,
                    Quaternion.identity
                );
                UpdateSpindashDust();

                spindashPower = -2F;
                SpindashCharge();
                modeGroupCurrent = rollingModeGroup;
                break;
            case CharacterState.rolling:
                switch(statePrev) {
                    default:
                        SFX.PlayOneShot(audioSource, "SFX/Sonic 1/S1_BE");
                        break;
                    case CharacterState.jump:
                    case CharacterState.spindash:
                        break;
                }
                modeGroupCurrent = rollingModeGroup;
                break;
            case CharacterState.ground:
                destroyEnemyChain = 0;
                pushing = false;
                modeGroupCurrent = groundModeGroup;
                break;
            case CharacterState.jump:
            case CharacterState.rollingAir:
                dropDashTimer = Mathf.Infinity;
                UpdateAirTopSoild();
                modeGroupCurrent = rollingAirModeGroup;
                break;
            case CharacterState.air:
                modeGroupCurrent = airModeGroup;
                break;
            case CharacterState.hurt:
                opacity = 1;
                modeGroupCurrent = airModeGroup;
                break;
            case CharacterState.dying:
                if (Utils.GetLevelManager().characterPackages.Count == 1)
                    Time.timeScale = 0;

                RemoveShield();
                opacity = 1;
                velocity = new Vector3(
                    0, 7, 0
                ) * physicsScale;
                SFX.Play(audioSource, "SFX/Sonic 1/S1_A3");
                spriteAnimator.Play("Dying");
                dyingTimer = 3F;
                modeGroupCurrent = null;
                break;
            case CharacterState.drowning:
                if (Utils.GetLevelManager().characterPackages.Count == 1)
                    Time.timeScale = 0;
  
                RemoveShield();
                opacity = 1;
                velocity = Vector3.zero;
                SFX.Play(audioSource, "SFX/Sonic 1/S1_B2");
                spriteAnimator.Play("Drowning");
                dyingTimer = 3F;
                modeGroupCurrent = null;
                break;
            case CharacterState.dead:
                modeGroupCurrent = null;
                lives--;
                ReloadLevel();
                break;
        }
    }

    public void ReloadLevel() {
        if (Utils.GetLevelManager().characterPackages.Count == 1) {
            Time.timeScale = 0;
            ScreenFade screenFade = Instantiate(
                Resources.Load<GameObject>("Objects/Screen Fade Out"),
                Vector3.zero,
                Quaternion.identity
            ).GetComponent<ScreenFade>();
            Utils.GetMusicManager().FadeOut();
            screenFade.onComplete = () => {
                victoryLock = false;
                timerPause = false;
                currentLevel.Reload();
            };
        } else SoftRespawn();
    }

    void StateDeinit(CharacterState state) {

        switch (state) {
            case CharacterState.rolling:
            case CharacterState.ground:
                detectorCurrent = null;
                break;
            case CharacterState.spindash:
                if (spindashDust != null) {
                    spindashDust.GetComponent<Animator>().Play("Spindash End");
                    spindashDust = null;
                }
                
                detectorCurrent = null;
                break;
        }
    }

    void StateChange(CharacterState newState) {
        if (_stateCurrent == newState) return;
        StateDeinit(_stateCurrent);
        statePrev = _stateCurrent;
        _stateCurrent = newState;
        StateInit(_stateCurrent);
    }

    void StateUpdate(CharacterState state) {
        switch (state) {
            case CharacterState.ground:
                UpdateGround();
                break;
            case CharacterState.air:
                UpdateAir();
                break;
            case CharacterState.jump:
                UpdateJump();
                break;
            case CharacterState.rollingAir:
                UpdateRollingAir();
                break;
            case CharacterState.rolling:
                UpdateRolling();
                break;
            case CharacterState.spindash:
                UpdateSpindash();
                break;
            case CharacterState.dying:
            case CharacterState.drowning:
                UpdateDying();
                break;
            case CharacterState.hurt:
                UpdateHurt();
                break;
            case CharacterState.victory:
                UpdateVictory();
                break;
        }
    }

    public void OnCollisionEnter(Collision collision) {
        switch (stateCurrent) {
            case CharacterState.rolling:
            case CharacterState.ground:
                OnCollisionGround(collision);
                break;
            case CharacterState.rollingAir:
            case CharacterState.jump:
            case CharacterState.air:
                OnCollisionAir(collision);
                break;
            case CharacterState.hurt:
                OnCollisionHurt(collision);
                break;
        }
    }

    public void OnCollisionStay(Collision collision) {
        switch (stateCurrent) {
            case CharacterState.rolling:
            case CharacterState.ground:
                OnCollisionGround(collision);
                break;
            case CharacterState.rollingAir:
            case CharacterState.jump:
            case CharacterState.air:
                OnCollisionAir(collision);
                break;
            case CharacterState.hurt:
                OnCollisionHurt(collision);
                break;
        }
    }

    public void OnCollisionExit(Collision collision) {

        switch (stateCurrent) {
            case CharacterState.ground:
            case CharacterState.rolling:
                pushing = false;
                break;
        }
    }

    // ========================================================================
    RaycastHit GetGroundRaycast() {
        return GetSolidRaycast(-transform.up);
    }

    // 3D-Ready: YES
    RaycastHit GetSolidRaycast(Vector3 direction, float maxDistance = 0.8F) {
        RaycastHit hit;
        Physics.Raycast(
            position, // origin
            direction.normalized, // direction,
            out hit,
            maxDistance * sizeScale, // max distance
            ~solidRaycastMask // layer mask
        );
        return hit;
    }

    bool GetIsGrounded() {
        RaycastHit hit = GetGroundRaycast();
        return GetIsGrounded(hit);
    }

    bool IsValidRaycastHit(RaycastHit hit) {
        return hit.collider != null;
    }

    // 3D-Ready: NO
    bool GetIsGrounded(RaycastHit hit) { // Potentially avoid recomputing raycast
        if (!IsValidRaycastHit(hit)) return false;

        float angleDiff = Mathf.DeltaAngle(
            Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles.z,
            forwardAngle
        );
        return angleDiff < 67.5F;
    }

    // ========================================================================

    public float groundSpeedPrev = 0;
    public float _groundSpeed = 0;
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

    float horizontalInputLockTimer = 0;

    // 3D-Ready: YES
    void UpdateGround() {
        UpdateGroundMove();
        UpdateGroundTerminalSpeed();
        UpdateGroundSnap();
        UpdateGroundSpindash();
        UpdateGroundJump();
        UpdateGroundAnim();
        UpdateGroundRoll();
        UpdateGroundFallOff();
        UpdateInvulnerable();
        UpdatePositionBounds();
        UpdateVictory();
    }

    // 3D-Ready: Sorta
    void UpdateGroundMove() {
        float accelerationMagnitude = 0F;

        int inputDir = 0;
        if ((horizontalInputLockTimer <= 0) && !controlLock) {
            // ORDER MATTERS!
            if (Input.GetKey(KeyCode.RightArrow)) inputDir = 1;
            if (Input.GetKey(KeyCode.LeftArrow)) inputDir = -1;
        } else horizontalInputLockTimer -= deltaTime;

        if (inputDir == 1) {
            if (groundSpeed < 0) {
                accelerationMagnitude = decelerationGround;
            } else if (groundSpeed < topSpeed * physicsScale) {
                accelerationMagnitude = accelerationGround;
            }
        } else if (inputDir == -1) {
            if (groundSpeed > 0) {
                accelerationMagnitude = -decelerationGround;
            } else if (groundSpeed > -topSpeed * physicsScale) {
                accelerationMagnitude = -accelerationGround;
            }
        } else {
            if (Mathf.Abs(groundSpeed) > 0.05F * physicsScale) {
                accelerationMagnitude = -Mathf.Sign(groundSpeed) * frictionGround;
            } else {
                groundSpeed = 0;
                accelerationMagnitude = 0;
            }
        }

        float slopeFactorAcc = slopeFactorGround * Mathf.Sin(forwardAngle * Mathf.Deg2Rad);
        if (Mathf.Abs(slopeFactorAcc) > 0.04)
            accelerationMagnitude -= slopeFactorAcc;

        groundSpeed += accelerationMagnitude * physicsScale * deltaTime * 60F;
    }

    CharacterGroundedDetector _detectorCurrent;
    CharacterGroundedDetector detectorCurrent {
        get { return _detectorCurrent; }
        set {
            if (value == _detectorCurrent) return;
            if (_detectorCurrent != null) _detectorCurrent.Leave(this);
            _detectorCurrent = value;
            if (_detectorCurrent != null) _detectorCurrent.Enter(this);
        }
    }

    enum BalanceState {
        None,
        Left, // platform is to the left
        Right // platform is to the right
    }

    BalanceState balanceState = BalanceState.None;

    // Keeps character locked to ground while in ground state
    // 3D-Ready: No, but pretty close, actually.
    bool UpdateGroundSnap() {
        RaycastHit potentialHit = GetGroundRaycast();
        balanceState = BalanceState.None;

        if (GetIsGrounded(potentialHit)) {
            RaycastHit hit = (RaycastHit)potentialHit;
            transform.eulerAngles = new Vector3(
                0,
                0,
                Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles.z
            );

            Vector3 newPos = hit.point + (transform.up * 0.5F);
            newPos.z = position.z; // Comment this for 3D movement
            position = newPos;
            detectorCurrent = hit.collider.GetComponent<CharacterGroundedDetector>();
            return true;
        } else {
            // Didn't find the ground from the player center?
            // We might be on a ledge. Better check to the left and right of
            // the character to be sure.
            for (int dir = -1; dir <= 1; dir += 2) {
                RaycastHit potentialHitLedge;
                Physics.Raycast(
                    position + (dir * transform.right * 0.375F * sizeScale), // origin
                    -transform.up, // direction,
                    out potentialHitLedge,
                    0.8F * sizeScale, // max distance
                    ~solidRaycastMask // layer mask
                );
                if (IsValidRaycastHit(potentialHitLedge)) {
                    balanceState = dir < 0 ? BalanceState.Left : BalanceState.Right;
                    detectorCurrent = potentialHitLedge.collider.GetComponent<CharacterGroundedDetector>();

                    Vector3 newPos = (
                        potentialHitLedge.point -
                        (dir * transform.right * 0.375F * sizeScale) +
                        (transform.up * 0.5F)
                    );
                    newPos.x = position.x;
                    newPos.z = position.z;
                    position = newPos;
                    return true;
                }
            }

            if (stateCurrent == CharacterState.rolling) stateCurrent = CharacterState.rollingAir;
            else stateCurrent = CharacterState.air;
        }
        return false;
    }

    // Updates the character's animation while they're on the ground
    // 3D-Ready: NO
    void UpdateGroundAnim() {
        bool ignoreFlipX = false;
        spriteAnimator.speed = 1;

        // Check if we are transitioning to a rolling air state. If so, set the speed of it
        if (inRollingAirState) {
            spriteAnimator.speed = 1 + (
                (
                    Mathf.Abs(groundSpeed) /
                    (topSpeedNormal * physicsScale)
                ) * 2F
            );
        } else {
            bool pressLeft = Input.GetKey(KeyCode.LeftArrow) && !controlLock;
            bool pressRight = Input.GetKey(KeyCode.RightArrow) && !pressLeft && !controlLock;

            // Turning
            // ======================
            if (pressLeft && (groundSpeed < 0)) facingRight = false;
            else if (pressRight && (groundSpeed > 0)) facingRight = true;

            // Skidding
            // ======================
            bool skidding = (
                (pressRight && groundSpeed < 0) ||
                (pressLeft && groundSpeed > 0)
            );

            // You can only trigger a skid state if:
            // - Your angle (a) is <= 45d or >= 270d and your absolute speed is above the threshhold
            // - OR you're already skidding
            bool canSkid = (
                (
                    (
                        (forwardAngle <= 45F) ||
                        (forwardAngle >= 270F)
                    ) && (
                        Mathf.Abs(groundSpeed) >= skidThreshold * physicsScale
                    )
                ) || spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Skid")
            );

            // Standing still, looking up/down, idle animation
            // ======================
            if (groundSpeed == 0) {
                if (Input.GetKey(KeyCode.DownArrow) && !controlLock) spriteAnimator.Play("Look Down");
                else if (Input.GetKey(KeyCode.UpArrow) && !controlLock) spriteAnimator.Play("Look Up");
                else if (balanceState != BalanceState.None) {
                    ignoreFlipX = true;
                    flipX = balanceState == BalanceState.Right;
                    spriteAnimator.Play("Balancing");
                } else {
                    if (
                        !spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Tap") &&
                        !spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle")
                    ) spriteAnimator.Play("Idle");
                }
            // Pushing anim
            // ======================
            } else if (pushing) {
                spriteAnimator.Play("Push");
                spriteAnimator.speed = 1 + (Mathf.Abs(groundSpeed) / (topSpeedNormal * physicsScale));
            // Skidding, again
            // ======================
            } else if (skidding && canSkid) {
                if (!spriteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Skid"))
                    SFX.Play(audioSource, "SFX/Sonic 1/S1_A4");

                spriteAnimator.Play("Skid");
            // Walking
            // ======================
            } else if (Mathf.Abs(groundSpeed) < topSpeedNormal * physicsScale) {
                spriteAnimator.Play("Walk");
                spriteAnimator.speed = 1 + (Mathf.Abs(groundSpeed) / (topSpeedNormal * physicsScale));
            } else {
            // Running
            // ======================
                spriteAnimator.Play("Run");
                spriteAnimator.speed = Mathf.Abs(groundSpeed) / (topSpeedNormal * physicsScale);
            }
            // (Peel out anim goes here)
        }

        // Final value application
        // ======================
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = GetSpriteRotation();
        if (!ignoreFlipX) flipX = !facingRight;
    }

    // Switches the character to rolling state if connditions are met:
    // - Pressing roll key
    // - Moving faster than roll threshold
    // 3D-Ready: YES
    void UpdateGroundRoll() {
        if (controlLock) return;
        if (!InputCustom.GetKeyDownPreventRepeat(KeyCode.DownArrow)) return;
        if (Mathf.Abs(groundSpeed) < rollThreshold * physicsScale) return;
        stateCurrent = CharacterState.rolling;
    }

    // Switches the character to spindash state if connditions are met:
    // - Spindash enabled
    // - Pressing spindash key combo
    // - Standing still
    // 3D-Ready: YES
    void UpdateGroundSpindash() {
        if (!spinDashEnabled) return;
        if (controlLock) return;
        if (!Input.GetKey(KeyCode.DownArrow)) return;
        if (!InputCustom.GetKeyDownPreventRepeat(KeyCode.D)) return;
        if (groundSpeed != 0) return;
        stateCurrent = CharacterState.spindash;
    }

    // Switches the character to jump state if connditions are met:
    // - Pressing jump key
    // See: https://info.sonicretro.org/SPG:Solid_Tiles
    // 3D-Ready: YES
    void UpdateGroundJump() {
        // Sorta hack? This function still runs even after the state has changed to spindash
        if (stateCurrent == CharacterState.spindash) return;
        if (controlLock) return;
        if (!InputCustom.GetKeyDownPreventRepeat(KeyCode.D)) return;

        velocity += transform.up * jumpSpeed * physicsScale;
        SFX.PlayOneShot(audioSource, "SFX/Sonic 1/S1_A0");
        stateCurrent = CharacterState.jump;
    }

    // Gets rotation for sprite
    // 3D-Ready: No
    Vector3 GetSpriteRotation() {
        if (!GlobalOptions.Get<bool>("smoothRotation"))
            return (transform.eulerAngles / 45F).Round(0) * 45F;
    
        Vector3 currentRotation = spriteObject.transform.eulerAngles;
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

    // 3D-Ready: YES
    void UpdateGroundFallOff() {
        // SPG says these angles should be 270 and 90... I don't really believe that
        // That results in some wicked Sonic 4 walk up walls shenanigans

        if (horizontalInputLockTimer > 0) return;
        if (Mathf.Abs(groundSpeed) >= fallThreshold * physicsScale) return;

        if (!((forwardAngle <= 315) && (forwardAngle >= 45))) return;
        horizontalInputLockTimer = 0.5F;

        if (!((forwardAngle <= 270) && (forwardAngle >= 90))) {
        //     groundSpeed = 0;
            return;
        }

        if (stateCurrent == CharacterState.rolling) stateCurrent = CharacterState.rollingAir;
        else stateCurrent = CharacterState.air;
    }

    bool pushing = false;

    // 3D-Ready: NO
    void OnCollisionGround(Collision collision) {
        // Prevent tight spaces from slowing down character
        // (tight spaces may cause collisions on top and bottom of char)
        // This is kinda a hack, but it works.
        Vector3 collisionPoint = collision.GetContact(0).point;
        float collisionAngle = Quaternion.FromToRotation(
            Vector3.up,
            collision.GetContact(0).normal
        ).eulerAngles.z - 90F;

        if (
            (
                Mathf.Abs(Mathf.DeltaAngle(
                    collisionAngle, forwardAngle
                )) > 30F
            ) && (
                Mathf.Abs(Mathf.DeltaAngle(
                    collisionAngle, forwardAngle + 180
                )) > 30F
            )
        ) return;

        bool pressLeft = Input.GetKey(KeyCode.LeftArrow) && !controlLock;
        bool pushLeft = pressLeft && (collisionPoint.x < position.x);

        bool pressRight = Input.GetKey(KeyCode.RightArrow) && !pressLeft && !controlLock;
        bool pushRight = pressRight && (collisionPoint.x > position.x);

        pushing = pushLeft || pushRight;

        GroundSpeedSync();
    }

    // 3D-Ready: YES
    void UpdateGroundTerminalSpeed() {
        groundSpeed = Mathf.Min(
            Mathf.Abs(groundSpeed),
            terminalSpeed * physicsScale
        ) * Mathf.Sign(groundSpeed);
    }

    // ========================================================================

    float spindashPower;
    const float spindashPowerMax = 8F;
    const float spindashSpeedMax = 12F;

    // See: https://info.sonicretro.org/SPG:Special_Abilities#Spindash_.28Sonic_2.2C_3.2C_.26_K.29
    // 3D-Ready: Yes
    void UpdateSpindash() {
        UpdateGroundSnap();
        UpdateSpindashDrain();
        UpdateSpindashInput();
        UpdateSpindashAnim();
        UpdateSpindashDust();
        UpdateInvulnerable();
        UpdatePositionBounds();
        UpdateVictory();
    }

    // 3D-Ready: YES
    void UpdateSpindashDust() {
        if (spindashDust == null) return;
        spindashDust.transform.position = dashDustPosition.position;
        spindashDust.transform.localScale = spriteObject.transform.localScale;
    }

    // 3D-Ready: YES
    void UpdateSpindashInput() {
        if (!Input.GetKey(KeyCode.DownArrow) || controlLock) {
            SpindashRelease();
            return;
        }

        if (InputCustom.GetKeyDownPreventRepeat(KeyCode.D) && !controlLock) SpindashCharge();
    }

    // 3D-Ready: YES
    void UpdateSpindashDrain() {
        spindashPower -= ((spindashPower / 0.125F) / 256F) * deltaTime * 60F;
        spindashPower = Mathf.Max(0, spindashPower);
    }

    // 3D-Ready: YES
    void SpindashCharge() {
        spriteAnimator.Play("Spindash", 0, 0);
        spindashPower += 2;
        Debug.Log(spindashPower);
        SFX.Play(audioSource, "SFX/Sonic 2/S2_60",
            1 + ((spindashPower / (spindashPowerMax + 2)) * 0.5F)
        );
    }

    // 3D-Ready: YES
    void SpindashRelease() {
        groundSpeed = (
            (facingRight ? 1 : -1) * 
            (8F + (Mathf.Floor(spindashPower) / 2F)) *
            physicsScale
        );
        characterCamera.lagTimer = 0.26667F;
        SFX.Play(audioSource, "SFX/Sonic 1/S1_BC");
        stateCurrent = CharacterState.rolling;
    }

    // 3D-Ready: YES
    void UpdateSpindashAnim() {
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = GetSpriteRotation();
        flipX = !facingRight;
    }

    bool facingRight = true;
    // ========================================================================

    // 3D-Ready: YES
    void UpdateRolling() {
        UpdateRollingMove();
        UpdateGroundTerminalSpeed();
        UpdateGroundSnap();
        if (!rollLock) UpdateGroundJump();
        UpdateRollingAnim();
        UpdateInvulnerable();
        UpdatePositionBounds();
        UpdateVictory();
    }

    // 3D-Ready: YES
    bool movingUphill { get {
        return Mathf.Sign(groundSpeed) == Mathf.Sign(Mathf.Sin(forwardAngle * Mathf.Deg2Rad));
    }}

    // Handles movement while rolling
    // See: https://info.sonicretro.org/SPG:Rolling
    // 3D-Ready: NO
    void UpdateRollingMove() {
        float accelerationMagnitude = 0F;

        if (Input.GetKey(KeyCode.LeftArrow) && !controlLock) {
            if (groundSpeed > 0)
                accelerationMagnitude = -decelerationRoll;
        } else if (Input.GetKey(KeyCode.RightArrow) && !controlLock) {
            if (groundSpeed < 0)
                accelerationMagnitude = decelerationRoll;
        }

        if (Mathf.Abs(groundSpeed) > 0.05F * physicsScale)
            accelerationMagnitude -= Mathf.Sign(groundSpeed) * frictionRoll;

        float slopeFactor = movingUphill ? slopeFactorRollUp : slopeFactorRollDown;
        accelerationMagnitude -= slopeFactor * Mathf.Sin(forwardAngle * Mathf.Deg2Rad);
        groundSpeed += accelerationMagnitude * physicsScale * deltaTime * 60F;

        // Unroll / roll lock boost
        if (Mathf.Abs(groundSpeed) < unrollThreshold * physicsScale) {
            if (rollLock) {
                if ((forwardAngle < 270) && (forwardAngle > 180)) facingRight = true;
                if ((forwardAngle > 45) && (forwardAngle < 180)) facingRight = false;

                groundSpeed = rollLockBoostSpeed * physicsScale * (facingRight ? 1 : -1);
            } else {
                groundSpeed = 0;
                stateCurrent = CharacterState.ground;
            }
        }
    }

    // 3D-Ready: YES
    void UpdateRollingAnim() {
        spriteAnimator.Play("Roll");
        spriteAnimator.speed = 1 + ((Mathf.Abs(groundSpeed) / (topSpeedNormal * physicsScale)) * 2F);
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = Vector3.zero;
        flipX = !facingRight;
    }

    // ========================================================================

    // 3D-Ready: YES
    void UpdateAir() {
        UpdateAirMove();
        UpdateAirRotation();
        UpdateAirGravity();
        UpdateAirAnim();
        UpdateAirTopSoild();
        UpdateInvulnerable();
        GroundSpeedSync();
        UpdatePositionBounds();
    }

    // 3D-Ready: YES
    void UpdateAirTopSoild() {
        if (velocity.y >= 0) {
            rollingAirModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Rolling and Ignore Top Solid");
            airModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Ignore Top Solid");
        } else {
            rollingAirModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Rolling");
            airModeCollider.gameObject.layer = LayerMask.NameToLayer("Player - Default");
        }
    }

    // See: https://info.sonicretro.org/SPG:Jumping
    // 3D-Ready: NO
    void UpdateAirMove() {
        Vector3 velocityTemp = velocity;
        float accelerationMagnitude = 0;

        // Acceleration
        if (Input.GetKey(KeyCode.LeftArrow) && !controlLock) {
            if (velocityTemp.x > -topSpeed * physicsScale) {
                accelerationMagnitude = -accelerationAir;
            }
        } else if (Input.GetKey(KeyCode.RightArrow) && !controlLock) {
            if (velocityTemp.x < topSpeed * physicsScale) {
                accelerationMagnitude = accelerationAir;
            }
        }

        Vector3 acceleration = new Vector3(
            accelerationMagnitude,
            0,
            0
        ) * physicsScale * deltaTime * 60F;
        velocityTemp += acceleration;

        // Air Drag
        if ((velocityTemp.y > 0 ) && (velocityTemp.y < 4F * physicsScale))
            velocityTemp.x -= (
                ((int)(velocityTemp.x / 0.125F)) / 256F
            ) * (deltaTime * 60F);

        velocity = velocityTemp;
    }

    // 3D-Ready: YES
    void UpdateAirRotation() {
        transform.eulerAngles = Vector3.RotateTowards(
            transform.eulerAngles, // current
            forwardAngle <= 180 ? Vector3.zero : new Vector3(0, 0, 360), // target // TODO: 3D
            0.5F * deltaTime * 60F, // max rotation
            2F * deltaTime * 60F // magnitude
        );
    }
    
    // 3D-Ready: Yes
    void UpdateAirGravity() {
        velocity += Vector3.up * gravity * physicsScale * deltaTime * 60F;
    }

    // Handle air collisions
    // Called directly from rigidbody component
    const float angleDistFlat = 20.5F;
    const float angleDistSteep = 45F;
    const float angleDistWall = 90F;

    // Ready for hell?
    // Have fun
    // 3D-Ready: ABSO-FUCKING-LUTELY NOT
    void OnCollisionAir(Collision collision) {
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
        RaycastHit potentialHit = GetSolidRaycast(
            collision.GetContact(0).point - transform.position
        );
        if (!IsValidRaycastHit(potentialHit)) return;
        RaycastHit hit = (RaycastHit)potentialHit;

        Vector3 hitEuler = Quaternion.FromToRotation(Vector3.up, hit.normal).eulerAngles;
        // Round this or any tiiiny deviation in angle can allow the character
        // to jump at walls and stick to them
        float hitAngle = Mathf.Round(hitEuler.z); // TODO: 3D

        // This looks like a mess, but honestly this is about as simple as it can be.
        // This is pretty much implemented 1:1 from the SPG, so read that for an explanation
        // See: https://info.sonicretro.org/SPG:Solid_Tiles#Reacquisition_Of_The_Ground
        if (velocityPrev.y <= 0) {
            if ((hitAngle <= angleDistFlat) || (hitAngle >= 360 - angleDistFlat)) {
                groundSpeed = velocityPrev.x;
            } else if ((hitAngle <= angleDistSteep) || (hitAngle >= 360 - angleDistSteep)) {

                if (Mathf.Abs(velocityPrev.x) > Mathf.Abs(velocityPrev.y))
                    groundSpeed = velocityPrev.x;
                else
                    groundSpeed = velocityPrev.y * Mathf.Sign(Mathf.Sin(Mathf.Deg2Rad * hitAngle)) * 0.5F;
            } else if ((hitAngle < angleDistWall) || (hitAngle > 360F - angleDistWall)) {

                if (Mathf.Abs(velocityPrev.x) > Mathf.Abs(velocityPrev.y))
                    groundSpeed = velocityPrev.x;
                else
                    groundSpeed = velocityPrev.y * Mathf.Sign(Mathf.Sin(Mathf.Deg2Rad * hitAngle));
            } else return;
        } else {
            if ((hitAngle <= 225F) && (hitAngle >= 135F)) {
                return;
            } else if ((hitAngle < 270F) && (hitAngle > 90F)) {
                groundSpeed = velocityPrev.y * Mathf.Sign(Mathf.Sin(Mathf.Deg2Rad * hitAngle));
            } else return;
        };

        // Set position and angle
        // -------------------------
        transform.eulerAngles = hitEuler;

        // If we don't snap to the ground, then we're still in the air and
        // should keep going the speed we were before.
        if (!UpdateGroundSnap()) {
            rigidbody.velocity = velocityPrev;
            return;
        }

        // Set state
        // -------------------------
        if (isDropDashing) DropDashRelease();
        else if (
            Input.GetKey(KeyCode.DownArrow) &&
            (Mathf.Abs(groundSpeed) >= rollThreshold * physicsScale) &&
            !controlLock
        ) {
            SFX.Play(audioSource, "SFX/Sonic 1/S1_BE");
            stateCurrent = CharacterState.rolling;
        } else stateCurrent = CharacterState.ground;
    }

    public Vector3 velocityPrev;
    // void FixedUpdate() { velocityPrev = velocity; }

    void UpdateAirAnimDirection() {
        if (Input.GetKey(KeyCode.LeftArrow) && !controlLock) facingRight = false;
        else if (Input.GetKey(KeyCode.RightArrow) && !controlLock) facingRight = true;
    }

    void UpdateAirAnim() {
        UpdateAirAnimDirection();
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = GetSpriteRotation();
        flipX = !facingRight;
    }

    // ========================================================================

    // 3D-Ready: YES
    void UpdateRollingAir() {
        UpdateAirMove();
        UpdateAirGravity();
        UpdateRollingAirAnim();
        UpdateAirTopSoild();
        UpdateInvulnerable();
        UpdateRollingAirRotation();
        GroundSpeedSync();
        UpdatePositionBounds();
    }

    // 3D-Ready: YES
    void UpdateRollingAirAnim() {
        UpdateAirAnimDirection();
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = Vector3.zero;
        flipX = !facingRight;
    }

    // 3D-Ready: YES
    void UpdateRollingAirRotation() {
        transform.eulerAngles = Vector3.zero;
    }

    // ========================================================================

    float dropDashTimer;
    public bool isDropDashing { get { return (
        (stateCurrent == CharacterState.jump) &&
        (dropDashTimer <= 0) &&
        Input.GetKey(KeyCode.D) &&
        !controlLock
    ); }}

    // 3D-Ready: YES
    void UpdateJump() {
        UpdateAirMove();
        UpdateAirGravity();
        UpdateRollingAirAnim();
        UpdateAirTopSoild();
        UpdateInvulnerable();
        UpdateJumpDropDashStart();
        UpdateJumpHeight();
        UpdateJumpAnim();
        UpdateRollingAirRotation();
        UpdatePositionBounds();
        GroundSpeedSync();
    }

    // 3D-Ready: YES
    void UpdateJumpDropDashStart() {
        if (!dropDashEnabled) return;
        
        if (InputCustom.GetKeyDownPreventRepeat(KeyCode.D) && !controlLock)
            dropDashTimer = 0.33333F;

        if (Input.GetKey(KeyCode.D) && dropDashTimer > 0 && !controlLock) {
            dropDashTimer -= deltaTime;

            if (isDropDashing)
                SFX.Play(audioSource, "SFX/Sonic 2/S2_60");
        }
    }

    // 3D-Ready: YES
    void UpdateJumpHeight() {
        if (!Input.GetKey(KeyCode.D) || controlLock) {
            if (velocity.y > 4 * physicsScale)
                velocity = new Vector3(
                    velocity.x,
                    4 * physicsScale,
                    velocity.z
                );
        }
    }

    // 3D-Ready: YES
    void UpdateJumpAnim() {
        UpdateAirAnimDirection();
        if (isDropDashing) spriteAnimator.Play("Drop Dash");
        else spriteAnimator.Play("Roll");
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = Vector3.zero;
        flipX = !facingRight;
    }

    // 3D-Ready: YES
    void DropDashRelease() {
        SFX.Play(audioSource, "SFX/Sonic 1/S1_BC");
        stateCurrent = CharacterState.rolling;
        characterCamera.lagTimer = 0.26667F;

        GameObject dust = Instantiate(
            (GameObject)Resources.Load("Objects/Dash Dust (Drop Dash)"),
            dashDustPosition.position,
            Quaternion.identity
        );
        dust.transform.localScale = spriteObject.transform.localScale;

        float dashSpeed = 8F;
        float maxSpeed = 12F;
        if (!facingRight) {
            if (velocity.x <= 0) {
                groundSpeed = Mathf.Min(
                    -maxSpeed,
                    (groundSpeed / 4F) - dashSpeed
                );
                return;
            }
            if (Mathf.Floor(transform.rotation.z) > 0) {
                groundSpeed = (groundSpeed / 2F) - dashSpeed;
                return;
            }
            dashSpeed = -dashSpeed;
        } else {
            if (velocity.x >= 0) {
                float v5 = 
                groundSpeed = Mathf.Max(
                    dashSpeed + (groundSpeed / 4F),
                    maxSpeed
                );
                return;
            }
            if (Mathf.Floor(transform.rotation.z) > 0) {
                groundSpeed = dashSpeed + (groundSpeed / 2F);
                return;
            }
        }
        groundSpeed = dashSpeed * physicsScale;
    }

    // ========================================================================

    float dyingTimer = 3F;

    // 3D-Ready: YES
    void UpdateDying() {
        UpdateAirGravity();
        dyingTimer -= deltaTime;
        if (dyingTimer <= 0) stateCurrent = CharacterState.dead;
        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = Vector3.zero;
        flipX = false;

        if (Time.timeScale == 0)
            transform.position = position + (velocity * deltaTime);
    }
 
    // ========================================================================

    // 3D-Ready: NO
    public void Hurt(bool moveLeft = true, bool spikes = false) {
        if (isInvulnerable) return;
        
        if (shield != null) {
            SFX.Play(audioSource, "SFX/Sonic 1/S1_A3");
            RemoveShield();
        } else if (rings == 0) {
            if (spikes) SFX.Play(audioSource, "SFX/Sonic 1/S1_A6");
            else SFX.Play(audioSource, "SFX/Sonic 1/S1_A3");
            stateCurrent = CharacterState.dying;
            return;
        } else {
            ObjRing.ExplodeRings(transform.position, Mathf.Min(rings, 32));
            rings = 0;
            SFX.Play(audioSource, "SFX/Sonic 1/S1_C6");
        }

        stateCurrent = CharacterState.hurt;
        velocity = new Vector3( // TODO: 3D
            2 * (moveLeft ? -1 : 1)  * physicsScale,
            4 * physicsScale,
            velocity.z
        );
        position += velocity / 30F; // HACK
    }

    // 3D-Ready: YES
    void UpdateHurt() {
        UpdateAirTopSoild();
        UpdateHurtGravity();
        UpdateHurtAnim();
        UpdatePositionBounds();
    }

    // 3D-Ready: YES
    void UpdateHurtGravity() {
        velocity += new Vector3(
            0,
            hurtGravity,
            0
        ) * physicsScale * deltaTime * 60F;
    }

    // 3D-Ready: YES
    void UpdateHurtAnim() {
        spriteAnimator.Play("Hurt");

        UpdateSpritePosition();
        spriteObject.transform.eulerAngles = Vector3.zero;
        flipX = !facingRight;
    }

    // 3D-Ready: YES
    void OnCollisionHurt(Collision collision) {
        OnCollisionAir(collision);
        if (inGroundedState) {
            groundSpeed = 0;
            stateCurrent = CharacterState.ground;
            invulnTimer = 2F;
        }
    }

    // ========================================================================

    // https://info.sonicretro.org/SPG:Rebound#Badniks
    // 3D-Ready: YES
    public void BounceEnemy(GameObject enemyObj) {
        if (inGroundedState) return;

        bool shouldntRebound = (
            (position.y < enemyObj.transform.position.y) ||
            (velocity.y > 0)
        );

        Vector3 velocityTemp = velocity;
        if (shouldntRebound) {
            velocityTemp.y -= Mathf.Sign(velocityTemp.y) * physicsScale;
        } else {
            velocityTemp.y *= -1;
        }
        velocity = velocityTemp;
    }

    // ========================================================================

    // I would use stateCurrent.In but that creates garbage
    public bool inIgnoreState { get {
        switch (stateCurrent) {
            case CharacterState.dying:
            case CharacterState.drowning:
            case CharacterState.dead:
            case CharacterState.hurt:
                return true;
            default:
                return false;
        }
    }}

    public bool inDyingState { get {
        switch (stateCurrent) {
            case CharacterState.dying:
            case CharacterState.drowning:
                return true;
            default:
                return false;
        }
    }}

    public bool inDeadState { get {
        switch (stateCurrent) {
            case CharacterState.dying:
            case CharacterState.drowning:
            case CharacterState.dead:
                return true;
            default:
                return false;
        }
    }}

    public bool inGroundedState { get {
        switch (stateCurrent) {
            case CharacterState.ground:
            case CharacterState.rolling:
            case CharacterState.spindash:
                return true;
            default:
                return false;
        }
    }}

    public bool inRollingAirState { get {
        switch (stateCurrent) {
            case CharacterState.rollingAir:
            case CharacterState.jump:
                return true;
            default:
                return false;
        }
    }}

    public bool inRollingState { get {
        switch (stateCurrent) {
            case CharacterState.rolling:
            case CharacterState.rollingAir:
            case CharacterState.jump:
            case CharacterState.spindash:
                return true;
            default:
                return false;
        }
    }}

    // ========================================================================

    public bool isInvulnerable { get {
        return (
            inIgnoreState ||
            (invulnTimer > 0) ||
            (invincibilityTimer > 0)
        );
    } }

    public bool isHarmful { get {
        return(
            inRollingState ||
            (invincibilityTimer > 0)
        );
    }}

    // ========================================================================

    public float opacity {
        get { return spriteRenderer.color.a; }
        set {
            Color colorTemp = spriteRenderer.color;
            colorTemp.a = value;
            spriteRenderer.color = colorTemp;
        }
    }


    // 3D-Ready: YES
    void UpdateInvulnerable() {
        if (invulnTimer <= 0) return;

        invulnTimer -= deltaTime;
        if (invulnTimer <= 0) {
            opacity = 1;
        } else {
            int frame = (int)Mathf.Round(invulnTimer * 60);
            opacity = (frame % 8) > 3 ? 1 : 0;
        }
    }

    // 3D-Ready: YES
    void UpdateSpritePosition() {
        spriteObject.transform.position = position;
    }

    public ObjShield shield;

    // 3D-Ready: YES

    public void RemoveShield() {
        if (shield == null) return;
        Destroy(shield.gameObject);
        shield = null;
    }

    public Level currentLevel;

    // ========================================================================

    public Vector2 positionMin = new Vector2(
        -Mathf.Infinity,
        -Mathf.Infinity
    );
    public Vector2 positionMax = new Vector2(
        Mathf.Infinity,
        Mathf.Infinity
    );

    void UpdatePositionBounds() {
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

    void UpdateVictory() {
        if (!victoryLock) {
            if (stateCurrent == CharacterState.victory)
                stateCurrent = CharacterState.ground;
        } else {
            facingRight = true;
            stateCurrent = CharacterState.victory;
            spriteAnimator.Play("Victory");
            spriteAnimator.speed = 1;
            rigidbody.velocity = Vector3.zero;
            groundSpeed = 0;
        }

    }

}