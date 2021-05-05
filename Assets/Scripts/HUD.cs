using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    public Character character;
    Canvas canvas;

    Text scoreText;
    Text timeText;
    Text timeTitleText;
    Text ringsText;
    Text ringsTitleText;
    Text livesText;

    void Awake() {
        canvas = GetComponent<Canvas>();
        scoreText = transform.Find("Score Content").GetComponent<Text>();
        timeText = transform.Find("Time Content").GetComponent<Text>();

        Transform timeTitle = transform.Find("Time Title");
        if (timeTitle) timeTitleText = timeTitle.GetComponent<Text>();
        else timeTitleText = timeText;

        ringsText = transform.Find("Rings Content").GetComponent<Text>();
        
        Transform ringsTitle = transform.Find("Rings Title");
        if (ringsTitle) ringsTitleText = ringsTitle.GetComponent<Text>();
        else ringsTitleText = ringsText;

        livesText = transform.Find("Lives Content").GetComponent<Text>();
    }
    StringBuilder sb = new StringBuilder("", 50);

    public void Update() {
        canvas.worldCamera = character.characterCamera.camera;
        // Debug.Log(character.characterCamera.camera);

        scoreText.text = Utils.IntToStrCached(character.score);
        ringsText.text = Utils.IntToStrCached(character.rings);
        livesText.text = Utils.IntToStrCached(character.lives);

        int minutes = (int)(character.timer / 60);
        int seconds = (int)(character.timer % 60);

        sb.Clear();
        sb.Append(Utils.IntToStrCached(minutes));
        sb.Append(":");
        if (seconds < 10) sb.Append("0");
        sb.Append(Utils.IntToStrCached(seconds));

        if (GlobalOptions.Get("timerType") != "NORMAL") {
            sb.Append(":");
            int preciseTime = 0;
            
            if (GlobalOptions.Get("timerType") == "CENTISECOND")
                preciseTime = (int)((character.timer % 1) * 100F);
            
            if (GlobalOptions.Get("timerType") == "FRAMES")
                preciseTime = (int)((character.timer * 60) % 60);

            if (preciseTime < 10) sb.Append("0");
            sb.Append(Utils.IntToStrCached(preciseTime));
        }
        
        timeText.text = sb.ToString();

        bool shouldFlash = (((int)(Time.unscaledTime * 60)) % 16) > 8;
        if (shouldFlash) {
            if (character.rings <= 0) ringsTitleText.color = Color.red;
    
            if (GlobalOptions.GetBool("timeLimit"))
                if (character.timer >= 9 * 60) timeTitleText.color = Color.red;
        } else {
            timeTitleText.color = Color.white;
            ringsTitleText.color = Color.white;
        }
    }
}
