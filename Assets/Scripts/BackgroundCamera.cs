using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundCamera : MonoBehaviour {
    public new Camera camera;
    public RenderTexture renderTexture;
    Material material;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<Camera>();
    }

    void OnPreRender() {
        camera.targetTexture = renderTexture;
    }

     void OnPostRender() {
        camera.targetTexture = null;
        Graphics.Blit(
            renderTexture,
            null as RenderTexture
        );
    }
}
