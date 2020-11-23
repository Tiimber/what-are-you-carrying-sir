using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;

public class Game : MonoBehaviour, IPubSub {

    const float CONVEYOR_SPEED = 0.15f;
//    const float CONVEYOR_SPEED = 2.0f;
	const float CONVEYOR_BACK_PCT_SPEED = 0.8f;

    private const float CLICK_RELEASE_TIME = 0.2f;
    private const float THRESHOLD_MAX_MOVE_TO_BE_CONSIDERED_CLICK = 30f;

    private const float TV_SCROLL_MOVEMENT_SPEED = 7f;

    private List<XmlDocument> peopleConfigs = new List<XmlDocument>();
    private List<Texture2D> passportTextures = new List<Texture2D>();
    
    private BagHandler bagHandler;
    public Person personPrefab;
    public WalkingMan walkingMan;
	public GameObject[] xrayMachines;

    public Camera gameCamera;
    public Camera inspectCamera;
    public Camera blurCamera;
    public Camera tvCamera;
    public LoudspeakerLogic loudspeaker;
    public Room room;

    public TVLogic tvLogic;
    public TVContentSet startMenuTVContent;
    public TVContentSet pauseTVContent;
    public Material blankScreen;

    public static Game instance;

    public bool zoomedOutState = true;
    public int cameraXPos = 1;

	public XRayMachine currentXrayMachine;

    private Dictionary<string, int> mistakeSeverity = new Dictionary<string, int>();

    bool enableLongPress = false;
    float leftClickReleaseTimer = 0f;
    Vector3 mouseDownPosition;
    Vector3 prevMousePosition;

    public GameObject[] gameObjectsToPreload;
    public GameObject preloadContainer;
    private static Vector3 TINY_INSTANTIATING_SCALE = new Vector3(0.01f, 0.01f, 0.01f);

    TKSwipeRecognizer swipeRecognizer;
    TKTapRecognizer twoTapRecognizer; // TODO - Only for debug
    TKTapRecognizer tapRecognizer;
    TKContinousHoldRecognizer continousHoldRecognizer;

    public static bool paused = false;
    public int points;
    
    private List<Person> allPeople = new List<Person>();

    enum Direction {
        UP,
        DOWN,
        LEFT,
        RIGHT,

        NONE
    }

    void Awake () {
        ItsRandom.setRandomSeed(1234567890, "bags");
        ItsRandom.setRandomSeed(987654321, "people");
        
        // "Download" people config - TODO - this should be done elsewhere, preloaded per mission 
        string peopleConfigUrl = "http://samlingar.com/whatareyoucarryingsir/example-people-config-2.xml";
        StartCoroutine (loadPeopleConfig (peopleConfigUrl));

        // Set framerate only for editor - Should do based on device later?!
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

        pauseGame(true);
    }

