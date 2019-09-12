using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
    void Update() {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        ScreenFade screenFade = Instantiate(
            Resources.Load<GameObject>("Objects/Screen Fade Out"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ScreenFade>();
        screenFade.stopTime = true;
        screenFade.onComplete = () => {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
                "Scenes/Level",
                LoadSceneMode.Single
            );
            asyncLoad.allowSceneActivation = true;
        };
        enabled = false;
    }
}
