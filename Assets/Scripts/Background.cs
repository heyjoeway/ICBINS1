using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {
    static int backgroundCount = 0;
    public Camera camera;
    public Character character;

    void Awake() {
        camera = transform.Find("Camera").GetComponent<Camera>();

        transform.position = new Vector3(
            0, 0,
            (backgroundCount + 1) * -100 
        );
        backgroundCount++;
    }

    public Vector3 targetPosition;
    public Vector3 autoMoveSpeed;

    public virtual void Update() {
        targetPosition += autoMoveSpeed * Utils.cappedDeltaTime;

        if (character != null && character.characterCamera != null)
            targetPosition = character.characterCamera.position;

        if ((character != null) && (character.characterCamera != null))
            camera.rect = character.characterCamera.camera.rect;
    }
}
