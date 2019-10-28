using UnityEngine;
using System.Collections.Generic;

public class InputCustom {
    public static bool preventRepeatLock = false;

    static string[] _TransformButtons(string[] buttons, int controllerId = 1) {
        if (controllerId == 1) return buttons;
        for(int i = 0; i < buttons.Length; i++)
            buttons[i] += " P" + controllerId;
        return buttons;
    }

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

    public static bool GetButtons(int controllerId, params string[] buttons) {
        buttons = _TransformButtons(buttons, controllerId);
        foreach (string button in buttons)
            if (Input.GetButton(button)) return true;
        return false;
    }

    public static bool GetButtonsDown(int controllerId, params string[] buttons) {
        buttons = _TransformButtons(buttons, controllerId);
        foreach (string button in buttons)
            if (Input.GetButtonDown(button)) return true;
        return false;
    }

    public static bool GetButtonsDownPreventRepeat(int controllerId, params string[] buttons) {
        if (preventRepeatLock) return false;
        return GetButtonsDown(controllerId, buttons);
    }

    public static bool GetAxesPositive(int controllerId, params string[] axes) {
        axes = _TransformButtons(axes, controllerId);
        foreach (string axis in axes)
            if (Input.GetAxis(axis) > 0) return true;
        return false;
    }

    public static bool GetAxesNegative(int controllerId, params string[] axes) {
        axes = _TransformButtons(axes, controllerId);
        foreach (string axis in axes)
            if (Input.GetAxis(axis) < 0) return true;
        return false;
    }

    // ========================================================================

    public int controllerId;
    public bool enabled;
    
    public InputCustom(int controllerId = 1) {
        this.controllerId = controllerId;
    }

    public bool GetButtons(params string[] buttons) {
        if (!enabled) return false;
        return GetButtons(controllerId, buttons);
    }

    public bool GetButtonsDown(params string[] buttons) {
        if (!enabled) return false;
        return GetButtonsDown(controllerId, buttons);
    }

    public bool GetButtonsDownPreventRepeat(params string[] buttons) {
        if (!enabled) return false;
        return GetButtonsDownPreventRepeat(controllerId, buttons);
    }

    public bool GetAxesPositive(params string[] axes) {
        if (!enabled) return false;
        return GetAxesPositive(controllerId, axes);
    }

    public bool GetAxesNegative(params string[] axes) {
        if (!enabled) return false;
        return GetAxesNegative(controllerId, axes);
    }

}