using UnityEngine;
using ActionCode2D.Renderers;
using System;

public class CharacterCapabilityAfterImage3D : CharacterCapability {
    Transform afterImage;
    Animator afterImageAnimator;
    Rewindable<Vector3> positionHistory;
    Rewindable<Vector3> scaleHistory;
    Rewindable<Quaternion> rotationHistory;

    public override void Init() {
        positionHistory = Rewindable<Vector3>.Create();
        scaleHistory = Rewindable<Vector3>.Create();
        rotationHistory = Rewindable<Quaternion>.Create();
        afterImage = character.spriteContainer.Find("Afterimage");
        afterImageAnimator = afterImage.GetComponent<Animator>();
    }

    bool historyFlicker = false;

    public override void CharUpdate(float deltaTime) {
        positionHistory.Set(character.sprite.position); 
        rotationHistory.Set(character.sprite.rotation);
        scaleHistory.Set(character.sprite.localScale);
        
        bool active = (
            character.HasEffect("afterImage") ||
            character.HasEffect("speedUp") ||
            character.HasEffect("boostMode")
        );

        afterImage.gameObject.SetActive(active);
        if (!active) return;
        
        for (int i = 0; i < afterImageAnimator.layerCount; i++) {
            afterImageAnimator.Play(
                character.spriteAnimator.GetCurrentAnimatorStateInfo(i).fullPathHash,
                i,
                character.spriteAnimator.GetCurrentAnimatorStateInfo(i).normalizedTime
            );
            afterImageAnimator.SetLayerWeight(
                i,
                character.spriteAnimator.GetLayerWeight(i)
            );
        }

        int historyIndexOffset = (historyFlicker ? 1 : 3);
        historyFlicker = !historyFlicker;

        try {
            afterImage.position = positionHistory.GetFromIndex(
                positionHistory.RewindByIndex(historyIndexOffset, false)
            );
            afterImage.localScale = scaleHistory.GetFromIndex(
                scaleHistory.RewindByIndex(historyIndexOffset, false)
            );
            afterImage.rotation = rotationHistory.GetFromIndex(
                rotationHistory.RewindByIndex(historyIndexOffset, false)
            );

            afterImage.gameObject.SetActive(active);
        } catch (Exception e) { }
    }
}