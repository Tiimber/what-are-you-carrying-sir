using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectUI : MonoBehaviour, IPubSub {

    // Bottom right
    public InspectUIButtonParent objectOk;
    // Top right
    public InspectUIButtonParent throwAway;
    // Top left
    public InspectUIButtonParent manualInspect;
    // Bottom left
    public InspectUIButtonParent callPolice;

    private Vector3 objectOffsetLeft = Vector3.left * 9f;
    private Vector3 objectOffsetRight = Vector3.right * 9f;

    public static InspectUI instance;

    // Use this for initialization
	void Start () {
		PubSub.subscribe("inspect_active", this);
		PubSub.subscribe("inspect_inactive", this);
        InspectUI.instance = this;
	}

	// Update is called once per frame
	void Update () {

	}

	public PROPAGATION onMessage(string message, object data) {
        if (message == "inspect_active") {
            animateTrashcan();
            animateOK();
            if (BagHandler.instance.allowManualInspectOnCurrentBag()) {
                animateManualInspect();
            }
            animatePolice();
        } else if (message == "inspect_inactive") {
            animateTrashcan(false);
            animateOK(false);
            if (BagHandler.instance.allowManualInspectOnCurrentBag()) {
                animateManualInspect(false);
            }
            animatePolice(false);
        }
		return PROPAGATION.DEFAULT;
	}

    private void animateInUiObj (InspectUIButtonParent obj, Vector3 offset, string animateKey) {
        // Safety check - are we currently animating the UI?
        bool animatingUiInProgress = Misc.IsAnimationActive("inspect_police");
        if (animatingUiInProgress) {
            // UI is animating, and a will be hidden after a delay, cancel these delays
            cancelUiDelay();
        }

        Vector3 currentButtonTargetPosition = obj.child.getTargetPosition();
        obj.gameObject.SetActive(true);
        obj.transform.position = currentButtonTargetPosition + offset;
        Debug.Log(animateKey + ": " + obj.transform.position + " - " + currentButtonTargetPosition);
        Misc.AnimateMovementTo(animateKey, obj.gameObject, currentButtonTargetPosition);
    }

    private Dictionary<string, Coroutine> currentUiDelays = new Dictionary<string, Coroutine>();
    private void animateOutUiObj (InspectUIButtonParent obj, Vector3 offset, string animateKey) {
        Vector3 throwAwayTargetPosition = obj.child.getTargetPosition() + offset;
        Misc.AnimateMovementTo(animateKey, obj.gameObject, throwAwayTargetPosition);
        currentUiDelays.Add(animateKey, StartCoroutine(setActiveUIButtonAfterDelay(obj.gameObject, false, animateKey)));
    }

    private void animateTrashcan (bool animateIn = true) {
        if (animateIn) {
            animateInUiObj(throwAway, objectOffsetRight, "inspect_throw_away");
        } else {
            animateOutUiObj(throwAway, objectOffsetRight, "inspect_throw_away");
        }
    }

    private void animateOK (bool animateIn = true) {
        if (animateIn) {
            animateInUiObj(objectOk, objectOffsetRight, "inspect_ok");
        } else {
            animateOutUiObj(objectOk, objectOffsetRight, "inspect_ok");
        }
    }

    private void animateManualInspect (bool animateIn = true) {
        if (animateIn) {
            UIButtonManualInspectLogic manualInspectLogic = manualInspect.GetComponent<UIButtonManualInspectLogic>();
            manualInspectLogic.showCorrectButtons(BagHandler.instance.allowNewTrayForBagContent());
            animateInUiObj(manualInspect, objectOffsetLeft, "inspect_manual");
        } else {
            animateOutUiObj(manualInspect, objectOffsetLeft, "inspect_manual");
        }
    }

    private void animatePolice (bool animateIn = true) {
        if (animateIn) {
            animateInUiObj(callPolice, objectOffsetLeft, "inspect_police");
        } else {
            animateOutUiObj(callPolice, objectOffsetLeft, "inspect_police");
        }
    }

    private IEnumerator setActiveUIButtonAfterDelay(GameObject gameObject, bool activeState, string animateKey, float time = Misc.DEFAULT_ANIMATION_TIME) {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(activeState);
        currentUiDelays.Remove(animateKey);
    }

    private void cancelUiDelay () {
        foreach (Coroutine uiDelayCoroutine in currentUiDelays.Values) {
            StopCoroutine(uiDelayCoroutine);
        }
        currentUiDelays.Clear();
    }

}
