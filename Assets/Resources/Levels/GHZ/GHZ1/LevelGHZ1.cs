using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGHZ1 : LevelGHZ {
    public override string zone { get { return "Unknown"; }}
    public override int act { get { return 1; }}

    Vector2 positionMin = new Vector2(16, -8);
    Vector2 positionMax = new Vector2(328, 40);

    // Vector3 positionOffset = Vector3.zero;
    Vector3 cameraMin1 = new Vector3(21.5F, 4.5F, -99999F);
    Vector3 cameraMin2 = new Vector3(21.5F, -3.5F, -99999F);
    Vector3 cameraMax = new Vector3(315.25F, 28.5F, 99999F);

    public override void DLEUpdateCharacter(CharacterPackage characterPackage) {
        CharacterCamera characterCamera = characterPackage.camera;
        Character character = characterPackage.character;

        if (character.position.x < positionMax.x - 16)
            character.positionMax = positionMax;

        characterCamera.maxPosition = cameraMax;

        if (character.position.x < 200) {
            characterCamera.minPosition = cameraMin1;
            // Hack
            character.positionMin = positionMin;
        } else characterCamera.minPosition = cameraMin2;
    }
}
