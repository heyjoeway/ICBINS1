using UnityEngine;
using ActionCode2D.Renderers;

public class CharacterCapabilityAfterImage3D : CharacterCapability {
    public CharacterCapabilityAfterImage3D(Character character) : base(character) { }

    Transform afterImage;
    Animator afterImageAnimator;

    public override void Init() {
        afterImage = character.spriteContainer.Find("Afterimage");
        afterImageAnimator = afterImage.GetComponent<Animator>();
    }

    Vector3[] positionHistory = new Vector3[5];
    Vector3[] scaleHistory = new Vector3[5];
    Quaternion[] rotationHistory = new Quaternion[5];
    int historyIndex = 0;
    bool historyFlicker = false;

    public override void Update(float deltaTime) {
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

        int historyIndexOffset = historyIndex - (historyFlicker ? 1 : 3);
        if (historyIndexOffset < 0)
            historyIndexOffset += 5;

        afterImage.position = positionHistory[historyIndexOffset];
        afterImage.localScale = scaleHistory[historyIndexOffset];
        afterImage.rotation = rotationHistory[historyIndexOffset];

        historyFlicker = !historyFlicker;
        historyIndex++;
        historyIndex %= 5;
        positionHistory[historyIndex] = character.sprite.position; 
        rotationHistory[historyIndex] = character.sprite.rotation;
        scaleHistory[historyIndex] = character.sprite.localScale;

        afterImage.gameObject.SetActive(
            character.HasEffect("afterImage") ||
            character.HasEffect("speedUp") ||
            character.HasEffect("boostMode")
        );
    }
}