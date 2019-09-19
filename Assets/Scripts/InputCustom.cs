using UnityEngine;
using System.Collections.Generic;

public static class InputCustom {

    static HashSet<KeyCode> _keysDownPreventRepeat = new HashSet<KeyCode>();
    public static bool GetKeyDownPreventRepeat(KeyCode key) {
        if (_keysDownPreventRepeat.Contains(key)) return false;
        bool down = Input.GetKeyDown(key);
        if (down) _keysDownPreventRepeat.Add(key);
        return down;
    }

    public static void PreventRepeatReset() {
        _keysDownPreventRepeat.Clear();
    }
}