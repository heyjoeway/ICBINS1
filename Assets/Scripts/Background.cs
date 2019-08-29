using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {
    static int backgroundCount = 0;
    public BackgroundCamera backgroundCamera {
        get { return transform.Find("Camera").GetComponent<BackgroundCamera>(); }
    }
    public CharacterPackage characterPackage;

    public virtual void Start() {
        transform.position = new Vector3(
            0, 0,
            (backgroundCount + 1) * -100 
        );
        backgroundCount++;
    }
}
