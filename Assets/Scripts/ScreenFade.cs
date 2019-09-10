using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour {
    new Image renderer;
    Material material;

    // Start is called before the first frame update
    void Start() {
        renderer = transform.Find("Image").GetComponent<Image>();
        material = Instantiate(renderer.material);
        renderer.material = material;
        Update();
    }

    public float fadeDelay = 5F;
    public float fadeSpeed = 0F; // percent per second
    public float destroyDelay = 2F / 60F;
    public bool destroyWhenDone = true;
    public Action onComplete;

    public float redOffset = -0.125F;
    public float blueOffset = 0;
    public float greenOffset = -0.125F;
    public float brightness = 0F;
    public float brightnessMin { get {
        return Mathf.Max(redOffset, blueOffset, greenOffset);
    }}
    public float brightnessMax { get {
        return 1 - Mathf.Min(redOffset, blueOffset, greenOffset);
    }}

    bool isComplete = false;
    void Complete() {
        if (onComplete != null) {
            onComplete();
            onComplete = null;
        }
        isComplete = true;
    }

    void Update() {
        if (isComplete && destroyWhenDone && (destroyDelay > 0)) {
            destroyDelay -= Time.deltaTime;
            if (destroyDelay <= 0) {
                Destroy(gameObject);
                return;
            }
        }

        material.SetFloat("_Brightness", brightness);
        material.SetFloat("_RedOffset", redOffset);
        material.SetFloat("_GreenOffset", greenOffset);
        material.SetFloat("_BlueOffset", blueOffset);

        if (fadeDelay > 0) {
            fadeDelay -= Time.deltaTime;
            return;
        }

        if (fadeSpeed == 0) return;

        brightness += fadeSpeed * Time.deltaTime;
        if ((fadeSpeed > 0) && (brightness >= brightnessMax)) Complete();
        if ((fadeSpeed < 0) && (brightness <= brightnessMin)) Complete();
    }
}
