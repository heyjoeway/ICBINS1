using UnityEngine;
using System.Collections.Generic;

public static class InputCustom {
    public static bool preventRepeatLock = false;

    public static bool GetKeys(params KeyCode[] keys) {
        foreach (KeyCode key in keys)
            if (Input.GetKey(key)) return true;
        return false;
    }

    public static bool GetKeysDown(params KeyCode[] keys) {
        foreach (KeyCode key in keys)
            if (Input.GetKeyDown(key)) return true;
        return false;
    }

    public static bool GetKeysDownPreventRepeat(params KeyCode[] keys) {
        if (preventRepeatLock) return false;
        return GetKeysDown(keys);
    }

    public static bool GetButtons(params string[] buttons) {
        foreach (string button in buttons)
            if (Input.GetButton(button)) return true;
        return false;
    }

    public static bool GetButtonsDown(params string[] buttons) {
        foreach (string button in buttons)
            if (Input.GetButtonDown(button)) return true;
        return false;
    }

    public static bool GetButtonsDownPreventRepeat(params string[] buttons) {
        if (preventRepeatLock) return false;
        return GetButtonsDown(buttons);
    }

    public static bool GetAxesPositive(params string[] axes) {
        foreach (string axis in axes)
            if (Input.GetAxis(axis) > 0) return true;
        return false;
    }

    public static bool GetAxesNegative(params string[] axes) {
        foreach (string axis in axes)
            if (Input.GetAxis(axis) < 0) return true;
        return false;
    }
}