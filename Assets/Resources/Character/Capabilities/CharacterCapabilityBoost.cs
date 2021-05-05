using UnityEngine;
using System.Collections.Generic;

public class CharacterCapabilityBoost : CharacterCapability {
    string[] buttonsBoost = new string[] { "Primary" };

    // ========================================================================

    public CharacterCapabilityBoost(Character character) : base(character) { }

    GameObject boostParticle;

    public override void Init() {
        character.stats.Add(new Dictionary<string, object>() {
            ["boostSpeed"] = 12F,
            ["boostLowerThreshold"] = 6F
        });

        boostParticle = GameObject.Instantiate(
            Constants.Get<GameObject>("prefabBoostEffect"),
            Vector3.zero,
            Quaternion.identity
        );
        boostParticle.SetActive(false);
    }

    CharacterEffect boostingEffect = null;

    void ExitBoost() {
        if (boostingEffect == null) return;
        boostingEffect.DestroyBase();
        boostingEffect = null;
        Debug.Log("what");
    }

    void EnterBoost() {
        if (boostingEffect != null) return;
        boostingEffect = new CharacterEffect(character, "boosting");
        character.effects.Add(boostingEffect);
        
        if (character.input.GetAxisPositive("Horizontal")) 
            character.facingRight = true;

        if (character.input.GetAxisNegative("Horizontal"))
            character.facingRight = false;

        character.groundSpeed = (
            character.stats.Get("boostSpeed") *
            (character.facingRight ? 1 : -1)
        );

        if (character.characterCamera != null)
            character.characterCamera.lagTimer = 0.26667F / 2;

        GameObject boostBurstParticle = GameObject.Instantiate(
            Constants.Get<GameObject>("prefabBoostBurstEffect"),
            character.position,
            character.rotation
        );
        boostBurstParticle.transform.localScale = new Vector3(
            character.facingRight ? 1 : -1,
            1,
            1
        );

        // SFX.PlayOneShot(character.audioSource, "sfxBoost");
    }

    public override void Update(float deltaTime) {
        bool buttonDown = character.input.GetButtons(buttonsBoost);
        bool buttonPressed = character.input.GetButtonsDownPreventRepeat(buttonsBoost);

        if (buttonPressed && character.InStateGroup("ground"))
            EnterBoost();

        if (!buttonDown)
            ExitBoost();

        if (Mathf.Abs(character.groundSpeed) < character.stats.Get("boostLowerThreshold"))
            ExitBoost();

        boostParticle.SetActive(character.HasEffect("boosting"));
        // boostParticle.transform.rotation = Quaternion.LookRotation(
        //     (character.position - boostParticle.transform.position).normalized
        // );
        boostParticle.transform.eulerAngles = new Vector3(
            0, 0,
            Mathf.Atan2(
                character.position.y - boostParticle.transform.position.y,
                character.position.x - boostParticle.transform.position.x
            ) * (180 / Mathf.PI)
        );
        boostParticle.transform.position = character.position;
        // boostParticle.transform.localScale = new Vector3(
        //     character.facingRight ? 1 : -1,
        //     1,
        //     1
        // );
    }
}