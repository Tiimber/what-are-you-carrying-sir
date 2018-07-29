using System;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour, IPubSub {

//    const float CONVEYOR_SPEED = 0.005f;
    const float CONVEYOR_SPEED = 0.1f;
	const float CONVEYOR_BACK_PCT_SPEED = 0.8f;

    private const float CLICK_RELEASE_TIME = 0.2f;
    private const float THRESHOLD_MAX_MOVE_TO_BE_CONSIDERED_CLICK = 30f;

    private BagHandler bagHandler;
	public GameObject[] xrayMachines;

    public Camera gameCamera;
    public Camera inspectCamera;
    public Camera blurCamera;

    public static Game instance;

    public bool zoomedOutState = true;
    public int cameraXPos = 1;

	private XRayMachine currentXrayMachine;

    bool enableLongPress = false;
    float leftClickReleaseTimer = 0f;
    Vector3 mouseDownPosition;
    Vector3 prevMousePosition;

    void Awake () {
        Game.instance = this;
        CameraHandler.SetPerspectiveCamera(gameCamera);

        // Last in line for click triggering
        PubSub.subscribe("Click", this, Int32.MaxValue);
    }

    // Use this for initialization
	void Start () {
        bagHandler = GetComponent<BagHandler>();

		// TODO - Pick xrayMachine depending on level instead...
		GameObject xrayMachineGameObject = Instantiate(xrayMachines[0], xrayMachines[0].transform.position, Quaternion.identity);
		currentXrayMachine = xrayMachineGameObject.GetComponent<XRayMachine> ();
        currentXrayMachine.attachConenctingConveyors();
	}

	// Update is called once per frame
	void Update () {
		if ((Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKey (KeyCode.Space)) {
            // Backwards
            if (BagHandler.instance.bagInspectState == BagHandler.BagInspectState.NOTHING) {
                List<GameObject> bags = Misc.FindShallowStartsWith("Bag_");
                foreach (GameObject bag in bags) {
                    if (bag.GetComponent<BagProperties>().isOnConveyor) {
                        bag.transform.position = new Vector3(bag.transform.position.x - CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED, bag.transform.position.y, bag.transform.position.z);
                    }
                }
            }
		} else if (Input.GetKey(KeyCode.Space)) {
            // Forwards
            if (BagHandler.instance.bagInspectState == BagHandler.BagInspectState.NOTHING) {
                List<GameObject> bags = Misc.FindShallowStartsWith ("Bag_");
                foreach (GameObject bag in bags) {
                    BagProperties bagProperties = bag.GetComponent<BagProperties>();
                    if (bagProperties.isOnConveyor) {
                        float bagNewXPos = bag.transform.position.x + CONVEYOR_SPEED;
                        if (bagNewXPos > currentXrayMachine.xPointOfNoReturn) {
                            bagProperties.bagFinished();
                        } else {
                            bag.transform.position = new Vector3(bagNewXPos, bag.transform.position.y, bag.transform.position.z);
                        }
                    }
                }
            }
		}

		// Create a new bag - disable lid
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			Vector3 bagDropPositionRelativeXrayMachine = currentXrayMachine.bagDropPoint;
			Vector3 bagDropPosition = Misc.getWorldPosForParentRelativePos (bagDropPositionRelativeXrayMachine, currentXrayMachine.transform);

            bagHandler.createBag(bagDropPosition);
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
            bagHandler.placeItems();
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
            bagHandler.shuffleBag();
		} else if (Input.GetKeyDown (KeyCode.Alpha4)) {
            bagHandler.closeLid();
		} else if (Input.GetKeyDown (KeyCode.Alpha5)) {
            bagHandler.dropBag();
		} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (zoomedOutState && canMoveCamera()) {
                zoomedOutState = false;
                animateCameraChange();
            }
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (!zoomedOutState && canMoveCamera()) {
               zoomedOutState = true;
                animateCameraChange();
            }
		} else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (cameraXPos != 0 && canMoveCamera()) {
                cameraXPos--;
                animateCameraChange();
            }
		} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (cameraXPos != 2 && canMoveCamera()) {
                cameraXPos++;
                animateCameraChange();
            }
        }

        // Left mouse button
        if (Input.GetMouseButton (0)) {
            // Drag logic
            bool firstFrame = Input.GetMouseButtonDown (0);
            Vector3 mousePosition = Input.mousePosition;

            if (!firstFrame) {
                Vector3 diffMove = mousePosition - prevMousePosition;
                if (BagHandler.instance.bagInspectState == BagHandler.BagInspectState.ITEM_INSPECT) {
                    PubSub.publish("inspect_rotate", diffMove);
                }
                // More "mouse drag" actions when needed...
            } else {
                mouseDownPosition = mousePosition;
            }
            prevMousePosition = mousePosition;

            // Click logic
            if (firstFrame) {
                leftClickReleaseTimer = CLICK_RELEASE_TIME;
                enableLongPress = true;
            } else {
                leftClickReleaseTimer -= Time.deltaTime;
            }

//            if (leftClickReleaseTimer < -1f && enableLongPress) {
//                // Debug.Log("Trigger long press");
//                enableLongPress = false;
//                PubSub.publish("LongPress", mouseDownPosition);
//            }
        } else if (leftClickReleaseTimer > 0f) {
            // Button not pressed, and was pressed < 0.2s, accept as click if not moved too much
            if (Misc.getDistance (mouseDownPosition, prevMousePosition) < THRESHOLD_MAX_MOVE_TO_BE_CONSIDERED_CLICK) {
                PubSub.publish ("Click", mouseDownPosition);
                leftClickReleaseTimer = 0f;
            }
        }

	}

    private bool canMoveCamera () {
        return bagHandler.bagInspectState == BagHandler.BagInspectState.NOTHING;
    }

    private void animateCameraChange() {
        float zoomTo;
        Vector3 moveTo;
        Vector3 rotateToVector;
        switch (cameraXPos) {
            case 0:
            	zoomTo = zoomedOutState ? currentXrayMachine.dropPointZoomOutZoom : currentXrayMachine.dropPointZoomInZoom;
                moveTo = zoomedOutState ? currentXrayMachine.dropPointZoomOutPos : currentXrayMachine.dropPointZoomInPos;
                rotateToVector = zoomedOutState ? currentXrayMachine.dropPointZoomOutRotation : currentXrayMachine.dropPointZoomInRotation;
            	break;
            case 2:
            	zoomTo = zoomedOutState ? currentXrayMachine.checkPointZoomOutZoom : currentXrayMachine.checkPointZoomInZoom;
                moveTo = zoomedOutState ? currentXrayMachine.checkPointZoomOutPos : currentXrayMachine.checkPointZoomInPos;
                rotateToVector = zoomedOutState ? currentXrayMachine.checkPointZoomOutRotation : currentXrayMachine.checkPointZoomInRotation;
            	break;
            case 1:
			default:
            	zoomTo = zoomedOutState ? currentXrayMachine.scanPointZoomOutZoom : currentXrayMachine.scanPointZoomInZoom;
                moveTo = zoomedOutState ? currentXrayMachine.scanPointZoomOutPos : currentXrayMachine.scanPointZoomInPos;
                rotateToVector = zoomedOutState ? currentXrayMachine.scanPointZoomOutRotation : currentXrayMachine.scanPointZoomInRotation;
            	break;
        }
        Quaternion rotateTo = Quaternion.Euler(rotateToVector);
        CameraHandler.ZoomPerspectiveCameraTo(zoomTo);
        CameraHandler.MovePerspectiveCameraTo(moveTo);
        CameraHandler.RotatePerspectiveCameraTo(rotateTo);
    }

    private void moveCameraToXPos() {
		switch (cameraXPos) {
            case 0:
                CameraHandler.MovePerspectiveCameraTo(new Vector3(-1.17f, 0.99f, -10f));
	            break;
            case 2:
                CameraHandler.MovePerspectiveCameraTo(new Vector3(1.17f, 0.99f, -10f));
	            break;
            case 1:
            default:
                CameraHandler.MovePerspectiveCameraTo(new Vector3(0f, 0.99f, -10f));
	            break;
        }
	}

    public PROPAGATION onMessage(string message, object data) {
        if (message == "Click") {
            // No other subscriber have captured and kept this click for itself, raytrace the click, triggering any possible buttons beneath it
            Vector3 position = (Vector3) data;

            RaycastHit hit;
            Ray ray = (BagHandler.instance.bagInspectState == BagHandler.BagInspectState.ITEM_INSPECT ? inspectCamera : gameCamera).ScreenPointToRay(position);

            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.isTrigger) {
                    InspectUIButton inspectUIButton = hit.transform.GetComponent<InspectUIButton>();
                    if (inspectUIButton) {
                        inspectUIButton.trigger();
                    }
                }
            }
        }
        return PROPAGATION.DEFAULT;
    }

    public Vector3 getTrayDropPosition () {
        Vector3 dropPositionRelativeXrayMachine = new Vector3(currentXrayMachine.xPointOfTrayInsertion, currentXrayMachine.bagDropPoint.y, currentXrayMachine.bagDropPoint.z);
        Vector3 dropPosition = Misc.getWorldPosForParentRelativePos (dropPositionRelativeXrayMachine, currentXrayMachine.transform);
        return dropPosition;
    }
}