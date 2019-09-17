using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour {
    void Start() {
        Utils.SetFramerate();
    }

    public void Exit() {
        ScreenFade fader = Instantiate(
            Resources.Load<GameObject>("Objects/Screen Fade Out")
        ).GetComponent<ScreenFade>();
        fader.onComplete = () => {
            SceneManager.LoadScene(
                "Scenes/Title Screen",
                LoadSceneMode.Single
            );
        };
    }
}