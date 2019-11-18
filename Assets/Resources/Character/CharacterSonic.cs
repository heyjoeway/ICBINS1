using UnityEngine;

public class CharacterSonic : Character {
    public override void Start() {
        capabilities.Add(new CharacterCapabilityGround(this));
        capabilities.Add(new CharacterCapabilityAir(this));
        capabilities.Add(new CharacterCapabilityHurt(this));
    
        if (GlobalOptions.GetBool("spindash"))
            capabilities.Add(new CharacterCapabilitySpindash(this));

        if (GlobalOptions.GetBool("peelOut"))
            capabilities.Add(new CharacterCapabilityPeelOut(this));
    
        if (GlobalOptions.GetBool("dropDash"))
            capabilities.Add(new CharacterCapabilityDropdash(this));

        if (GlobalOptions.GetBool("homingAttack"))
            capabilities.Add(new CharacterCapabilityHomingAttack(this));
    
    if (GlobalOptions.GetBool("lightDash"))
            capabilities.Add(new CharacterCapabilityLightDash(this));
            
        capabilities.Add(new CharacterCapabilityJump(this));
        capabilities.Add(new CharacterCapabilityRolling(this));
        capabilities.Add(new CharacterCapabilityRollingAir(this));
        capabilities.Add(new CharacterCapabilityVictory(this));
        capabilities.Add(new CharacterCapabilityDeath(this));

        if (GlobalOptions.GetBool("afterImages"))
            capabilities.Add(new CharacterCapabilityAfterImage(this));
            
        base.Start();
    }
}