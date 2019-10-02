using UnityEngine;

public class CharacterSonic : Character {
    public override void Start() {
        capabilities.Add(new CharacterCapabilityGround(this));
        capabilities.Add(new CharacterCapabilityAir(this));
        capabilities.Add(new CharacterCapabilityHurt(this));
    
        if (GlobalOptions.Get<bool>("spindash"))
            capabilities.Add(new CharacterCapabilitySpindash(this));

        if (GlobalOptions.Get<bool>("peelOut"))
            capabilities.Add(new CharacterCapabilityPeelOut(this));
    
        if (GlobalOptions.Get<bool>("dropDash"))
            capabilities.Add(new CharacterCapabilityDropdash(this));

        if (GlobalOptions.Get<bool>("homingAttack"))
            capabilities.Add(new CharacterCapabilityHomingAttack(this));
    
        if (GlobalOptions.Get<bool>("lightDash"))
            capabilities.Add(new CharacterCapabilityLightDash(this));
            
        capabilities.Add(new CharacterCapabilityJump(this));
        capabilities.Add(new CharacterCapabilityRolling(this));
        capabilities.Add(new CharacterCapabilityRollingAir(this));
        capabilities.Add(new CharacterCapabilityVictory(this));
        capabilities.Add(new CharacterCapabilityDeath(this));

        if (GlobalOptions.Get<bool>("afterImages"))
            capabilities.Add(new CharacterCapabilityAfterImage(this));
            
        base.Start();
    }
}