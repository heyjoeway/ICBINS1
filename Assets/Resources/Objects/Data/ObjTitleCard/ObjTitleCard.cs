using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjTitleCard : MonoBehaviour {
    bool debug = false; // Finishes card instantly

    public static ObjTitleCard Make(Character character = null, bool fadeIn = true) {
        ObjTitleCard titleCard = Instantiate(
            Constants.Get<GameObject>("prefabTitleCard"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ObjTitleCard>();
        
        if (character != null)
            titleCard.character = character;
        
        if (!fadeIn)
            titleCard.screenFade.brightness = titleCard.screenFade.brightnessMax;
            
        titleCard.Init();
        return titleCard;
    }

    [HideInInspector]
    public Character character;
    [HideInInspector]
    public ScreenFade screenFade => GetComponent<ScreenFade>();
    [HideInInspector]
    public Canvas canvas;
    Animator animator;
    Text zoneTextComponent;
    Text actTextComponent;

    public void StartTime() {
        Time.timeScale = 1;
    }

    void Awake() {
        actTextComponent = transform.Find("Act Value").GetComponent<Text>();
        zoneTextComponent = transform.Find("Zone Name").GetComponent<Text>();
        canvas = GetComponent<Canvas>();
        animator = GetComponent<Animator>();

        if (debug) {
            Destroy(screenFade.gameObject);
            StartTime();
            Destroy(gameObject);
            return;
        }
    }

    public void Init() {
        if (character.characterCamera != null)
            canvas.worldCamera = character.characterCamera.camera;

        actTextComponent.text = character.currentLevel.act.ToString();
        zoneTextComponent.text = character.currentLevel.zone.ToUpper();

        screenFade.Update();
    }

    void Start() {
        if (MusicManager.current.musicStackEntryCurrent != null)
            if (character.currentLevel.musicLoop == MusicManager.current.musicStackEntryCurrent.loopClip)
                return;

        MusicManager.current.Play(new MusicManager.MusicStackEntry{
            introClip = character.currentLevel.musicIntro,
            loopClip = character.currentLevel.musicLoop
        });
    }

    void Update() {
        animator.Update(Utils.cappedUnscaledDeltaTime);
    }
}
