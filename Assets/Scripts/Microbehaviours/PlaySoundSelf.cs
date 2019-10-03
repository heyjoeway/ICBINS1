using UnityEngine;

public class PlaySoundSelf : MonoBehaviour {
    public void PlaySound() {
        GetComponent<AudioSource>().Play();
    }
}