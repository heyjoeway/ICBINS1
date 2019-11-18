using UnityEngine;
using System.Collections.Generic;

public class InputCustom {
    // ========================================================================
    // ========================================================================
    // Static
    // ========================================================================    
    // ========================================================================

    public static bool preventRepeatLock = false;

    static Dictionary<int, Dictionary<string, string>> _TransformButtonCache = new Dictionary<int, Dictionary<string, string>>();

    static string _TransformButton(string button, int controllerId = 1) {
        if (!_TransformButtonCache.ContainsKey(controllerId))
            _TransformButtonCache.Add(controllerId, new Dictionary<string, string>());

        if (!_TransformButtonCache[controllerId].ContainsKey(button)) {
            string buttonResult = button;
            if (controllerId > 1) buttonResult += " P" + controllerId;
            _TransformButtonCache[controllerId].Add(button, buttonResult);
        }

        return _TransformButtonCache[controllerId][button];
    }

    // ========================================================================

    public static bool GetKeys(KeyCode[] keys) {
        foreach (KeyCode key in keys)
            if (Input.GetKey(key)) return true;
        return false;
    }

    public static bool GetKeysDown(KeyCode[] keys) {
        foreach (KeyCode key in keys)
            if (Input.GetKeyDown(key)) return true;
        return false;
    }

    public static bool GetKeysDownPreventRepeat(KeyCode[] keys) {
        if (preventRepeatLock) return false;
        return GetKeysDown(keys);
    }

    public static bool GetButtons(int controllerId, string[] buttons) {
        foreach (string button in buttons) {
            string buttonPlayer = _TransformButton(button, controllerId);
            if (Input.GetButton(buttonPlayer)) return true;
        }
        return false;
    }

    public static bool GetButtonsDown(int controllerId, string[] buttons) {
        foreach (string button in buttons) {
            string buttonPlayer = _TransformButton(button, controllerId);
            if (Input.GetButtonDown(buttonPlayer)) return true;
        }
        return false;
    }

    public static bool GetButtonsDownPreventRepeat(int controllerId, string[] buttons) {
        if (preventRepeatLock) return false;
        return GetButtonsDown(controllerId, buttons);
    }

    public static bool GetAxesPositive(int controllerId, string[] axes) {
        foreach (string axis in axes) {
            string axisPlayer = _TransformButton(axis, controllerId);
            if (Input.GetAxis(axisPlayer) > 0) return true;
        }
        return false;
    }

    public static bool GetAxesNegative(int controllerId, string[] axes) {
        foreach (string axis in axes) {
            string axisPlayer = _TransformButton(axis, controllerId);
            if (Input.GetAxis(axisPlayer) < 0) return true;
        }
        return false;
    }

    // ========================================================================
    // Singular
    // ========================================================================

    public static bool GetButton(int controllerId, string button) {
        string buttonPlayer = _TransformButton(button, controllerId);
        return Input.GetButton(buttonPlayer);
    }

    public static bool GetButtonDown(int controllerId, string button) {
        string buttonPlayer = _TransformButton(button, controllerId);
        return Input.GetButtonDown(buttonPlayer);
    }

    public static bool GetButtonDownPreventRepeat(int controllerId, string button) {
        if (preventRepeatLock) return false;
        return GetButtonDown(controllerId, button);
    }

    public static bool GetAxisPositive(int controllerId, string axis) {
        axis = _TransformButton(axis, controllerId);
        return Input.GetAxis(axis) > 0;
    }

    public static bool GetAxisNegative(int controllerId, string axis) {
        axis = _TransformButton(axis, controllerId);
        return Input.GetAxis(axis) < 0;
    }

    // ========================================================================
    // ========================================================================
    // Non-Static
    // ========================================================================
    // ========================================================================

    public int controllerId;
    public bool enabled;
    
    public InputCustom(int controllerId = 1) {
        this.controllerId = controllerId;
    }

    // ========================================================================
    // Plural
    // ========================================================================

    // Q. "Why not use params?"
    // A. Garbage collection. Go bug Microsoft about it.
    // https://github.com/dotnet/csharplang/issues/179

    public bool GetButtons(string[] buttons) {
        if (!enabled) return false;
        return GetButtons(controllerId, buttons);
    }

    public bool GetButtonsDown(string[] buttons) {
        if (!enabled) return false;
        return GetButtonsDown(controllerId, buttons);
    }

    public bool GetButtonsDownPreventRepeat(string[] buttons) {
        if (!enabled) return false;
        return GetButtonsDownPreventRepeat(controllerId, buttons);
    }

    public bool GetAxesPositive(string[] axes) {
        if (!enabled) return false;
        return GetAxesPositive(controllerId, axes);
    }

    public bool GetAxesNegative(string[] axes) {
        if (!enabled) return false;
        return GetAxesNegative(controllerId, axes);
    }

    // ========================================================================
    // Singular
    // ========================================================================

    public bool GetButton(string buttons) {
        if (!enabled) return false;
        return GetButton(controllerId, buttons);
    }

    public bool GetButtonDown(string buttons) {
        if (!enabled) return false;
        return GetButtonDown(controllerId, buttons);
    }

    public bool GetButtonDownPreventRepeat(string buttons) {
        if (!enabled) return false;
        return GetButtonDownPreventRepeat(controllerId, buttons);
    }

    public bool GetAxisPositive(string axis) {
        if (!enabled) return false;
        return GetAxisPositive(controllerId, axis);
    }

    public bool GetAxisNegative(string axis) {
        if (!enabled) return false;
        return GetAxisNegative(controllerId, axis);
    }
}