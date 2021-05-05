using UnityEngine;

public class FollowObject : MonoBehaviour {
    Transform target = null;
    bool followPosition = true;
    bool followRotation = true;
    bool followLocalScale = true;
    public void LateUpdate() {
        if (target == null) return;
        if (followPosition) transform.position = target.position;
        if (followRotation) transform.rotation = target.rotation;
        if (followLocalScale) transform.localScale = target.localScale;
    }
}