using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemPreventDeselect : MonoBehaviour {
    GameObject prevSelection;

    void Update() {
        GameObject selection = EventSystem.current.currentSelectedGameObject;
        if (selection == null) {
            EventSystem.current.SetSelectedGameObject(prevSelection);
            selection = prevSelection;
        }
        prevSelection = selection;
    }
}