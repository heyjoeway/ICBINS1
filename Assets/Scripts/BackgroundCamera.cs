using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundCamera : MonoBehaviour {
    [HideInInspector]
    public new Camera camera;
    public RenderTexture renderTexture;
    Material material;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<Camera>();
    }

    Vector2 resolutionPrev;

    void ResizeRenderTexture() {
        if (
            (resolutionPrev.x == Screen.width) &&
            (resolutionPrev.y == Screen.height)
         ) return;
        resolutionPrev = new Vector2(Screen.width, Screen.height);
        
        renderTexture.Release();
        renderTexture.width = (int)Mathf.Round(
            (float)renderTexture.height * (
                ((float)Screen.width) /
                ((float)Screen.height)
            )
        );
        camera.orthographicSize = (renderTexture.height / 32) * 0.5F;
    }

    void OnPreRender() {
        ResizeRenderTexture();
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
