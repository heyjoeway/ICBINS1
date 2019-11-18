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
        Hurt,
        ShieldElectricity,
        ShieldBubble,
        ShieldFire
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

    void Start() {
        if (!GlobalOptions.GetBool("elementalShields")) {
            switch(type) {
                case ContentsType.ShieldElectricity:
                case ContentsType.ShieldBubble:
                case ContentsType.ShieldFire:
                    type = ContentsType.Shield;
                    break;
            }
        }

        animator.Play(ContentsAnimations[(int)type]);
    }

    public void Award() {
        switch(type) {
            case ContentsType.Ring:
                recipient.rings += 10;
                SFX.Play(audioSource, "sfxRingMonitor");
                break;
            case ContentsType.Shield:
                SFX.Play(audioSource, "sfxShieldNormal");
                recipient.shield = Instantiate(
                    Constants.Get<GameObject>("prefabShieldNormal")
                ).GetComponent<ObjShield>();
                recipient.shield.character = recipient;
                break;
            case ContentsType.Life:
                recipient.lives++;
                break;
            case ContentsType.Shoes:
                recipient.effects.Add(new CharacterEffectSpeedUp(recipient));
                break;
            case ContentsType.Invincibility:
                recipient.effects.Add(new CharacterEffectInvincible(recipient));
                break;
        }
    }

    public void Destroy() {
        Destroy(gameObject);
    }
}
