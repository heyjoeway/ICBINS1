using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour {
    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================

    public new Camera camera;

    // ========================================================================
    // PUBLIC VARIABLES
    // ========================================================================

    public Character character;
    public float zDistance = -13.5F;

    Vector2 _minPositionTarget = Vector2.one * -Mathf.Infinity;
    Vector2 _minPositionReal = Vector2.one * -Mathf.Infinity;
    public Vector2 minPosition  {
        get { return _minPositionTarget; }
        set {
            _minPositionTarget = value;
            if (Mathf.Abs(_minPositionReal.magnitude) == Mathf.Infinity)
                _minPositionReal = _minPositionTarget;
        }
    }

    Vector2 _maxPositionTarget  = Vector2.one * Mathf.Infinity;
    Vector2 _maxPositionReal  = Vector2.one * Mathf.Infinity;
    public Vector2 maxPosition  {
        get { return _maxPositionTarget; }
        set {
            _maxPositionTarget = value;
            if (Mathf.Abs(_maxPositionReal.magnitude) == Mathf.Infinity)
                _maxPositionReal = _maxPositionTarget;
        }
    }

    public void MinMaxPositionSnap() {
        _maxPositionReal = _maxPositionTarget;
        _minPositionReal = _minPositionTarget;
    }

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

    public GameObject backgroundObjRaw;
    public GameObject backgroundObj {
        get { return backgroundObjRaw; }
        set {
            if (backgroundObjRaw != null) return;
            Destroy(backgroundObjRaw);
            backgroundObjRaw = Instantiate(value);
            Background background = backgroundObjRaw.GetComponent<Background>();
            BackgroundCamera backgroundCamera = background.backgroundCamera;
            backgroundCamera.renderTexture = renderTexture;
            backgroundCamera.GetComponent<Camera>().targetDisplay = camera.targetDisplay;
            if (character == null) return;
            background.characterPackage = character.characterPackage;
        }
    }

    public Vector3 position;
    Vector2 moveAmt;
    RenderTexture renderTexture;
    
    void Start() {
        renderTexture = new RenderTexture(camera.targetTexture);
        renderTexture.filterMode = FilterMode.Point;
        position = transform.position;
    }

    float hDist;
    public bool preventExit = false;

    public void LockHorizontal() { LockHorizontal(transform.position.x); }
    public void LockHorizontal(float xPos) {
        _minPositionTarget.x = xPos;
        _maxPositionTarget.x = xPos;
        _minPositionReal.x = xPos;
        _maxPositionReal.x = xPos;
        position.x = xPos;
    }

    public void LockVertical() { LockVertical(transform.position.y); }
    public void LockVertical(float yPos) {
        _minPositionTarget.y = yPos;
        _maxPositionTarget.y = yPos;
    }

    public void SetCharacterBoundsFromCamera() {
        float screenHeightWorld = 224 / 32F;
        float screenWidthWorld = 398 / 32F;

        character.positionMin = new Vector2(
            minPosition.x - (screenWidthWorld / 2F) + 0.5F,
            minPosition.y - (screenHeightWorld / 2F) + 1F
        );

        character.positionMax = new Vector2(
            maxPosition.x + (screenWidthWorld / 2F) - 0.5F,
            maxPosition.y + (screenHeightWorld / 2F) + 1F
        );
    }

    const float minMaxMoveAmtMax = (6F / 32F) * 60F;

    void MoveMinMaxTowardsTarget() {
        float minXDist = _minPositionTarget.x - _minPositionReal.x;
        _minPositionReal.x += Mathf.Min(
            Mathf.Abs(minXDist),
            minMaxMoveAmtMax * Utils.cappedUnscaledDeltaTime
        ) * Mathf.Sign(minXDist);

        float maxXDist = _maxPositionTarget.x - _maxPositionReal.x;
        _maxPositionReal.x += Mathf.Min(
            Mathf.Abs(maxXDist),
            minMaxMoveAmtMax * Utils.cappedUnscaledDeltaTime
        ) * Mathf.Sign(maxXDist);

        float minYDist = _minPositionTarget.y - _minPositionReal.y; 
        _minPositionReal.y += Mathf.Min(
            Mathf.Abs(minYDist),
            minMaxMoveAmtMax * Utils.cappedUnscaledDeltaTime
        ) * Mathf.Sign(minYDist);

        float maxYDist = _maxPositionTarget.y - _maxPositionReal.y; 
        _maxPositionReal.y += Mathf.Min(
            Mathf.Abs(maxYDist),
            minMaxMoveAmtMax * Utils.cappedUnscaledDeltaTime
        ) * Mathf.Sign(maxYDist);
    }

    void LateUpdate() {
        if (character == null) return;
        if (character.inDeadState) return;

        MoveMinMaxTowardsTarget();

        if (lagTimer > 0) {
            lagTimer -= Utils.cappedUnscaledDeltaTime;
            return;
        }

        moveAmt = Vector2.zero;

        Transform characterLocation = character.spriteObject.transform;
        Vector3 characterPosition = characterLocation.position;

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

        position += (Vector3)moveAmt * (Utils.cappedUnscaledDeltaTime * 60F);
        position = Vector2.Min(
            _maxPositionReal,
            Vector2.Max(
                _minPositionReal,
                position
            )
        );
        position.z = characterPosition.z + zDistance;

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
        );
    }
}
