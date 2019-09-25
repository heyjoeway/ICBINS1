using UnityEngine;

public class CharacterCapabilitySpindash : CharacterCapability {
    float spindashPower;
    float spindashPowerMax { get {
        return 8F * character.physicsScale;
    }}
    float spindashSpeedMax { get {
        return 12F * character.physicsScale;
    }}

    // ========================================================================

    Transform dustLocation;
    GameObject dust = null;


    // ========================================================================

    public CharacterCapabilitySpindash(Character character) : base(character) { }

    public override void Init() {
        name = "spindash";
        character.AddStateGroup("rolling", "spindash");
        character.AddStateGroup("ground", "spindash");
        character.AddStateGroup("harmful", "spindash");

        dustLocation = character.spriteContainer.Find("Spindash Dust Position");
    }

    public override void StateInit(string stateName, string prevStateName) {
        if (character.stateCurrent != name) return;

        dust = GameObject.Instantiate(
            (GameObject)Resources.Load("Objects/Dash Dust (Spindash)"),
            dustLocation.position,
            Quaternion.identity
        );
        UpdateSpindashDust();

        spindashPower = -2F;
        SpindashCharge();
        character.modeGroupCurrent = character.rollingModeGroup;
    }

    public override void StateDeinit(string stateName, string nextStateName) {
        if (character.stateCurrent != name) return;

        if (dust != null) {
            dust.GetComponent<Animator>().Play("Spindash End");
            dust = null;
        }
        
        character.groundedDetectorCurrent = null;
    }

    // See: https://info.sonicretro.org/SPG:Special_Abilities#Spindash_.28Sonic_2.2C_3.2C_.26_K.29
    public override void Update(float deltaTime) {
        if (character.stateCurrent == "ground") {
            // Switches the character to spindash state if connditions are met:
            // - Spindash enabled
            // - Pressing spindash key combo
            // - Standing still
            if (character.controlLock) return;
            if (!Input.GetKey(KeyCode.DownArrow)) return;
            if (!InputCustom.GetKeyDownPreventRepeat(KeyCode.D)) return;
            if (character.groundSpeed != 0) return;
            character.stateCurrent = name;
            return;
        } else if (character.stateCurrent != name) return;

        character.GroundSnap();
        UpdateSpindashDrain(deltaTime);
        UpdateSpindashInput();
        UpdateSpindashAnim(deltaTime);
        UpdateSpindashDust();
    }

    // 3D-Ready: YES
    void UpdateSpindashDust() {
        if (dust == null) return;
        dust.transform.position = dustLocation.position;
        dust.transform.localScale = character.spriteContainer.transform.localScale;
    }

    // 3D-Ready: YES
    void UpdateSpindashInput() {
        if (!Input.GetKey(KeyCode.DownArrow) || character.controlLock) {
            SpindashRelease();
            return;
        }

        if (InputCustom.GetKeyDownPreventRepeat(KeyCode.D) && !character.controlLock)
            SpindashCharge();
    }

    // 3D-Ready: YES
    void UpdateSpindashDrain(float deltaTime) {
        spindashPower -= ((spindashPower / 0.125F) / 256F) * deltaTime * 60F;
        spindashPower = Mathf.Max(0, spindashPower);
    }

    // 3D-Ready: YES
    void SpindashCharge() {
        character.spriteAnimator.Play("Spindash", 0, 0);
        spindashPower += 2;
        SFX.Play(character.audioSource, "SFX/Sonic 2/S2_60",
            1 + ((spindashPower / (spindashPowerMax + 2)) * 0.5F)
        );
    }

    // 3D-Ready: YES
    void SpindashRelease() {
        character.groundSpeed = (
            (character.facingRight ? 1 : -1) * 
            (8F + (Mathf.Floor(spindashPower) / 2F)) *
            character.physicsScale
        );
        character.characterCamera.lagTimer = 0.26667F;
        SFX.Play(character.audioSource, "SFX/Sonic 1/S1_BC");
        character.stateCurrent = "rolling";
    }

    // 3D-Ready: YES
    void UpdateSpindashAnim(float deltaTime) {
        character.spriteContainer.transform.position = character.position;
        character.spriteContainer.transform.eulerAngles = character.GetSpriteRotation(deltaTime);
        character.flipX = !character.facingRight;
    }
}