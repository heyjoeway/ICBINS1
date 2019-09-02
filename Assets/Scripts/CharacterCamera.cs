using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour {
    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================

    Character character;
    public new Camera camera;

    void InitReferences() {
        character = characterObj.GetComponent<Character>();
        camera = GetComponent<Camera>();
    }

    // ========================================================================
    // PUBLIC VARIABLES
    // ========================================================================

    public GameObject characterObj;
    public float zDistance = -13.5F;
    public Vector3 minPosition = new Vector3(
        -Mathf.Infinity,
        -Mathf.Infinity,
        -Mathf.Infinity
    );
    public Vector3 maxPosition  = new Vector3(
        Mathf.Infinity,
        Mathf.Infinity,
        Mathf.Infinity
    );
    public float lagTimer = 0;

    // ========================================================================
    // CONSTANTS
    // ========================================================================

    const float hBorderDistance = 8F;
    const float hMoveMax = 16F;
    const float vMoveMaxGroundSlow = 6F;
    const float vMoveMaxGroundFast = 16F;
    const float vMoveMaxAir = 16F;
    const float vBorderDistanceAir = 32F;
    const float valScale = 1F/32F;

    // ========================================================================

    GameObject _backgroundObj;
    public GameObject backgroundObj {
        get { return _backgroundObj; }
        set {
            if (_backgroundObj != null) return;
            Destroy(_backgroundObj);
            _backgroundObj = Instantiate(
                value,
                Vector3.zero,
                Quaternion.identity
            );
            Background background = _backgroundObj.GetComponent<Background>();
            background.characterPackage = character.characterPackage;
            BackgroundCamera backgroundCamera = background.backgroundCamera;
            backgroundCamera.renderTexture = renderTexture;
            backgroundCamera.GetComponent<Camera>().targetDisplay = camera.targetDisplay;
        }
    }


    Vector3 position;
    Vector3 moveAmt;
    RenderTexture renderTexture;
    // Material material;
    
    void Start() {
        InitReferences();
        renderTexture = new RenderTexture(camera.targetTexture);
        position = transform.position;
    }

    float hDist;
    public bool hLock = false;
    public bool preventExit = false;

    void LateUpdate() {
        if (character.inDeadState) return;

        if (lagTimer > 0) {
            lagTimer -= Time.deltaTime;
            return;
        }

        moveAmt.Set(0,0,0);

        Transform characterLocation = character.spriteObject.transform;
        Vector3 characterPosition = Vector3.Min(
            maxPosition,
            Vector3.Max(
                minPosition,
                characterLocation.position
            )
        );

        // Move camera horizontally towards character but not past them,
        // only move a max of hMoveMax, and restrict self to boundaries
        float hDist = characterPosition.x - transform.position.x;
        if (Mathf.Abs(hDist) > hBorderDistance * valScale) {
            moveAmt.x = Mathf.Abs(hDist) - (hBorderDistance * valScale);
            moveAmt.x = Mathf.Min(moveAmt.x, hMoveMax * valScale);
            moveAmt.x = Mathf.Sign(hDist) * moveAmt.x;
        }
        
        float vDist = characterPosition.y - transform.position.y;
        if (character.inGroundedState) {
            if (Mathf.Abs(vDist) > 0) {
                moveAmt.y = Mathf.Abs(vDist);
                if (character.velocity.y <= 6F * character.physicsScale)
                    moveAmt.y = Mathf.Min(moveAmt.y, vMoveMaxGroundSlow * valScale);
                else
                    moveAmt.y = Mathf.Min(moveAmt.y, vMoveMaxGroundFast * valScale);

                moveAmt.y = Mathf.Sign(vDist) * moveAmt.y;
            }
        } else {
            if (Mathf.Abs(vDist) > vBorderDistanceAir * valScale) {
                moveAmt.y = Mathf.Abs(vDist) - (vBorderDistanceAir * valScale);
                moveAmt.y = Mathf.Min(moveAmt.y, vMoveMaxAir * valScale);
                moveAmt.y = Mathf.Sign(vDist) * moveAmt.y;
            }
        }

        position += moveAmt * (Time.deltaTime * 60F);
        position.z = characterPosition.z + zDistance;
        
        // position = Vector3.Min(maxPosition, Vector3.Max(minPosition, position));
        transform.position = position;
    }

    Vector2 resolutionPrev;

    void ResizeRenderTexture() {
        if (
            (resolutionPrev.x == Screen.width) &&
            (resolutionPrev.y == Screen.height)
         ) return;
        resolutionPrev = new Vector2(Screen.width, Screen.height);
        
        renderTexture.Release();
        renderTexture.width = (int)Mathf.Round(
            (float)renderTexture.height * (
                ((float)Screen.width) /
                ((float)Screen.height)
            )
        );
        camera.orthographicSize = (renderTexture.height / 32) * 0.5F;
        // renderTexture.width = Screen.width;
        // renderTexture.height = Screen.height;
    }

    void OnPreRender() {
        ResizeRenderTexture();
        camera.targetTexture = renderTexture;
    }


     void OnPostRender() {
        camera.targetTexture = null;
        Graphics.Blit(
            renderTexture,
            null as RenderTexture
            // material
        );
    }
}
