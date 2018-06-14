using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectUI : MonoBehaviour, IPubSub {

    // Bottom right
    public GameObject objectOk;
    // Top right
    public GameObject throwAway;
    // Top left
    public GameObject manualInspect;
    // Bottom left
    public GameObject callPolice;

    private Vector3 objectOffsetLeft = Vector3.left * 0.5f;
    private Vector3 objectOffsetRight = Vector3.right * 0.5f;

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
            animateManualInspect();
            animatePolice();
        } else if (message == "inspect_inactive") {
            animateTrashcan(false);
            animateOK(false);
            animateManualInspect(false);
            animatePolice(false);
        }
		return PROPAGATION.DEFAULT;
	}

    private void animateInUiObj (GameObject obj, Vector3 offset, string animateKey) {
        Vector3 throwAwayTargetPosition = obj.GetComponentInChildren<InspectUIButton>().getTargetPosition();
        obj.transform.position = throwAwayTargetPosition + offset;
        obj.SetActive(true);
        Misc.AnimateMovementTo(animateKey, obj, throwAwayTargetPosition);
    }

    private void animateOutUiObj (GameObject obj, Vector3 offset, string animateKey) {
        Vector3 throwAwayTargetPosition = obj.GetComponentInChildren<InspectUIButton>().getTargetPosition() + offset;
        Misc.AnimateMovementTo(animateKey, obj, throwAwayTargetPosition);
        StartCoroutine(setActiveAfterDelay(obj, false));

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
            animateInUiObj(manualInspect, objectOffsetLeft, "inspect_manual");
        } else {
            animateOutUiObj(manualInspect, objectOffsetLeft, "inspect_manual");
        }
    }

    private void animatePolice(bool animateIn = true) {
        if (animateIn) {
            animateInUiObj(callPolice, objectOffsetLeft, "inspect_police");
        } else {
            animateOutUiObj(callPolice, objectOffsetLeft, "inspect_police");
        }
    }

    private IEnumerator setActiveAfterDelay (GameObject gameObject, bool activeState, float time = Misc.DEFAULT_ANIMATION_TIME) {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(activeState);
    }

}
