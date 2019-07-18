using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BagContentProperties : MonoBehaviour, IPubSub {

    public const float TIME_ANIMATE_TO_DROP_POINT = 0.3f;
    public const float TIME_TO_TARGET_POS = 0.4f;
    private static float ROTATION_SPEED = 25f;

    private const int POINTS_ALL_FINE_ACTION = 1000;

    private static int idCounter = 0;
    public int id;

    private bool inspecting = false;

	public Vector3 objectSize;

    private Vector3 locationInBag;
    private Quaternion rotationInBag;
    private Transform parentBag;

    public InspectUIButton.INSPECT_TYPE[] acceptableActions;

    public InspectUIButton.INSPECT_TYPE actionTaken = InspectUIButton.INSPECT_TYPE.UNDEFINED;
    public int manualInspectTrayNumber;

    public Generic.CONSEQUENCE[] consequences;

    public string displayName;
    public string category;
    public GameObject[] xrayMaterialObjects;

    public static BagContentProperties currentInspectedItem;

    public static int manualInspectTrayCounter = 0;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void Awake() {
        id = ++idCounter;
    }

    private void enableShadows (bool enable = true) {
        foreach (MeshRenderer meshRenderer in gameObject.GetComponentsInChildren<MeshRenderer>()) {
            meshRenderer.shadowCastingMode = enable ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }

    public void inspectDone () {
        PubSub.unsubscribe("inspect_rotate", this);
        PubSub.unsubscribe("inspect_action", this);
        BagContentProperties.currentInspectedItem = null;

        // Special method to run when starting inspection on this object
        ActionOnInspect[] actionOnInspects = GetComponents<ActionOnInspect>();
        foreach (ActionOnInspect actionOnInspect in actionOnInspects) {
            if (actionOnInspect != null) {
                actionOnInspect.run(true);
            }
        }
    }

    public void inspect () {
        PubSub.subscribe("inspect_rotate", this);
        PubSub.subscribe("inspect_action", this);
        locationInBag = this.transform.localPosition;
        rotationInBag = this.transform.localRotation;
        parentBag = this.transform.parent;

        // Turn off casting shadows
        enableShadows(false); // TODO - When action is taken, don't forget to enable shadows again

        // Target object position
        Vector3 targetPosition = Game.instance.gameCamera.transform.position + Game.instance.gameCamera.transform.rotation * (Vector3.forward * 18f);
        this.transform.parent = null;
        Misc.AnimateMovementTo("content-zoom-"+id, this.gameObject, targetPosition);

        // Special method to run when starting inspection on this object
        ActionOnInspect[] actionOnInspects = GetComponents<ActionOnInspect>();
        foreach (ActionOnInspect actionOnInspect in actionOnInspects) {
            Debug.Log(actionOnInspect);
            if (actionOnInspect != null) {
                actionOnInspect.run();
            }
        }

        inspecting = true;
        BagContentProperties.currentInspectedItem = this;
    }

    public void throwAway () {
        inspectDone();
        Misc.AnimateMovementTo("throw_away_move_" + id, this.gameObject, InspectUI.instance.throwAway.transform.localPosition);
        Misc.AnimateScaleTo("throw_away_scale_" + id, this.gameObject, Vector3.zero);
        BagHandler.instance.bagInspectItemEnded();
    }

    public void acceptItem () {
        inspectDone();
        Misc.AnimateMovementTo("ok_move_" + id, this.gameObject, InspectUI.instance.objectOk.transform.localPosition);
        Misc.AnimateScaleTo("ok_scale_" + id, this.gameObject, Vector3.zero);
        BagHandler.instance.bagInspectItemEnded();
    }

    public void sendItemToManualInspect (bool sameTray = true) {
        inspectDone();
        // TODO - Animate to the clicked icon... right now it only looks correct for the big, single button (use the bool in arguments)
        Misc.AnimateMovementTo("manual_inspect_move_" + id, this.gameObject, InspectUI.instance.manualInspect.transform.localPosition);
        Misc.AnimateScaleTo("manual_inspect_scale_" + id, this.gameObject, Vector3.zero);
        BagHandler.instance.bagInspectItemEnded(!sameTray);
    }

    public void callPolice () {
        inspectDone();
        Misc.AnimateMovementTo("call_police_move_" + id, this.gameObject, InspectUI.instance.callPolice.transform.localPosition);
        Misc.AnimateScaleTo("call_police_scale_" + id, this.gameObject, Vector3.zero);
        BagHandler.instance.bagInspectItemEnded();
    }

    public PROPAGATION onMessage(string message, object data) {

        if (message == "inspect_rotate") {
            Vector3 move = (Vector3) data;

            float rotX = move.x * ROTATION_SPEED * Mathf.Deg2Rad;
            float rotY = move.y * ROTATION_SPEED * Mathf.Deg2Rad;

            transform.Rotate(Game.instance.gameCamera.transform.rotation * Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        } else if (message == "inspect_action") {
            InspectUIButton.INSPECT_TYPE action = (InspectUIButton.INSPECT_TYPE) data;
            actionTaken = action;
            PersonBagDefinition currentBagDefinition = BagHandler.instance.currentBagInspect.bagDefinition;

            // Send signal about action taken for this istem
            PubSub.publish("bag_inspect_item", new InspectActionBag(BagHandler.instance.currentBagInspect.id, BagContentProperties.currentInspectedItem, action));

            if (action == InspectUIButton.INSPECT_TYPE.TRASHCAN) {
                currentBagDefinition.removedItems.Add(currentInspectedItem);
                throwAway();
            } else if (action == InspectUIButton.INSPECT_TYPE.OK) {
                acceptItem();
            } else if (action == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT) {
                manualInspectTrayNumber = BagContentProperties.manualInspectTrayCounter;
                sendItemToManualInspect();
            } else if (action == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT_NEW) {
                manualInspectTrayNumber = ++BagContentProperties.manualInspectTrayCounter;
                sendItemToManualInspect(false);
            } else if (action == InspectUIButton.INSPECT_TYPE.POLICE) {
                currentBagDefinition.removedItems.Add(currentInspectedItem);
                callPolice();
            }
        }

        return PROPAGATION.DEFAULT;
    }

    public void animateToBag (float timeToDropPoint = TIME_ANIMATE_TO_DROP_POINT, float timeToTargetPos = TIME_TO_TARGET_POS) {
        transform.SetParent(parentBag);

        Misc.AnimateMovementTo("put_back_move_" + id, this.gameObject, locationInBag + new Vector3(0f, parentBag.GetComponentInParent<BagProperties>().halfBagHeight * 3f, 0f), timeToDropPoint);
        Misc.AnimateRotationTo("put_back_rotate_" + id, this.gameObject, rotationInBag, timeToDropPoint);
        Misc.AnimateScaleTo("put_back_scale_" + id, this.gameObject, Vector3.one, timeToDropPoint);

        StartCoroutine(delayedPutBackToTarget(timeToDropPoint, timeToTargetPos));
    }

    private IEnumerator delayedPutBackToTarget (float delay, float time) {
        yield return new WaitForSeconds(delay);

        Misc.AnimateMovementTo("put_back_move_" + id, this.gameObject, locationInBag, time, straightMotion: true);
    }

    public int calculatePoints () {
        if (actionTaken == InspectUIButton.INSPECT_TYPE.UNDEFINED) {
            actionTaken = InspectUIButton.INSPECT_TYPE.OK;
        }

        if (Array.IndexOf(acceptableActions, actionTaken) != -1) {
            consequences = new Generic.CONSEQUENCE[0];
            return POINTS_ALL_FINE_ACTION;
        }
        return 0;
    }

    public Generic.CONSEQUENCE getConsequence () {
        if (consequences.Length > 0) {
            return consequences[Misc.randomRange(0, consequences.Length)];
        }
        return Generic.CONSEQUENCE.NOTHING;
    }

    public void setVisibleXrayState() {
        Debug.Log("Set Xray stuff");
        foreach (GameObject xrayMaterialObject in xrayMaterialObjects) {
            if (xrayMaterialObject != null) {
                string materialName = xrayMaterialObject.GetComponent<Renderer>().material.name;
                ToggleButtons.ToggleMaterial shaderMaterial = ToggleButtons.GetToggleMaterialForString(materialName);
                xrayMaterialObject.SetActive(ToggleButtons.XRAY_MATERIALS_VISIBLE.Contains(shaderMaterial));
            }
        }
    }
}
