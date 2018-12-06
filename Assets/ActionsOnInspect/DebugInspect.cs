using UnityEngine;

public class DebugInspect : MonoBehaviour {

    void Start () {
        ActionOnInspect[] actionsOnInspect = GetComponents<ActionOnInspect>();
        foreach (ActionOnInspect actionOnInspect in actionsOnInspect) {
            actionOnInspect.run();
        }
    }

}
