using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class ExtensionMethods {
    /// <summary>
    /// Rounds Vector3.
    /// </summary>
    /// <param name="vector3"></param>
    /// <param name="decimalPlaces"></param>
    /// <returns></returns>
    public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2) {
        float multiplier = 1;
        for (int i = 0; i < decimalPlaces; i++) {
            multiplier *= 10f;
        }
        return new Vector3(
            Mathf.Round(vector3.x * multiplier) / multiplier,
            Mathf.Round(vector3.y * multiplier) / multiplier,
            Mathf.Round(vector3.z * multiplier) / multiplier);
    }

    public static float AngleTowards(this Vector3 from, Vector3 to) {
        return ((Vector2)from).AngleTowards((Vector2)to);
    }

    public static float AngleTowards(this Vector2 from, Vector2 to) {
        return Mathf.Atan2(
            to.y - from.y,
            to.x - from.x
        ) * Mathf.Rad2Deg;
    }

    public static Vector2 RadianToVector2(this float radian) {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }
    
    public static Vector2 DegreeToVector2(this float degree) {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public static bool In<T>(this T t,params T[] values){
        foreach (T value in values) {
            if (t.Equals(value)) {
                return true;
            }
        }
        return false;
    }
}