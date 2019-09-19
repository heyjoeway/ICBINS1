using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl : MonoBehaviour {
    public void StartScene(string scenePath) {
        ScreenFade screenFade = Instantiate(
            Resources.Load<GameObject>("Objects/Screen Fade Out"),
            Vector3.zero,
            Quaternion.identity
        ).GetComponent<ScreenFade>();
        screenFade.stopTime = true;
        screenFade.onComplete = () => {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
                scenePath,
                LoadSceneMode.Single
            );
            asyncLoad.allowSceneActivation = true;
        };
        enabled = false;
    }
}