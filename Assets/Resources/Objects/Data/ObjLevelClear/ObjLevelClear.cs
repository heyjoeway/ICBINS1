using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ObjLevelClear : MonoBehaviour {
    Animator animator;
    AudioSource audioSource;
    Canvas canvas;
    Text scoreTextComponent;
    Text ringTextComponent;
    Text timeTextComponent;
    Text actTextComponent;

    void InitReferences() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        canvas = GetComponent<Canvas>();
        scoreTextComponent = transform.Find("Score Line/Score Value").GetComponent<Text>();
        ringTextComponent = transform.Find("Ring Line/Ring Bonus Value").GetComponent<Text>();
        timeTextComponent = transform.Find("Time Line/Time Bonus Value").GetComponent<Text>();
        actTextComponent = transform.Find("Act Group/Act Value").GetComponent<Text>();
    }

    void Start() { InitReferences(); }

    public SceneReference sceneReference;
    public UnityEvent onNextLevel;

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

    int timeBonus = 0;
    int ringBonus = 0;

    void StartNextLevel(Level nextLevel) {
        character.currentLevel = nextLevel;
        character.timer = 0;
        character.rings = 0;
        character.respawnData.position = character.currentLevel.spawnPosition;
        character.checkpointId = 0;

        if (GlobalOptions.Get("levelTransitions") != "OFF") {
            character.timerPause = false;
            character.TryGetCapability("victory", (CharacterCapability capability) => {
                ((CharacterCapabilityVictory)capability).victoryLock = false;
            });
            character.positionMax = Mathf.Infinity * Vector2.one;

            if (character.characterCamera != null)
                character.characterCamera.maxPosition = Mathf.Infinity * Vector2.one;

            ObjTitleCard titleCard = ObjTitleCard.Make(character, false);
        } else {
            character.currentLevel.ReloadFadeOut(character);
        }
        Destroy(gameObject);
    }

    void LoadNextLevel() {
        if (sceneReference.ScenePath != "") {
            StartCoroutine(Utils.LoadLevelAsync(
                sceneReference.ScenePath,
                StartNextLevel
            ));
        } else onNextLevel.Invoke();
    }

    // Update is called once per frame
    void Update() {
        if (endTimer <= 0) return;

        if (GlobalOptions.Get("levelTransitions") == "INSTANT") {
            if (sceneReference.ScenePath != "") {
                character.score += GetTimeBonus(character.timer);
                character.score += character.rings * 100;
                character.effects.Clear();
                LoadNextLevel();
                endTimer = 0;
                return;
            }
        }

        scoreTextComponent.text = character.score.ToString();
        ringTextComponent.text = ringBonus.ToString();
        timeTextComponent.text = timeBonus.ToString();

        if (showTimer > 0) {
            showTimer -= Utils.cappedDeltaTime;
            if (showTimer <= 0) {
                // animator.Play("Items Enter");
                timeBonus = GetTimeBonus(character.timer);
                ringBonus = character.rings * 100;

                if (character.characterCamera != null)
                    canvas.worldCamera = character.characterCamera.camera; // i hate this name too, trust me
                    
                actTextComponent.text = character.currentLevel.act.ToString();

                character.TryGetCapability("victory", (CharacterCapability capability) => {
                    ((CharacterCapabilityVictory)capability).victoryLock = true;
                });
                character.effects.Clear();

                MusicManager.current.Play(new MusicManager.MusicStackEntry{
                    introPath = "Music/Level Clear"
                });
            }
            return;
        }

        if (tallyTimer > 0) {
            tallyTimer -= Utils.cappedDeltaTime;
            return;
        }

        if ((timeBonus > 0) || (ringBonus > 0)) {
            if (tallyFrameTimer > 0) {
                tallyFrameTimer -= Utils.cappedDeltaTime;
                if (tallyFrameTimer <= 0)
                    tallyFrameTimer = 1F / 60F;
                else return;
            }

            int transferAmtTime = Mathf.Min(100, timeBonus);
            int transferAmtRing = Mathf.Min(100, ringBonus);

            if (Input.GetButtonDown("Pause")) {
                transferAmtTime = timeBonus;
                transferAmtRing = ringBonus;
            }

            timeBonus -= transferAmtTime;
            ringBonus -= transferAmtRing;

            character.score += transferAmtTime;
            character.score += transferAmtRing;
            SFX.Play(audioSource, "sfxTallyBeep");

            if ((timeBonus <= 0) && (ringBonus <= 0)) {
                SFX.Play(audioSource, "sfxTallyChaChing");
                if (GlobalOptions.Get("levelTransitions") != "OFF")
                    animator.Play("Items Exit");
            }
            return;
        }

        if (endTimer > 0) {
            endTimer -= Utils.cappedDeltaTime;
            if (endTimer <= 0) LoadNextLevel();
        }
    }
}
