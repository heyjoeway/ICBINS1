using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Modified from 
[RequireComponent(typeof(ScrollRect))]
public class ButtonSelectionController : MonoBehaviour {
    private ScrollRect scrollRect;
    GameObject prevSelection;

    public void Start() {
        scrollRect = GetComponent<ScrollRect>();
    }
    
    public void Update() {
        GameObject selection = EventSystem.current.currentSelectedGameObject;

        if (Mathf.Abs(selection.transform.position.y) > 0.25) {
            scrollRect.verticalNormalizedPosition += (
                Mathf.Sign(selection.transform.position.y) *
                Utils.cappedUnscaledDeltaTime * 1.5F
            );
        }
    }
}
