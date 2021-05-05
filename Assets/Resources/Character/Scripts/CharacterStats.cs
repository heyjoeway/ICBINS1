using UnityEngine;
using System.Collections.Generic;
using System;

public class CharacterStats {
    public float physicsScale = 1;
    Dictionary<string, object> stats = new Dictionary<string, object>();

    public bool ContainsKey(string key) => stats.ContainsKey(key);
    public float Get(string key) => GetRaw(key) * physicsScale;
    public float GetRaw(string key) {
        object val = stats[key];
        if (val is float)
            return (float)val;
        if (val is bool)
            return (bool)val ? 1 : 0;
        if (val is string) 
            return GetRaw((string)val);
        if (val is Func<float>) 
            return ((Func<float>)val).Invoke();
        if (val is Func<string>) 
            return GetRaw(((Func<string>)val).Invoke());

        return -Mathf.Infinity; // Should have a better fail case
    }
    public void Add(string key, object val) => stats[key] = val;
    public void Add(Dictionary<string, object> data) {
        foreach (string key in data.Keys)
            stats[key] = data[key];
    }
}