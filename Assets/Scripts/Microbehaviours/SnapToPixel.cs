using UnityEngine;

public class SnapToPixel : MonoBehaviour {
    new Rigidbody rigidbody;
    public void Start() {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Update() {
        transform.position = (transform.position * 32).Round(0) / 32F;
        if (rigidbody != null) rigidbody.position = transform.position;
    }
}