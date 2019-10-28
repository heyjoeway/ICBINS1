using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants {
    static Dictionary<string, string> defaults = new Dictionary<string, string> {
        ["sfxDie"] = "SFX/Sonic 1/S1_A3",
        ["sfxDrown"] = "SFX/Sonic 1/S1_B2",
        ["sfxDropDashCharge"] = "SFX/Sonic 2/S2_60",
        ["sfxDropDashRelease"] = "SFX/Sonic 1/S1_BC",
        ["sfxSkid"] = "SFX/Sonic 1/S1_A4",
        ["sfxHomingAttack"] = "SFX/Megamix/Thok",
        ["sfxJump"] =  "SFX/Sonic 1/S1_A0",
        ["sfxLightDash"] = "SFX/Sonic 1/S1_BC",
        ["sfxPeelOutCharge"] = "SFX/Sonic CD/SCD_FM_11",
        ["sfxPeelOutRelease"] = "SFX/Sonic CD/SCD_FM_01",
        ["sfxRoll"] = "SFX/Sonic 1/S1_BE",
        ["sfxSpindashCharge"] = "SFX/Sonic 2/S2_60",
        ["sfxSpindashRelease"] = "SFX/Sonic 1/S1_BC",
        ["sfxHurt"] = "SFX/Sonic 1/S1_A3",
        ["sfxDieSpikes"] = "SFX/Sonic 1/S1_A6",
        ["sfxHurtRings"] = "SFX/Sonic 1/S1_C6",
        ["sfxTallyBeep"] = "SFX/Sonic 1/S1_CD",
        ["sfxTallyChaChing"] = "SFX/Sonic 1/S1_C5",
        ["sfxRingMonitor"] = "SFX/Sonic 1/S1_B5",
        ["sfxShieldNormal"] = "SFX/Sonic 1/S1_AF",

        ["prefabDropDashDust"] = "Objects/Dash Dust (Drop Dash)",
        ["prefabSpindashDust"] = "Objects/Dash Dust (Spindash)",
        ["prefabAnimal"] = "Objects/Animal",
        ["prefabExplosionEnemy"] = "Objects/Explosion (Enemy)",
        ["prefabPoints"] = "Objects/Points",
        ["prefabShieldNormal"] = "Objects/Shield (Normal)",
        ["prefabShieldFire"] = "Objects/Shield (Fire)",
        ["prefabShieldElectricity"] = "Objects/Shield (Electricity)",
        ["prefabShieldBubble"] = "Objects/Shield (Bubble)",
        ["prefabRing"] = "Objects/Ring",
        ["prefabLevelClear"] = "Objects/Level Clear",
        ["prefabScreenFadeOut"] = "Objects/Screen Fade Out",
        ["prefabTitleCard"] = "Objects/Title Card",
        ["prefabInvincibilityStars"] = "Objects/Invincibility Stars",
        ["prefabRingSparkle"] = "Objects/Ring Sparkle"
    };

    public static string Get(string key) => defaults[key];

    static Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    public static T Get<T>(string key) {
        string data = Get(key);

        if (typeof(T) == typeof(bool))
            return (T)(object)Utils.StringBool(data);

        if (typeof(T) == typeof(GameObject)) {
            if (!prefabCache.ContainsKey(data)) {
                prefabCache[data] = (GameObject)Resources.Load(data);
            }
            
            return (T)(object)prefabCache[data];
        }

        return (T)(object)data;
    }
}