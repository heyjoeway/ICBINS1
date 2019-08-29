using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBridge : MonoBehaviour {

    // ========================================================================
    // OBJECT AND COMPONENT REFERENCES
    // ========================================================================

    Transform links;

    void InitReferences() {
        links = transform.Find("Links");
    }

    void Start() {
        InitReferences();
    }

    // ========================================================================
    // CONSTANTS
    // ========================================================================
    
    const float maxDepression = -0.5F;
    const int depressSpeed = 2;

    // ========================================================================
    // PRIVATE VARIABLES
    // ========================================================================

    float acrossBridgeAmtCurrent = 0;

    float timer = 0;
    public Character characterCurrent;

    // ========================================================================

    Character character { get {
        for (int i = 0; i < links.childCount; i++) {
            Transform child = links.GetChild(i);
            CharacterGroundedDetector detector = child.GetComponent<CharacterGroundedDetector>();
            if (detector == null) continue;
            foreach (var character in detector.characters) 
                if (character != null) return character;
        }
        return null;
    }}
    float? acrossBridgeAmt { get {
        if (characterCurrent == null) return null;

        Bounds bounds = new Bounds();
        Collider[] colliders = links.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            bounds.Encapsulate(col.bounds);

        float bridgeBoundsRightWorld = bounds.max.x;
        float bridgeWidth = 2F * (bridgeBoundsRightWorld - transform.position.x);
        float bridgeBoundsLeftWorld = bridgeBoundsRightWorld - bridgeWidth;

        float amt = (
            characterCurrent.position.x -
            bridgeBoundsLeftWorld
        ) / bridgeWidth;

        if ((amt > 1) || (amt < 0)) return null;
        return amt;
    }}

    // ========================================================================

    void Update() {
        characterCurrent = character;

        if (characterCurrent != null) {
            timer += depressSpeed * Utils.deltaTimeScale;
            timer = Mathf.Min(90, timer);
            float? acrossBridgeAmtTemp = acrossBridgeAmt;
            if (acrossBridgeAmt != null)
                acrossBridgeAmtCurrent = (float)acrossBridgeAmtTemp;
        } else if (timer > 0) {
            timer -= depressSpeed * Utils.deltaTimeScale;
            timer = Mathf.Max(0, timer);
        }

        // Set the position of each log
        for (int i = 0; i < links.childCount; i++) {
            Transform child = links.GetChild(i);
            Vector3 childPosition = child.position;

            if (timer == 0) {
                childPosition.y = transform.position.y;
            } else {
                float logPosAmt = i / (float)links.childCount;
                float dipAmt = 1 - Mathf.Abs(logPosAmt - (float)acrossBridgeAmtCurrent);
                float dipLimiter = Mathf.Sin(Mathf.PI * logPosAmt);
                float timerLimiter = Mathf.Sin(Mathf.Deg2Rad * timer);

                childPosition.y = transform.position.y + (
                    dipAmt * dipLimiter * maxDepression * timerLimiter
                );
            }

            child.position = childPosition;
        }
    }    
}