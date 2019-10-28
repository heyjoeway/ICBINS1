using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour {
    new Image renderer;
    Material material;
    public Canvas canvas;

    void Awake() {
        renderer = transform.Find("Image").GetComponent<Image>();
        material = Instantiate(renderer.material);
        renderer.material = material;
        canvas = GetComponent<Canvas>();
    }

    // Start is called before the first frame update
    void Start() { Update(); }

    public float fadeDelay = 5F;
    public float fadeSpeed = 0F; // percent per second
    public float destroyDelay = 10F / 60F;
    public bool destroyWhenDone = false;
    public Action onComplete;

    public float redOffset = -0.125F;
    public float blueOffset = 0;
    public float greenOffset = -0.125F;
    public float brightness = 0F;
    public bool stopTime = false;
    public float brightnessMin { get {
        return Mathf.Max(redOffset, blueOffset, greenOffset);
    }}
    public float brightnessMax { get {
        return 1 - Mathf.Min(redOffset, blueOffset, greenOffset);
    }}

    bool isComplete = false;
    void Complete() {
        if (isComplete) return;
        if (onComplete != null) {
            onComplete();
            onComplete = null;
        }
        if (stopTime) Time.timeScale = 0;
        isComplete = true;
    }

    public void Update() {
        if ((LevelManager.current != null) && (LevelManager.current.characters.Count != 1))
            stopTime = false;

        if (canvas.worldCamera == null) {
            CharacterCamera characterCamera = FindObjectOfType<CharacterCamera>();
            if (characterCamera != null) canvas.worldCamera = characterCamera.camera;
            else canvas.worldCamera = FindObjectOfType<Camera>();
        }

        if (stopTime) Time.timeScale = 0;

        if (isComplete && destroyWhenDone && (destroyDelay > 0)) {
            destroyDelay -= Utils.cappedUnscaledDeltaTime;
            if (destroyDelay <= 0) {
                Complete();
                Destroy(gameObject);
                return;
            }
        }

        material.SetFloat("_Brightness", brightness);
        material.SetFloat("_RedOffset", redOffset);
        material.SetFloat("_GreenOffset", greenOffset);
        material.SetFloat("_BlueOffset", blueOffset);

        if (fadeDelay > 0) {
            fadeDelay -= Utils.cappedUnscaledDeltaTime;
            return;
        }

        if (fadeSpeed == 0) return;

        brightness += fadeSpeed * Utils.cappedUnscaledDeltaTime;
        if ((fadeSpeed > 0) && (brightness >= brightnessMax)) Complete();
        if ((fadeSpeed < 0) && (brightness <= brightnessMin)) Complete();
    }

    void OnDestroy() {
        if (stopTime) Time.timeScale = 1;
    }
}
