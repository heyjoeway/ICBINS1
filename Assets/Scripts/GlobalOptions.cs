using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalOptions : MonoBehaviour {
    static Dictionary<string, string> options = new Dictionary<string, string> {
        ["dropDash"] = "ON",
        ["spindash"] = "ON",
        ["levelTransitions"] = "ON",
        ["smoothRotation"] = "ON",
        ["afterImages"] = "ON",
        ["linearInterpolation"] = "ON",
        ["centisecondTimer"] = "ON",
    };

    public static string Get(string key) {
        return options[key];
    }

    public static T Get<T>(string key) {
        string data = options[key];
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
        options[key] = value;
    }

    // Start is called before the first frame update
    void Start() {
        if (dropdown != null) {
            for (int i = 0; i < dropdown.options.Count; i++) {
                if (dropdown.options[i].text == options[key]) {
                    dropdown.value = i;
                    break;
                }
            }
        }
    }

    public string key;
    public Dropdown dropdown;
    public void Set() {
        if (dropdown != null) {
            options[key] = dropdown.options[dropdown.value].text;
            return;
        }
    }
}
