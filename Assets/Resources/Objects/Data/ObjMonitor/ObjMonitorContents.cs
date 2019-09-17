using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjMonitorContents : MonoBehaviour {
    [HideInInspector]
    public Character recipient;

    public enum ContentsType {
        Ring,
        Invincibility,
        Shield,
        Shoes,
        Life,
        Super,
        Goggles,
        Hurt
    }

    string[] ContentsAnimations = new string[] {
        "Ring",
        "Invincibility",
        "Shield",
        "Shoes",
        "Life",
        "Super",
        "Goggles",
        "Hurt"
    };

    public AudioSource audioSource { get {
        return GetComponent<AudioSource>();
    }}

    public ContentsType type = ContentsType.Ring;

    Animator animator { get { return GetComponent<Animator>(); }}

    void Start() { animator.Play(ContentsAnimations[(int)type]); }

    public void Award() {
        switch(type) {
            case ContentsType.Ring:
                recipient.rings += 10;
                SFX.Play(audioSource, "SFX/Sonic 1/S1_B5");
                break;
            case ContentsType.Shield:
                SFX.Play(audioSource, "SFX/Sonic 1/S1_AF");
                recipient.shield = Instantiate(Resources.Load<GameObject>(
                    "Objects/Shield (Normal)"
                )).GetComponent<ObjShield>();
                recipient.shield.character = recipient;
                break;
            case ContentsType.Life:
                recipient.lives++;
                break;
            case ContentsType.Shoes:
                recipient.speedUpTimer += 20F;
                break;
            case ContentsType.Invincibility:
                recipient.invincibilityTimer += 20F;
                ObjShield stars = Instantiate(Resources.Load<GameObject>(
                    "Objects/Invincibility Stars"
                )).GetComponent<ObjShield>();
                stars.character = recipient;
                break;
        }
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}
