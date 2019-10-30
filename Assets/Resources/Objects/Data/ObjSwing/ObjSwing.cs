using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjSwing : MonoBehaviour {
    public float cycleTime = 4F;
    float timer = 0;

    void Update() {
        timer += Utils.cappedDeltaTime;
        transform.eulerAngles = new Vector3(
            0, 0,
            Mathf.Sin((timer / cycleTime) * 2 * Mathf.PI) * 90F
        );
        foreach (Transform child in transform)
            child.eulerAngles = Vector3.zero;
    }
}
