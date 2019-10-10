using UnityEngine;
using System.Collections.Generic;

public class InputCustom {
    public static bool preventRepeatLock = false;

    static string[] _TransformButtons(string[] buttons, int playerId = 1) {
        if (playerId == 1) return buttons;
        for(int i = 0; i < buttons.Length; i++)
            buttons[i] += " P" + playerId;
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

    public static bool GetButtons(int playerId, params string[] buttons) {
        buttons = _TransformButtons(buttons, playerId);
        foreach (string button in buttons)
            if (Input.GetButton(button)) return true;
        return false;
    }

    public static bool GetButtonsDown(int playerId, params string[] buttons) {
        buttons = _TransformButtons(buttons, playerId);
        foreach (string button in buttons)
            if (Input.GetButtonDown(button)) return true;
        return false;
    }

    public static bool GetButtonsDownPreventRepeat(int playerId, params string[] buttons) {
        buttons = _TransformButtons(buttons, playerId);
        if (preventRepeatLock) return false;
        return GetButtonsDown(playerId, buttons);
    }

    public static bool GetAxesPositive(int playerId, params string[] axes) {
        axes = _TransformButtons(axes, playerId);
        foreach (string axis in axes)
            if (Input.GetAxis(axis) > 0) return true;
        return false;
    }

    public static bool GetAxesNegative(int playerId, params string[] axes) {
        axes = _TransformButtons(axes, playerId);
        foreach (string axis in axes)
            if (Input.GetAxis(axis) < 0) return true;
        return false;
    }

    // ========================================================================

    public int playerId;
    public bool enabled;
    
    public InputCustom(int playerId = 1) {
        this.playerId = playerId;
    }

    public bool GetButtons(params string[] buttons) {
        if (!enabled) return false;
        return GetButtons(playerId, buttons);
    }

    public bool GetButtonsDown(params string[] buttons) {
        if (!enabled) return false;
        return GetButtonsDown(playerId, buttons);
    }

    public bool GetButtonsDownPreventRepeat(params string[] buttons) {
        if (!enabled) return false;
        return GetButtonsDownPreventRepeat(playerId, buttons);
    }

    public bool GetAxesPositive(params string[] axes) {
        if (!enabled) return false;
        return GetAxesPositive(playerId, axes);
    }

    public bool GetAxesNegative(params string[] axes) {
        if (!enabled) return false;
        return GetAxesNegative(playerId, axes);
    }

}