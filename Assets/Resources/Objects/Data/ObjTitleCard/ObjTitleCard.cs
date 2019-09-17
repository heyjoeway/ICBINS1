using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjTitleCard : MonoBehaviour {
    [HideInInspector]
    public Character character;
    [HideInInspector]
    public ScreenFade screenFade;
    Canvas canvas;
    Text zoneTextComponent;
    Text actTextComponent;

    public void StartTime() {
        Time.timeScale = 1;
    }

    public void Init() {
        actTextComponent = transform.Find("Act Value").GetComponent<Text>();
        zoneTextComponent = transform.Find("Zone Name").GetComponent<Text>();
        screenFade = GetComponent<ScreenFade>();
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = character.characterCamera.camera;

        actTextComponent.text = character.currentLevel.act.ToString();
        zoneTextComponent.text = character.currentLevel.zone.ToUpper();

        MusicManager musicManager = Utils.GetMusicManager();
        musicManager.Play(new MusicManager.MusicStackEntry{
            introClip = character.currentLevel.musicIntro,
            loopClip = character.currentLevel.musicLoop
        });
    }
}
