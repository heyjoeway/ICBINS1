using UnityEngine;
using System.Collections.Generic;

public class CharacterSonic : Character {
    public override void Start() {
        // capabilities.Add(new CharacterCapabilityGround(this));
        // capabilities.Add(new CharacterCapabilityAir(this));
        // capabilities.Add(new CharacterCapabilityHurt(this));
    
        // if (GlobalOptions.GetBool("spindash"))
        //     capabilities.Add(new CharacterCapabilitySpindash(this));

        // if (GlobalOptions.GetBool("peelOut"))
        //     capabilities.Add(new CharacterCapabilityPeelOut(this));
    
        // if (GlobalOptions.GetBool("dropDash"))
        //     capabilities.Add(new CharacterCapabilityDropdash(this));

        // if (GlobalOptions.GetBool("homingAttack"))
        //     capabilities.Add(new CharacterCapabilityHomingAttack(this, true, true));
    
        // if (GlobalOptions.GetBool("lightDash"))
        //     capabilities.Add(new CharacterCapabilityLightDash(this));
            
        // capabilities.Add(new CharacterCapabilityJump(this));
        // capabilities.Add(new CharacterCapabilityRolling(this));
        // capabilities.Add(new CharacterCapabilityRollingAir(this));
        // capabilities.Add(new CharacterCapabilityVictory(this));
        // capabilities.Add(new CharacterCapabilityDeath(this));

        // // if (GlobalOptions.GetBool("afterImages"))
        // capabilities.Add(new CharacterCapabilityAfterImage3D(this));
        // capabilities.Add(new CharacterCapabilityBoostMode(this));
        // capabilities.Add(new CharacterCapabilityBoost(this));
            
        // this.stats.Add(new Dictionary<string, object>() {
        //     ["homingAttackSpeed"] = 6F, // Guessed (fix)
        //     ["jumpSpeed"] = 6F, // Found from video
        //     ["horizontalInputLockTime"] = 0.5F, // seconds
        //     ["slopeFactorGround"] = 0.1F,
        //     ["slopeFactorHInputLock"] = 0.5F,
        //     ["frictionGroundNormal"] = 0.046875F * 4,
        //     ["frictionGroundSpeedUp"] = 0.09375F * 4,
        //     ["decelerationGround"] = 0.5F,
        //     ["accelerationGroundNormal"] = 0.046875F * 2F,
        //     ["accelerationGroundSpeedUp"] = 0.046875F * 2F * 1.5F,
        //     ["topSpeedNormal"] = 8F,
        //     ["topSpeedSpeedUp"] = 12F,
        //     ["accelerationAirNormal"] = 0.09375F * 2F,
        //     ["accelerationAirSpeedUp"] = 0.09375F * 2F,
        //     ["airDragThreshold"] = 0,
        //     ["frictionAirNormal"] = 0.046875F * 6,
        //     ["frictionAirSpeedUp"] = 0.09375F * 6,
        //     ["decelerationAirNormal"] = 0.09375F * 6,
        //     ["decelerationAirSpeedUp"] = 0.1875F * 6,
        //     ["smoothRotationThreshold"] = 45,
        //     ["fallThreshold"] = 3.5F,
        //     ["rollRotate"] = true
        // });

        base.Start();
    }
}