using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGHZ2 : Level {
    public GameObject levelBoundLeft;
    // Vector3 positionOffset = new Vector3(336, -16, 0);
    Vector3 cameraMin0 = new Vector3(21.5F, -3.5F, -99999F);
    Vector3 cameraMin1 = new Vector3(349F, -19.46875F, -99999F);
    Vector3 cameraMin2 = new Vector3(349F, -11.5F, -99999F);
    Vector3 cameraMin3 = new Vector3(349F, -27.46875F, -99999F);
    Vector3 cameraMin4 = new Vector3(349F, -19.46875F, -99999F);
    Vector3 cameraMax = new Vector3(595F, 28.5F, 99999F);

    public override void DLEUpdateCharacter(CharacterPackage characterPackage) {
        Vector3 characterPos = characterPackage.character.position;
        CharacterCamera characterCamera = characterPackage.camera;

        characterCamera.maxPosition = cameraMax;
        if ((characterCamera.minPosition.x < cameraMin1.x) && (characterPos.x < cameraMin1.x)) {
            characterCamera.minPosition = cameraMin0;
            return;
        }
        if (characterCamera.minPosition.x < cameraMin1.x) {
            levelBoundLeft.SetActive(true);
            Utils.GetLevelManager().UnloadUnpopulatedLevels();
        }
        characterCamera.minPosition = cameraMin1;
        if (characterPos.x > 467.85F) characterCamera.minPosition = cameraMin2;
        if (characterPos.x > 520F) characterCamera.minPosition = cameraMin3;
        if (characterPos.x > 585F) characterCamera.minPosition = cameraMin4;
    }
}
