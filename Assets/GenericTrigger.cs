using UnityEngine;
using UnityEngine.Events;

public class GenericTrigger : MonoBehaviour {

    public UnityEvent trigger;

    public void toggle() {
        trigger.Invoke();
    }

}
