using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalOptions : MonoBehaviour {
    static Dictionary<string, string> defaults = new Dictionary<string, string> {
        ["dropDash"] = "ON",
        ["spindash"] = "ON",
        ["levelTransitions"] = "ON",
        ["timeLimit"] = "OFF",
        ["smoothRotation"] = "ON",
        ["afterImages"] = "ON",
        ["linearInterpolation"] = "ON",
        ["timerType"] = "NORMAL",
        ["peelOut"] = "ON",
        ["homingAttack"] = "OFF",
        ["lightDash"] = "OFF",
        ["airCurling"] = "OFF"
    };

    public static string Get(string key) {
        return PlayerPrefs.GetString(key, defaults[key]);
    }

    public static T Get<T>(string key) {
        string data = Get(key);
        if (typeof(T) == typeof(bool)) {
            switch (data.ToLower()) {
                case "true":
                case "on":
                case "yes":
                case "y":
                case "t":
                case "ok":
                    return (T)(object)true;
                case "false":
                case "off":
                case "no":
                case "n":
                case "f":
                case "cancel":
                    return (T)(object)false;
                default:
                    return (T)(object)null;
            }
        }

        return (T)(object)data;
    }

    static void Set(string key, string value) {
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
