using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGHZ3 : LevelGHZ {
    public override int act { get { return 3; }}

    Vector2 positionMinTransition = new Vector2(309.5F, -40);
    Vector2 positionMin = new Vector2(608, -40);
    Vector2 positionMax = new Vector2(933.5F, 24);

    // Vector3 positionOffset = new Vector3(336, -16, 0);
    Vector3 cameraMax = new Vector3(925.5F, 4.5F, 99999F);    
    Vector3 cameraMin0 = new Vector3(595F, -19.5F, -99999F);
    Vector3 cameraMin1 = new Vector3(614F, -19.5F, -99999F);
    Vector3 cameraMin2 = new Vector3(614F, -35.5F, -99999F);
    Vector3 cameraMin3 = new Vector3(614F, -27.5F, -99999F);
    Vector3 cameraMin4 = new Vector3(614F, -19.5F, -99999F);


    public override void DLEUpdateCharacter(CharacterPackage characterPackage) {
        Character character = characterPackage.character;
        CharacterCamera characterCamera = characterPackage.camera;

        if (character.position.x > positionMax.x - 16)
            return;

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
        if (character.position.x > 680F) characterCamera.minPosition = cameraMin2;
        if (character.position.x > 770F) characterCamera.minPosition = cameraMin3;
        if (character.position.x > 800F) characterCamera.minPosition = cameraMin4;
    }
}
