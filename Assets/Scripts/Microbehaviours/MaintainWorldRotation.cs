using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainWorldRotation : MonoBehaviour
{
    Quaternion rotationInitial;
    // Start is called before the first frame update
    void Start() {
        rotationInitial = transform.rotation;
    }

    // Update is called once per frame
    void Update() {
        // Debug.Log(transform.rotation);
        transform.rotation = rotationInitial;
        // Debug.Log(transform.rotation);
    }
}
