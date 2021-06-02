using UnityEngine;

using System.Collections.Generic;

public class GameBehaviour : MonoBehaviour {
    [HideInInspector]
    public bool useUnscaledDeltaTime = true;
    public static HashSet<GameBehaviour> allGameBehvaiours = new HashSet<GameBehaviour>();

    public virtual void Awake() { allGameBehvaiours.Add(this); }
    public virtual void OnDestroy() { allGameBehvaiours.Remove(this); }

    public virtual void UpdateDelta(float modDeltaTime) { }
    public virtual void LateUpdateDelta(float modDeltaTime) { }
}