    // Use this for initialization
	void Start () {
        bagHandler = GetComponent<BagHandler>();

		// TODO - Pick xrayMachine depending on level instead...
		GameObject xrayMachineGameObject = Instantiate(xrayMachines[0], xrayMachines[0].transform.position, Quaternion.identity);
		currentXrayMachine = xrayMachineGameObject.GetComponent<XRayMachine> ();
        currentXrayMachine.attachConnectingConveyors();
        room.GetComponent<Room>().setLocation("se", "mma");

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

/*
#if UNITY_IOS
        Input.simulateMouseWithTouches = false;
#endif
#if UNITY_IOS || UNITY_ANDROID
        tapRecognizer.numberOfTouchesRequired = 1;
        tapRecognizer.gestureRecognizedEvent += tapDetected;
        TouchKit.addGestureRecognizer(tapRecognizer);
#endif
*/

        continousHoldRecognizer.gestureHoldingEvent += touchHoldActive;
        continousHoldRecognizer.gestureHoldingEventEnded += touchHoldEnded;
        TouchKit.addGestureRecognizer(continousHoldRecognizer);

        StartCoroutine(preloadGameObjects());
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
        if (!Game.paused) {
            PubSub.publish("Click", r.touchLocation());
        } else {
            PubSub.publish ("ClickTV", r.touchLocation());
        }
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

    public void removePerson(Person person) {
        allPeople.Remove(person);
    }
    
    private void createNewPerson() {

        Vector3 startPoint = currentXrayMachine.bagDropPoint + currentXrayMachine.transform.position;
        startPoint.z = 5;
        Person newPerson = Instantiate(personPrefab, startPoint, Quaternion.identity);
        newPerson.setConfig(getNextPersonConfig());
        newPerson.greetingPositionX = currentXrayMachine.scanRight;

        float walkingManStartPositionRelativePersonCube = 20f;
        Vector3 walkingManStartPoint = new Vector3(startPoint.x - walkingManStartPositionRelativePersonCube, 0, startPoint.z + 12);
        WalkingMan newWalkingMan = Instantiate(walkingMan, walkingManStartPoint, walkingMan.transform.rotation);
        newWalkingMan.person = newPerson;
        newPerson.walkingMan = newWalkingMan;
        newPerson.soundObject = newWalkingMan.spawningSoundObject;

        Vector3 bagDropPositionRelativeXrayMachine = currentXrayMachine.bagDropPoint;
        Vector3 bagDropPosition = Misc.getWorldPosForParentRelativePos(bagDropPositionRelativeXrayMachine, currentXrayMachine.transform);

        newPerson.startPlaceBags(bagHandler, bagDropPosition);
        
        allPeople.Add(newPerson);
    }

    public void pauseGame (bool gameStart = false) {
        Game.paused = !Game.paused;
        if (Game.paused && !gameStart) {
            tvLogic.setCurrentContent(pauseTVContent);
        } else if (gameStart) {
            tvLogic.setCurrentContent(startMenuTVContent);
        }
        PubSub.publish("pause", Game.paused);
        animateCameraChange();
    }

    public void pauseGameClickedTVStart (TVCamera tvCamera) {
        if (tvCamera.isOn()) {
            pauseGame();
        }
    }

    private void scrollTV (Direction dir) {
        float amount = 0f;
        if (dir == Direction.UP) {
            amount = TV_SCROLL_MOVEMENT_SPEED;
        } else if (dir == Direction.DOWN) {
            amount = -TV_SCROLL_MOVEMENT_SPEED;
        }
        if (amount != 0f) {
            PubSub.publish("scrollvertical", amount);
        }
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
                    } else if (Game.paused) {
                        scrollTV(Direction.UP);
                    }
                    break;
                case Direction.DOWN:
                    if (!zoomedOutState && canMoveCamera()) {
                        zoomedOutState = true;
                        animateCameraChange();
                    } else if (Game.paused) {
                        scrollTV(Direction.DOWN);
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
            // If this is the first frame we pressed space or shift - apply teddybear force
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Space)) {
                applyTeddybearForce(CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED);
            }
		} else if (Input.GetKey(KeyCode.Space)) {
            // Forwards
            BagHandler.instance.moveConveyor(CONVEYOR_SPEED, currentXrayMachine);
            // If this is the first frame we pressed space or we let go of shift - apply teddybear force
            if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.Space)) {
                applyTeddybearForce(-CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED);
            }
		}

		// Create a new bag - disable lid
		if (!Game.paused && Input.GetKeyDown (KeyCode.Alpha1)) {
            createNewPerson();
        } else if (Input.GetKeyDown(KeyCode.Escape)) {
            pauseGame();
		} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (!Game.paused) {
                moveCamera(Direction.UP);
            } else {
                scrollTV(Direction.UP);
            }
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (!Game.paused) {
                moveCamera(Direction.DOWN);
            } else {
                scrollTV(Direction.DOWN);
            }
		} else if (!Game.paused && Input.GetKeyDown(KeyCode.LeftArrow)) {
            moveCamera(Direction.LEFT);
		} else if (!Game.paused && Input.GetKeyDown(KeyCode.RightArrow)) {
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
                if (!Game.paused) {
                    PubSub.publish ("Click", mouseDownPosition);
                } else {
                    PubSub.publish ("ClickTV", mouseDownPosition);
                }
                leftClickReleaseTimer = 0f;
            }
        }

	}

    private bool canMoveCamera () {
        return !Game.paused && bagHandler.bagInspectState == BagHandler.BagInspectState.NOTHING;
    }

    private void animateCameraChange() {
        float zoomTo;
        Vector3 moveTo;
        Vector3 rotateToVector;
        if (Game.paused) {
            zoomTo = room.pauseScreenZoom;
            moveTo = room.pauseScreenPos;
            rotateToVector = room.pauseScreenRotation;
        } else {
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
        }
        Quaternion rotateTo = Quaternion.Euler(rotateToVector);
        CameraHandler.ZoomPerspectiveCameraTo(zoomTo);
        CameraHandler.MovePerspectiveCameraTo(moveTo);
        CameraHandler.RotatePerspectiveCameraTo(rotateTo);
        StartCoroutine(publishCameraMovementAfterDelay(Misc.DEFAULT_ANIMATION_TIME));
        if (!zoomedOutState && cameraXPos == 2) {
//            CameraHandler.SetGyroEnabledAfterDelay();
        }
    }

    private System.Collections.IEnumerator publishCameraMovementAfterDelay(float delay) {
        PubSub.publish("CameraMovementStarted");
        yield return new WaitForSeconds(delay);
        PubSub.publish("CameraMovementFinished");
    }

