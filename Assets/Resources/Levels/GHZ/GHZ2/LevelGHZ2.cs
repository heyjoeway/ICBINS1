using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGHZ2 : LevelGHZ {
    public override int act { get { return 2; }}

    Vector2 positionMinTransition = new Vector2(309.5F, -40);
    Vector2 positionMin = new Vector2(343.2F, -40);
    Vector2 positionMax = new Vector2(605, 24);

    // Vector3 positionOffset = new Vector3(336, -16, 0);
    Vector3 cameraMin0 = new Vector3(315.25F, -3.5F, -99999F);
    Vector3 cameraMin1 = new Vector3(349F, -19.46875F, -99999F);
    Vector3 cameraMin2 = new Vector3(349F, -11.5F, -99999F);
    Vector3 cameraMin3 = new Vector3(349F, -27.46875F, -99999F);
    Vector3 cameraMin4 = new Vector3(349F, -19.5F, -99999F);
    Vector3 cameraMax = new Vector3(595F, 28.5F, 99999F);

    public override void DLEUpdateCharacter(CharacterPackage characterPackage) {
        Character character = characterPackage.character;
        CharacterCamera characterCamera = characterPackage.camera;

        if (character.position.x < positionMax.x - 16)
            character.positionMax = positionMax;

        characterCamera.maxPosition = cameraMax;
        if ((characterCamera.minPosition.x < cameraMin1.x) && (character.position.x < cameraMin1.x)) {
            characterCamera.minPosition = cameraMin0;
            character.positionMin = positionMinTransition;
            return;
        }
        if (characterCamera.minPosition.x < cameraMin1.x) {
            Utils.GetLevelManager().UnloadUnpopulatedLevels();
            character.positionMin = positionMin;
        }
        characterCamera.minPosition = cameraMin1;
        if (character.position.x > 467.85F) characterCamera.minPosition = cameraMin2;
        if (character.position.x > 520F) characterCamera.minPosition = cameraMin3;
        if (character.position.x > 585F) characterCamera.minPosition = cameraMin4;
    }
}
