using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Modified from 
[RequireComponent(typeof(ScrollRect))]
public class ButtonSelectionController : MonoBehaviour {
    public string verticalAxis = "Vertical";

    private ScrollRect          scrollRect;
    GameObject prevSelection;

    public void Start() {
        scrollRect = GetComponent<ScrollRect>();
    }


    float? targetHeight;

    public void Update() {
        if ((scrollRect.verticalNormalizedPosition == 0) && (targetHeight < 0))
            targetHeight = null;
            
        if (targetHeight != null) {

            float heightPrev = scrollRect.verticalNormalizedPosition;
            scrollRect.verticalNormalizedPosition = Mathf.MoveTowards(
                scrollRect.verticalNormalizedPosition,
                (float)targetHeight,
                Time.deltaTime * 2F
            );

            if (Mathf.Abs(scrollRect.verticalNormalizedPosition - (float)targetHeight) < 0.01)
                targetHeight = null;
        }

        GameObject selection = EventSystem.current.currentSelectedGameObject;
        if (prevSelection == selection) return;

        bool controlsPressed = (
            (Input.GetAxis("Vertical") != 0) ||
            InputCustom.GetButtons("Primary", "Secondary", "Tertiary")
        );

        if (controlsPressed && (prevSelection != null)) {
            targetHeight = scrollRect.verticalNormalizedPosition + ((
                selection.transform.position.y -
                transform.position.y
            ) / 3.2F);
             // Guess who figured out the magic number by guessing
            // it's ya boi, comin atcha
        }
        prevSelection = selection;
    }
}
