using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalOptions : MonoBehaviour {
    static Dictionary<string, string> defaults = new Dictionary<string, string> {
        ["dropDash"] = "ON",
        ["spindash"] = "ON",
        ["levelTransitions"] = "ON",
        ["smoothRotation"] = "ON",
        ["afterImages"] = "ON",
        ["linearInterpolation"] = "ON",
        ["timerType"] = "NORMAL",
        ["peelOut"] = "ON",
        ["homingAttack"] = "OFF",
        ["lightDash"] = "OFF",
        ["airCurling"] = "OFF",
        ["gbaMode"] = "OFF",
        ["tinyMode"] = "OFF",
        ["integerScaling"] = "ON",
        ["timeLimit"] = "OFF", // TODO
        ["elementalShields"] = "OFF" // TODO
    };

    static Dictionary<string, string> playerPrefsCache = new Dictionary<string, string>();

    public static string Get(string key) {
        if (!playerPrefsCache.ContainsKey(key))
            playerPrefsCache[key] = PlayerPrefs.GetString(key, defaults[key]);
        
        return playerPrefsCache[key];
    }

    public static bool GetBool(string key) => Utils.StringBool(Get(key));

    static void Set(string key, string value) {
        playerPrefsCache[key] = value;
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    // Start is called before the first frame update
    void Start() {
        if (dropdown != null) {
            for (int i = 0; i < dropdown.options.Count; i++) {
                if (dropdown.options[i].text == Get(key)) {
                    dropdown.value = i;
                    break;
                }
            }
            dropdown.GetComponent<AudioSource>().Stop();
        }
    }

    public string key;
    public Dropdown dropdown;
    public void Set() {
        if (dropdown != null) {
            Set(key, dropdown.options[dropdown.value].text);
            return;
        }
    }
}
