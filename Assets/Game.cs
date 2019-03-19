using System;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour, IPubSub {

//    const float CONVEYOR_SPEED = 0.2f;
    const float CONVEYOR_SPEED = 2.0f;
	const float CONVEYOR_BACK_PCT_SPEED = 0.8f;

    private const float CLICK_RELEASE_TIME = 0.2f;
    private const float THRESHOLD_MAX_MOVE_TO_BE_CONSIDERED_CLICK = 30f;

    private BagHandler bagHandler;
    public Person personPrefab;
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

    TKSwipeRecognizer swipeRecognizer;
    TKTapRecognizer twoTapRecognizer; // TODO - Only for debug
    TKTapRecognizer tapRecognizer;
    TKContinousHoldRecognizer continousHoldRecognizer;

    enum Direction {
        UP,
        DOWN,
        LEFT,
        RIGHT,

        NONE
    }

    void Awake () {

        // Set framerate only for edito - Should do based on device later?!
//#if UNITY_EDITOR
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 90;
//#endif

        Game.instance = this;
        CameraHandler.SetPerspectiveCamera(gameCamera);

        swipeRecognizer = new TKSwipeRecognizer();
        twoTapRecognizer = new TKTapRecognizer();
        tapRecognizer = new TKTapRecognizer();
        continousHoldRecognizer = new TKContinousHoldRecognizer();

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

//        // when using in conjunction with a pinch or rotation recognizer setting the min touches to 2 smoothes movement greatly
//        if( Application.platform == RuntimePlatform.IPhonePlayer ) {
//            recognizer.minimumNumberOfTouches = 2;
//        }

        // continuous gestures have a complete event so that we know when they are done recognizing
        swipeRecognizer.gestureRecognizedEvent += swipeGestureDetected;
		TouchKit.addGestureRecognizer(swipeRecognizer);

        twoTapRecognizer.numberOfTouchesRequired = 2;
        twoTapRecognizer.gestureRecognizedEvent += twoTapDetected;
		TouchKit.addGestureRecognizer(twoTapRecognizer);

#if UNITY_IOS || UNITY_ANDROID
        tapRecognizer.numberOfTouchesRequired = 1;
        tapRecognizer.gestureRecognizedEvent += tapDetected;
        TouchKit.addGestureRecognizer(tapRecognizer);
#endif

        continousHoldRecognizer.gestureHoldingEvent += touchHoldActive;
        continousHoldRecognizer.gestureHoldingEventEnded += touchHoldEnded;
        TouchKit.addGestureRecognizer(continousHoldRecognizer);
    }

    void touchHoldEnded(TKContinousHoldRecognizer r) {
        if (r.touchCount == 1) {
            BagHandler.instance.zoomInspectItem(false);
        }
    }

    void touchHoldActive(TKContinousHoldRecognizer r) {
        if (r.touchCount == 2) {
            BagHandler.instance.moveConveyor(- CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED, currentXrayMachine);
        } else if (r.touchCount == 1) {
            BagHandler.instance.moveConveyor(CONVEYOR_SPEED, currentXrayMachine);
            if (r.firstFrame) {
                BagHandler.instance.zoomInspectItem(true, r.maxTouchMovement);
            } else {
                BagHandler.instance.potentiallyStopZoomInspectItem(r.maxTouchMovement);
            }
        }
    }

    void tapDetected(TKTapRecognizer r) {
        PubSub.publish("Click", r.touchLocation());
    }

    void twoTapDetected(TKTapRecognizer r) {
        createNewPerson();
    }

    void swipeGestureDetected (TKSwipeRecognizer r) {
        TKSwipeDirection direction = r.completedSwipeDirection;
        switch (direction) {
            case TKSwipeDirection.Up:
                moveCamera(Direction.UP);
                break;
            case TKSwipeDirection.Down:
                moveCamera(Direction.DOWN);
                break;
            case TKSwipeDirection.Left:
                moveCamera(Direction.RIGHT);
                break;
            case TKSwipeDirection.Right:
                moveCamera(Direction.LEFT);
                break;
            case TKSwipeDirection.UpLeft:
                moveCamera(Direction.UP, Direction.RIGHT);
                break;
            case TKSwipeDirection.UpRight:
                moveCamera(Direction.UP, Direction.LEFT);
                break;
            case TKSwipeDirection.DownLeft:
                moveCamera(Direction.DOWN, Direction.RIGHT);
                break;
            case TKSwipeDirection.DownRight:
                moveCamera(Direction.DOWN, Direction.LEFT);
                break;
            default:
                break;
        }
//        Debug.Log("swipe gesture complete: " + direction);
    }

    private void createNewPerson() {

        Vector3 startPoint = currentXrayMachine.bagDropPoint + currentXrayMachine.transform.position;
        startPoint.z = 5;
        Person newPerson = Instantiate(personPrefab, startPoint, Quaternion.identity);
        newPerson.greetingPositionX = currentXrayMachine.scanRight;

        Vector3 bagDropPositionRelativeXrayMachine = currentXrayMachine.bagDropPoint;
        Vector3 bagDropPosition = Misc.getWorldPosForParentRelativePos(bagDropPositionRelativeXrayMachine, currentXrayMachine.transform);

        newPerson.startPlaceBags(bagHandler, bagDropPosition);
    }

    private void moveCamera (Direction dir1, Direction dir2 = Direction.NONE) {
        List<Direction> directions = new List<Direction>(){
            dir1
        };
        if (dir2 != Direction.NONE) {
            directions.Add(dir2);
        }

        foreach (Direction dir in directions) {
            switch (dir) {
                case Direction.UP:
                    if (zoomedOutState && canMoveCamera()) {
                        zoomedOutState = false;
                        animateCameraChange();
                    }
                    break;
                case Direction.DOWN:
                    if (!zoomedOutState && canMoveCamera()) {
                        zoomedOutState = true;
                        animateCameraChange();
                    }
                    break;
                case Direction.LEFT:
                    if (cameraXPos != 0 && canMoveCamera()) {
                        cameraXPos--;
                        animateCameraChange();
                    }
                    break;
                case Direction.RIGHT:
                    if (cameraXPos != 2 && canMoveCamera()) {
                        cameraXPos++;
                        animateCameraChange();
                    }
                    break;
                case Direction.NONE:
                default:
                    break;
            }
        }
    }

	// Update is called once per frame
	void Update () {
		if ((Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKey (KeyCode.Space)) {
            // Backwards
            BagHandler.instance.moveConveyor(- CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED, currentXrayMachine);
		} else if (Input.GetKey(KeyCode.Space)) {
            // Forwards
            BagHandler.instance.moveConveyor(CONVEYOR_SPEED, currentXrayMachine);
		}

		// Create a new bag - disable lid
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
            createNewPerson();
		} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            moveCamera(Direction.UP);
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            moveCamera(Direction.DOWN);
		} else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            moveCamera(Direction.LEFT);
		} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            moveCamera(Direction.RIGHT);
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
        if (!zoomedOutState && cameraXPos == 2) {
//            CameraHandler.SetGyroEnabledAfterDelay();
        }
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
            Vector3 position = data.GetType() == typeof(Vector2) ? new Vector3(((Vector2)data).x, ((Vector2)data).y) : (Vector3) data;
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