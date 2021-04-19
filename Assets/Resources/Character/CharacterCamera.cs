using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour {
    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================

    public new Camera camera;
    RenderTextureCamera renderTextureCamera;

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
            if (_minPositionTarget == value) return;
            Vector3 minPositionTargetPrev = _minPositionTarget;

            _minPositionTarget = value;

            if (Mathf.Abs(_minPositionReal.magnitude) == Mathf.Infinity)
                _minPositionReal = _minPositionTarget;

            if (_minPositionTarget.x != minPositionTargetPrev.x) {
                // Locking left
                if (minPositionTargetPrev.x < _minPositionTarget.x) {
                    _minPositionReal.x = Mathf.Min(
                        transform.position.x,
                        _minPositionTarget.x
                    );
                // Unlocking left
                } else {
                    _minPositionReal.x = Mathf.Max(
                        transform.position.x,
                        _minPositionTarget.x
                    );
                }
            }

            if (_minPositionTarget.y != minPositionTargetPrev.y) {
                // Locking bottom
                if (minPositionTargetPrev.y < _minPositionTarget.y) {
                    _minPositionReal.y = Mathf.Min(
                        transform.position.y,
                        _minPositionTarget.y
                    );
                } else {
                    _minPositionReal.y = Mathf.Max(
                        transform.position.y,
                        _minPositionTarget.y
                    );
                }
            }

        }
    }

    Vector2 _maxPositionTarget  = Vector2.one * Mathf.Infinity;
    Vector2 _maxPositionReal  = Vector2.one * Mathf.Infinity;
    public Vector2 maxPosition  {
        get { return _maxPositionTarget; }
        set {
            if (_maxPositionTarget == value) return;
            Vector3 maxPositionTargetPrev = _maxPositionTarget;
            _maxPositionTarget = value;


            if (Mathf.Abs(_maxPositionReal.magnitude) == Mathf.Infinity)
                _maxPositionReal = _maxPositionTarget;

            if (_maxPositionTarget.x != maxPositionTargetPrev.x) {
                // Unlocking right
                if (maxPositionTargetPrev.x < _maxPositionTarget.x) {
                    _maxPositionReal.x = Mathf.Max(
                        transform.position.x,
                        _maxPositionTarget.x
                    );
                // Locking right
                } else {
                    _maxPositionReal.x = Mathf.Min(
                        transform.position.x,
                        _maxPositionTarget.x
                    );
                }
            }

            if (_maxPositionTarget.y != maxPositionTargetPrev.y) {
                // Locking bottom
                if (maxPositionTargetPrev.y < _maxPositionTarget.y) {
                    _maxPositionReal.y = Mathf.Max(
                        transform.position.y,
                        _maxPositionTarget.y
                    );
                } else {
                    _maxPositionReal.y = Mathf.Min(
                        transform.position.y,
                        _maxPositionTarget.y
                    );
                }
            }
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
    float valScale => character.sizeScale / 32F;

    // ========================================================================
    public Vector3 position {
        get { return transform.position; }
        set { transform.position = value; }
    }

    Vector2 moveAmt;    

    public void LockHorizontal() { LockHorizontal(transform.position.x); }
    public void LockHorizontal(float xPos) {
        minPosition = new Vector2(xPos, minPosition.y);
        maxPosition = new Vector2(xPos, maxPosition.y);
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
            _minPositionTarget.x - (screenWidthWorld / 2F) + 0.5F,
            _minPositionTarget.y - (screenHeightWorld / 2F) + 1F
        );

        character.positionMax = new Vector2(
            _maxPositionTarget.x + (screenWidthWorld / 2F) - 0.5F,
            _maxPositionTarget.y + (screenHeightWorld / 2F) + 1F
        );
    }

    float minMaxMoveAmtMax = 0;

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

    void Awake() {
        renderTextureCamera = GetComponent<RenderTextureCamera>();
        if (renderTextureCamera == null) return;
        renderTextureCamera.ResizeRenderTexture();
        camera.targetTexture = new RenderTexture(camera.targetTexture);
        camera.targetTexture.filterMode = FilterMode.Point;
    }

    public void UpdateDelta(float deltaTime) {
        if (character == null) return;
        if (character.InStateGroup("death")) return;

        minMaxMoveAmtMax += (0.1F / 32F) * 60F;
        MoveMinMaxTowardsTarget();
        if ((_minPositionReal == _minPositionTarget) && (_maxPositionReal == _maxPositionTarget))
            minMaxMoveAmtMax = 0;

        if (lagTimer > 0) {
            lagTimer -= deltaTime;
            return;
        }

        moveAmt = Vector2.zero;

        Transform characterLocation = character.spriteContainer;
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
        if (character.InStateGroup("ground")) {
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

        if (renderTextureCamera != null)
            camera.orthographicSize = (
                ((camera.targetTexture.height / 32) * 0.5F) *
                character.sizeScale
            );

        Vector3 newPos = transform.position;
        newPos += (Vector3)moveAmt * deltaTime * 60F;
        Vector2 minMaxLeniency = Vector2.one * ((3.5F - camera.orthographicSize) / 3.5F) * 6F;
        newPos = Vector2.Min(
            _maxPositionReal + minMaxLeniency,
            Vector2.Max(
                _minPositionReal - minMaxLeniency,
                newPos
            )
        );
        newPos.z = characterPosition.z + zDistance;
        transform.position = newPos;

        if (renderTextureCamera != null)
            renderTextureCamera.screenRectRelative = screenRects[
                LevelManager.current.characters.Count - 1
            ][character.playerId];
    }

    public static List<Rect[]> screenRects = new List<Rect[]> {
        new Rect[] {
            new Rect(0, 0, 1, 1)
        },
        new Rect[] {
            new Rect(0, 0, 1, 0.5F),

            new Rect(0, 0.5F, 1, 0.5F)
        },
        new Rect[] {
            new Rect(0.25F, 0, 0.5F, 0.5F),

            new Rect(0F, 0.5F, 0.5F, 0.5F),
            new Rect(0.5F, 0.5F, 0.5F, 0.5F)
        },
        new Rect[] {
            new Rect(0, 0, 0.5F, 0.5F),
            new Rect(0.5F, 0, 0.5F, 0.5F),

            new Rect(0, 0.5F, 0.5F, 0.5F),
            new Rect(0.5F, 0.5F, 0.5F, 0.5F)
        },
        new Rect[] {
            new Rect(0.16667F, 0, 0.33333F, 0.5F),
            new Rect(0.5F, 0, 0.33333F, 0.5F),

            new Rect(0, 0.5F, 0.33333F, 0.5F),
            new Rect(0.33333F, 0.5F, 0.33333F, 0.5F),
            new Rect(0.66667F, 0.5F, 0.33333F, 0.5F)
        },
        new Rect[] {
            new Rect(0, 0, 0.33333F, 0.5F),
            new Rect(0.33333F, 0, 0.33333F, 0.5F),
            new Rect(0.66667F, 0, 0.33333F, 0.5F),

            new Rect(0, 0.5F, 0.33333F, 0.5F),
            new Rect(0.33333F, 0.5F, 0.33333F, 0.5F),
            new Rect(0.66667F, 0.5F, 0.33333F, 0.5F)
        },
        new Rect[] {
            new Rect(0.16667F, 0, 0.33333F, 0.33333F),
            new Rect(0.5F, 0, 0.33333F, 0.33333F),

            new Rect(0, 0.33333F, 0.33333F, 0.33333F),
            new Rect(0.33333F, 0.33333F, 0.33333F, 0.33333F),
            new Rect(0.66667F, 0.33333F, 0.33333F, 0.33333F),

            new Rect(0.16667F, 0.66667F, 0.33333F, 0.33333F),
            new Rect(0.5F, 0.66667F, 0.33333F, 0.33333F)
        },
        new Rect[] {
            new Rect(0.16667F,  0,        0.33333F, 0.33333F),
            new Rect(0.5F,      0,        0.33333F, 0.33333F),

            new Rect(0,         0.33333F, 0.33333F, 0.33333F),
            new Rect(0.33333F,  0.33333F, 0.33333F, 0.33333F),
            new Rect(0.66667F,  0.33333F, 0.33333F, 0.33333F),

            new Rect(0,         0.66667F, 0.33333F, 0.33333F),
            new Rect(0.33333F,  0.66667F, 0.33333F, 0.33333F),
            new Rect(0.66667F,  0.66667F, 0.33333F, 0.33333F),
        },
        new Rect[] {
            new Rect(0,         0,        0.33333F, 0.33333F),
            new Rect(0.33333F,  0,        0.33333F, 0.33333F),
            new Rect(0.66667F,  0,        0.33333F, 0.33333F),

            new Rect(0,         0.33333F, 0.33333F, 0.33333F),
            new Rect(0.33333F,  0.33333F, 0.33333F, 0.33333F),
            new Rect(0.66667F,  0.33333F, 0.33333F, 0.33333F),

            new Rect(0,         0.66667F, 0.33333F, 0.33333F),
            new Rect(0.33333F,  0.66667F, 0.33333F, 0.33333F),
            new Rect(0.66667F,  0.66667F, 0.33333F, 0.33333F),
        }
    };

    public static List<Rect[]> screenRectsAlt = new List<Rect[]> {
        null,
        new Rect[] {
            new Rect(0, 0, 0.5F, 1),
            new Rect(0.5F, 0, 0.5F, 1)
        },
        null,
        null
    };

    public CameraZone cameraZone;
}
