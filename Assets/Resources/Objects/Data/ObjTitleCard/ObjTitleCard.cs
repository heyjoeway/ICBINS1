using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjTitleCard : MonoBehaviour {
    [HideInInspector]
    public Character character;
    [HideInInspector]
    public ScreenFade screenFade { get { return GetComponent<ScreenFade>(); }}
    Canvas canvas;
    Text zoneTextComponent;
    Text actTextComponent;

    public void StartTime() {
        Time.timeScale = 1;
    }

    public void Init() {
        actTextComponent = transform.Find("Act Value").GetComponent<Text>();
        zoneTextComponent = transform.Find("Zone Name").GetComponent<Text>();
        canvas = GetComponent<Canvas>();

        if (character.characterCamera != null)
            canvas.worldCamera = character.characterCamera.camera;

        actTextComponent.text = character.currentLevel.act.ToString();
        zoneTextComponent.text = character.currentLevel.zone.ToUpper();

        MusicManager musicManager = Utils.GetMusicManager();

        if (musicManager.musicStackEntryCurrent != null)
            if (character.currentLevel.musicLoop == musicManager.musicStackEntryCurrent.loopClip)
                return;

        musicManager.Play(new MusicManager.MusicStackEntry{
            introClip = character.currentLevel.musicIntro,
            loopClip = character.currentLevel.musicLoop
        });

        GetComponent<ScreenFade>().InitReferences();
    }
}