/*
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
*/

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

    public void registerMistake(string mistake, bool shouldPlaySpeakerSound = false) {
        Debug.Log("Register mistake: " + mistake);
        
        // TODO - Do something the other mistakes, that doesn't result in a speaker sound (warning, false arrest)

        if (!mistakeSeverity.ContainsKey(mistake)) {
            mistakeSeverity.Add(mistake, 0);
        }
        mistakeSeverity[mistake]++;
        if (shouldPlaySpeakerSound) {
            loudspeaker.putMessageOnQueue(mistake, mistakeSeverity[mistake], ItsRandom.randomRange(7f, 15f));
        }
        
        // Refresh score screen
        pauseTVContent.GetComponent<ScoreScreen>().update(mistakeSeverity, points);
    }

    private System.Collections.IEnumerator preloadGameObjects() {
        yield return null;
        List<GameObject> instantiatedPreloads = new List<GameObject>();
        foreach (GameObject preload in gameObjectsToPreload) {
            GameObject instantiated = Instantiate(preload, preloadContainer.transform);
            instantiated.transform.localScale = TINY_INSTANTIATING_SCALE;
            instantiatedPreloads.Add(instantiated);
            yield return null;
        }
        yield return null;
        foreach(GameObject instantiated in instantiatedPreloads) {
            Destroy(instantiated);
        }
    }

    private IEnumerator loadPeopleConfig(string peopleConfigUrl) {
        WWW www = CacheWWW.Get(peopleConfigUrl);
        yield return www;
        XmlDocument xmlDoc = new XmlDocument ();
        xmlDoc.LoadXml (www.text);

        XmlNodeList people = xmlDoc.SelectNodes("/people/person");
        foreach (XmlNode person in people) {
            string personUrl = Misc.xmlString(person.Attributes.GetNamedItem("href"));
            yield return loadPersonConfig(personUrl);
        }
    }

    private IEnumerator loadPersonConfig (string personConfigUrl) {
        WWW www = CacheWWW.Get(personConfigUrl);
        yield return www;
        XmlDocument xmlDoc = new XmlDocument ();
        xmlDoc.LoadXml (www.text);
        
        // Load Texture for passport photo that belongs to this config
        string photoUrl = Misc.xmlString(xmlDoc.SelectSingleNode("/person").Attributes.GetNamedItem("photo"));
        WWW photoWWW = CacheWWW.Get(photoUrl);
        yield return photoWWW;
        Texture2D passportTexture = new Texture2D (350, 389);
        photoWWW.LoadImageIntoTexture (passportTexture);

        peopleConfigs.Add(xmlDoc);
        passportTextures.Add(passportTexture);
    }

    private Tuple2<XmlDocument, Texture2D> getNextPersonConfig() {
        // TODO - pop lists
        int randomPersonIndex = ItsRandom.randomRange(0, peopleConfigs.Count);
        return new Tuple2<XmlDocument, Texture2D>(peopleConfigs[randomPersonIndex], passportTextures[randomPersonIndex]);
    }

    private void applyTeddybearForce(float force) {
        foreach (Person person in allPeople) {
            if (!person.isDestroyed) {
                person.applyTeddybearForce(force);
            }
        }
    }
}