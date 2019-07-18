using UnityEngine;
using UnityEngine.Events;

public class ToggleButton : MonoBehaviour {

    public bool pressed = true;
    public UnityEvent trigger;
    public Light light;
    public float xPressed;
    public float xUnpressed;
    public string buttonType;

    public void toggle() {
        trigger.Invoke();
        changeState();
    }

    void changeState() {
        pressed = !pressed;
        Misc.AnimateMovementTo("xray-button-press-" + buttonType, this.gameObject, new Vector3(pressed ? xPressed : xUnpressed, this.transform.localPosition.y, this.transform.localPosition.z));
    }

}
