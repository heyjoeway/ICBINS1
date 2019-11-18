using System.Collections;
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
        timeTitleText = transform.Find("Time Title").GetComponent<Text>();
        ringsText = transform.Find("Rings Content").GetComponent<Text>();
        ringsTitleText = transform.Find("Rings Title").GetComponent<Text>();
        livesText = transform.Find("Lives Content").GetComponent<Text>();
    }

    public void Update() {
        canvas.worldCamera = character.characterCamera.camera;
        // Debug.Log(character.characterCamera.camera);

        scoreText.text = character.score.ToString();
        ringsText.text = character.rings.ToString();
        livesText.text = character.lives.ToString();

        timeText.text = (
            Mathf.Floor(character.timer / 60).ToString() +
            ":" +
            Mathf.Floor(character.timer % 60).ToString().PadLeft(2, '0')
        );

        if (GlobalOptions.Get("timerType") == "CENTISECOND")
            timeText.text += ":" + Mathf.Floor((character.timer % 1) * 100F).ToString().PadLeft(2, '0');
        
        if (GlobalOptions.Get("timerType") == "FRAMES")
            timeText.text += ":" + Mathf.Floor((character.timer * 60) % 60).ToString().PadLeft(2, '0');

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
