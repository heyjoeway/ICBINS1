using UnityEngine;
using System.Collections.Generic;

public static class InputCustom {
    public static bool preventRepeatLock = false;
    public static bool GetKeyDownPreventRepeat(KeyCode key) {
        if (preventRepeatLock) return false;
        return Input.GetKeyDown(key);
    }
}