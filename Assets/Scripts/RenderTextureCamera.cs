using UnityEngine;
using System.Collections;

public class RenderTextureCamera : MonoBehaviour {
    public Rect screenRectRelative = new Rect(0,0,1,1);
    Rect screenRectRelativePrev;
    Rect screenRect;
    bool integerScaling;

    public Rect GetScreenRect() {
        if (integerScaling) {
            float ratio = (
                (screenRectRelative.width * Screen.width) /
                (screenRectRelative.height * Screen.height)
            );
            int intScaleFac = (int)((screenRectRelative.height * Screen.height) / camera.targetTexture.height);

            int height = camera.targetTexture.height * intScaleFac;
            int width = (int)(height * ratio);
            int xOffset = (Screen.width - width) / 2;
            int yOffset = (Screen.height - height) / 2;

            return new Rect(
                (screenRectRelative.x * Screen.width) +
                (screenRectRelative.width * xOffset),
                (screenRectRelative.y * Screen.height) +
                (screenRectRelative.height * yOffset),
                screenRectRelative.width * width,
                screenRectRelative.height * height
            );
        }

        return new Rect(
            screenRectRelative.x * Screen.width,
            screenRectRelative.y * Screen.height,
            screenRectRelative.width * Screen.width,
            screenRectRelative.height * Screen.height
        );
    }

    new Camera camera;
    void Awake() {
        camera = GetComponent<Camera>();
    }

    void Update() {
        integerScaling = GlobalOptions.GetBool("integerScaling");

        if (LevelManager.current != null)
            integerScaling &= LevelManager.current.characters.Count <= 1;
    }

    int renderTextureWidthPev; // Hack

    public void ResizeRenderTexture() {
        if (camera.targetTexture == null) return;
        Rect screenRectNew = GetScreenRect();
        if (
            (screenRectNew == screenRect) &&
            (renderTextureWidthPev == camera.targetTexture.width)
        ) return;
        screenRect = screenRectNew;

        camera.targetTexture.Release();
        camera.targetTexture.width = (int)Mathf.Round(
            (float)camera.targetTexture.height * (
                ((float)screenRect.width) /
                ((float)screenRect.height)
            )
        );
        renderTextureWidthPev = camera.targetTexture.width;

        Rect viewportRect = new Rect(
            0,0,
            camera.targetTexture.width,
            camera.targetTexture.height
        );
        camera.rect = viewportRect;
    }

    static bool clearScreen = true;
    void OnPreRender() {
        ResizeRenderTexture();
        clearScreen = true;
    }

    void OnGUI() {}
    IEnumerator OnPostRender() {
        yield return new WaitForEndOfFrame();
        if (clearScreen) {
            GL.Clear(false, true, Color.black);
            clearScreen = false;
        }
        Graphics.DrawTexture(screenRect, camera.targetTexture);
    }
}