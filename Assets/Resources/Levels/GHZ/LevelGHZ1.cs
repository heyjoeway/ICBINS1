using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGHZ1 : Level {
    Vector3 cameraMin1 = new Vector3(21.5F, 4.5F, -99999F);
    Vector3 cameraMin2 = new Vector3(21.5F, -3.5F, -99999F);
    Vector3 cameraMax = new Vector3(314.2F, 28.5F, 99999F);

    public override void DLEUpdateCharacter(CharacterPackage characterPackage) {
        Vector3 characterPos = characterPackage.character.position;
        CharacterCamera characterCamera = characterPackage.camera;

        characterCamera.maxPosition = cameraMax;
        if (characterPos.x < 200) characterCamera.minPosition = cameraMin1;
        else characterCamera.minPosition = cameraMin2;
    }
}
