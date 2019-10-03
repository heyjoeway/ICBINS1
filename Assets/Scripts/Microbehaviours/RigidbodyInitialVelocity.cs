using UnityEngine;

public class RigidbodyInitialVelocity : MonoBehaviour {
    public Vector3 intialVelocity;

    void Start() {
        GetComponent<Rigidbody>().velocity = intialVelocity;
        Destroy(this);
    }
}
