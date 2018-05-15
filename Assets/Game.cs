using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

//    const float CONVEYOR_SPEED = 0.005f;
    const float CONVEYOR_SPEED = 0.1f;
	const float CONVEYOR_BACK_PCT_SPEED = 0.8f;

    private BagHandler bagHandler;
	public GameObject[] xrayMachines;

    public Camera gameCamera;
    public Camera inspectCamera;
    public Camera blurCamera;

    public static Game instance;

    private bool zoomedOutState = true;
    private int cameraXPos = 1;

	private XRayMachine currentXrayMachine;

    void Awake () {
        Game.instance = this;
        CameraHandler.SetPerspectiveCamera(gameCamera);
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
			List<GameObject> bags = Misc.FindShallowStartsWith ("Bag_");
			foreach (GameObject bag in bags) {
                if (bag.GetComponent<BagProperties>().isOnConveyor) {
                    bag.transform.position = new Vector3 (bag.transform.position.x - CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED, bag.transform.position.y, bag.transform.position.z);
                }
			}
		} else if (Input.GetKey(KeyCode.Space)) {
			List<GameObject> bags = Misc.FindShallowStartsWith ("Bag_");
			foreach (GameObject bag in bags) {
                if (bag.GetComponent<BagProperties>().isOnConveyor) {
					bag.transform.position = new Vector3(bag.transform.position.x + CONVEYOR_SPEED, bag.transform.position.y, bag.transform.position.z);
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

        if (Input.GetMouseButtonDown(0) && cameraXPos == 2 && !zoomedOutState) {
            // TODO !!! click bag to open it, then enter "bag inspect mode". When click object, switch to inspector camera and move object towards camera
            Vector3 mousePosition = Input.mousePosition;
            PubSub.publish ("Click", mousePosition);
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
}