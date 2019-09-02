using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjLevelClear : MonoBehaviour {
    Animator animator;
    AudioSource audioSource;
    Canvas canvas;

    void InitReferences() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        canvas = GetComponent<Canvas>();
    }

    void Start() { InitReferences(); }

    public SceneReference sceneReference;

    public Character character;
    float showTimer = 2F;
    float tallyTimer = 4F;
    float tallyFrameTimer = 0;
    float endTimer = 3F;
    // Start is called before the first frame update

    int GetTimeBonus(float time) {
        if (time < 30) return 50000;
        if (time < 45) return 10000;
        if (time < 60) return 5000;
        if (time < 90) return 4000;
        if (time < 120) return 3000;
        if (time < 180) return 2000;
        if (time < 240) return 1000;
        if (time < 300) return 500;
        if (time > 24 * 60 * 60) return 1; // lol
        return 0;
    }

    int timeBonus;
    int ringBonus;

    // Update is called once per frame
    void Update() {
        Debug.Log(character.characterPackage.camera.camera);
        canvas.worldCamera = character.characterPackage.camera.camera; // i hate this name too, trust me

        if (showTimer > 0) {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0) {
                animator.Play("Items Enter");
                timeBonus = GetTimeBonus(character.timer);
                ringBonus = character.rings * 100;
            }
            return;
        }

        if (tallyTimer > 0) {
            tallyTimer -= Time.deltaTime;
            return;
        }


        if ((timeBonus > 0) || (ringBonus > 0)) {
            if (tallyFrameTimer > 0) {
                tallyFrameTimer -= Time.deltaTime;
                if (tallyFrameTimer <= 0)
                    tallyFrameTimer = 1F / 60F;
                else return;
            }

            int transferAmtTime = Mathf.Min(100, timeBonus);
            int transferAmtRing = Mathf.Min(100, ringBonus);

            timeBonus -= transferAmtTime;
            ringBonus -= transferAmtRing;

            character.score += transferAmtTime;
            character.score += transferAmtRing;
            SFX.PlayOneShot(audioSource, "SFX/Sonic 1/S1_CD");

            if ((timeBonus <= 0) && (ringBonus <= 0)) {
                SFX.PlayOneShot(audioSource, "SFX/Sonic 1/S1_C5");
                animator.Play("Items Exit");
            }
            return;
        }

        if (endTimer > 0) {
            endTimer -= Time.deltaTime;
            return;
        }

        StartCoroutine(Utils.LoadLevelAsync(
            sceneReference.ScenePath,
            character,
            () => {
                character.timer = 0;
                character.rings = 0;
                Destroy(gameObject);
            }
        ));

    }
}
