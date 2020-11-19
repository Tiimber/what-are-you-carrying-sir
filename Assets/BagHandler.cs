using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class BagHandler : MonoBehaviour, IPubSub {

    public static BagHandler instance;

    public const string RANDOM_TYPE = "bags";

    private BagProperties currentBagPlacing;
    public BagProperties currentBagInspect;

    public GameObject[] bags;
    public GameObject tray;
    public BagContentType[] bagContentTypes;
    public BagContentType[] bagContentTypesIllegal;
    public BagContentType[] bagContentTypesLegal;
    public BagContentType[] bagContentTypesHidden;

    private List<BagProperties> activeBags = new List<BagProperties>();

    public BagInspectState bagInspectState = BagInspectState.NOTHING;

    private static int BAG_CONTENTS_LAYER_MASK = -1;
    private static Vector3 INIT_BAG_POINT = new Vector3(0f, 20f, 100f);

    // TODO - Decide time for calculation cycles
    const float MAX_ITEM_PLACE_CYCLE_SECONDS = 0.016f;

    private GUIStyle currentInspectStateStyle;

    public enum BagInspectState {
        NOTHING,
        BAG_OPEN,
        ITEM_INSPECT,
        BUSY
    }

    // Use this for initialization
	void Start() {
        BagHandler.instance = this;

		PubSub.subscribe("Click", this);

        BAG_CONTENTS_LAYER_MASK = LayerMask.GetMask(new string[]{"BagContentRootObject"});
    }

	// Update is called once per frame
	void Update() {

	}

    private void createStyles () {
        currentInspectStateStyle = new GUIStyle();
        currentInspectStateStyle.alignment = TextAnchor.MiddleCenter;
        currentInspectStateStyle.fontSize = 48;
    }

    void OnGUI() {
        if (currentInspectStateStyle == null) {
            createStyles();
        }

        string bagInspectStateString = null;
        if (bagInspectState == BagInspectState.BAG_OPEN) {
            bagInspectStateString = "Inspecting " + currentBagInspect.bagDisplayName;
        } else if (bagInspectState == BagInspectState.ITEM_INSPECT) {
            bagInspectStateString = "Inspecting \"" + BagContentProperties.currentInspectedItem.displayName + "\" in " + currentBagInspect.bagDisplayName;
        }

        if (bagInspectStateString != null) {
            GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 50), bagInspectStateString, currentInspectStateStyle);

            if (GUI.Button(new Rect(Screen.width / 2f - 50f, Screen.height - 100f, 100f, 50), "Finish bag!")) {
                inspectBagDone();
            }
        }
    }

    public void createBag (Vector3 bagDropPosition) {
        // TODO - Random bag - Some distribution factor? Maybe not plastic tray except for certain content?
        int randomBagIndex = ItsRandom.randomRange(0, bags.Length, RANDOM_TYPE);
        GameObject bagGameObject = Instantiate (bags [randomBagIndex], bagDropPosition, Quaternion.identity);
        BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
        bagGameObject.transform.position = new Vector3 (bagDropPosition.x, bagDropPosition.y + bagProperties.halfBagHeight, bagDropPosition.z);

        currentBagPlacing = bagProperties;
        activeBags.Add(bagProperties);
        if (bagProperties.lid != null) {
            bagProperties.lid.SetActive (false);
        }
    }

    public void createTrayWithContents (Vector3 dropPosition, List<BagContentProperties> items, PersonBagDefinition bagDefinition) {
        if (items != null && items.Count > 0) {
            GameObject bagGameObject = Instantiate (tray, dropPosition, Quaternion.identity);
            BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
            bagGameObject.transform.position = new Vector3 (dropPosition.x, dropPosition.y + bagProperties.halfBagHeight, dropPosition.z);

            currentBagPlacing = bagProperties;
            bagProperties.bagDefinition = bagDefinition;
            bagProperties.bagType = BagProperties.TYPE.TRAY_AFTER_INSPECT;
            bagDefinition.addBag(bagProperties);

            moveBagsAsideForTray (bagGameObject.GetComponent<BagProperties>());
            StartCoroutine (placeItemsInBagAndDrop (currentBagPlacing, items));

            // TODO - Need shuffle

            activeBags.Add(bagProperties);
        }
    }

    public Coroutine placeItems (List<BagContentProperties> toBePlacedInTrays, Person person) {
        return StartCoroutine (placeItemsInBag (currentBagPlacing, 20, toBePlacedInTrays, person));
    }

    public Coroutine shuffleBag () {
        // Shake/rotate
        return StartCoroutine (shakeAndRotateBag(currentBagPlacing));
    }

    public Coroutine closeLid () {
        GameObject lid = currentBagPlacing.lid;
        float bagLidMovement = 2f * currentBagPlacing.halfBagHeight;
        return StartCoroutine (positionLid (lid, bagLidMovement, 0.5f));
        // TODO - 3 - Check collision, remove collision objects, make sure special substances are still in (if any)
    }

    public void dropBag (Vector3 bagDropPosition) {

        if (bagDropPosition != Vector3.zero) {
            currentBagPlacing.transform.position = new Vector3 (bagDropPosition.x, bagDropPosition.y + currentBagPlacing.halfBagHeight, bagDropPosition.z);
        }

        currentBagPlacing.disableInitialColliders ();
        currentBagPlacing.freezeContents();
        currentBagPlacing.setGravity(true);
    }

	public PROPAGATION onMessage(string message, object data) {
		if (message == "Click") {
            if (Game.instance.cameraXPos == 2 && !Game.instance.zoomedOutState) {
                Vector3 position = Vector3.zero;
                if (data.GetType() == typeof(Vector2)) {
                    Vector2 posV2 = (Vector2)data;
                    position = new Vector3(posV2.x, posV2.y);
                } else {
                    position = (Vector3)data;
                }

                // Get camera
                Camera camera = GetComponent<Game>().gameCamera;
                RaycastHit hit;
                Ray ray = camera.ScreenPointToRay(position);

                if (bagInspectState == BagInspectState.NOTHING) {
                    if (Physics.Raycast(ray, out hit)) {
                        Debug.Log(hit.transform.gameObject.name);

                        BagProperties clickedBagProperties = hit.transform.GetComponent<BagProperties>();

//                        // TODO - Can't contents be clicked directly when in a bag without lid?
//                        if (clickedBagProperties == null) {}

                        if (clickedBagProperties != null && !clickedBagProperties.isOpen) {
                            clickedBagProperties.showItems(true);
                            clickedBagProperties.animateLidState(true);
                            clickedBagProperties.enableContentColliders(true);
                            currentBagInspect = clickedBagProperties;
                            bagInspectState = BagInspectState.BAG_OPEN;
                            clickedBagProperties.resetActionOnItems();
                        }
                    }
                } else if (bagInspectState == BagInspectState.BAG_OPEN) {
//                    Debug.Log("Bag is open; " + BAG_CONTENTS_LAYER_MASK);
                    bool isBagEmpty = currentBagInspect.contents.transform.childCount == 0;
                    if (!isBagEmpty) {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity, BAG_CONTENTS_LAYER_MASK)) {
                            Debug.Log(hit.transform.gameObject.name);

                            BagContentProperties clickedBagContentProperties = hit.transform.GetComponent<BagContentProperties>();
                            if (clickedBagContentProperties != null) {

                                switchToGameCamera (true);

                                clickedBagContentProperties.inspect ();
                                Misc.AnimateBlurTo("blurCamera", Game.instance.blurCamera.GetComponent<BlurOptimized>(), 1, 3f, 2);
                                bagInspectState = BagInspectState.ITEM_INSPECT;
                                PubSub.publish("inspect_active");
                                PubSub.publish("bag_inspect_item", new InspectActionBag(currentBagInspect.id, clickedBagContentProperties, InspectUIButton.INSPECT_TYPE.UNDEFINED));
                            }
                        }
                    } else {
                        // Bag is empty, if clicked - close it and end inspect state
                        if (Physics.Raycast(ray, out hit)) {
//                        Debug.Log(hit.transform.gameObject.name);

                            BagProperties clickedBagProperties = hit.transform.GetComponent<BagProperties>();
                            if (clickedBagProperties == currentBagInspect) {
                                inspectBagDone();
                            }
                        }
                    }

                }
            }
        }

        return PROPAGATION.DEFAULT;
    }

    public void inspectBagDone() {
        if (bagInspectState == BagInspectState.BAG_OPEN) {
            bagInspectState = BagInspectState.BUSY;
            currentBagInspect.putBackOkContent();
        }
    }

    public void bagInspectItemEnded (bool separateTrayItems = false) {
        bagInspectState = BagInspectState.BUSY;
        Misc.AnimateBlurTo("blurCamera", Game.instance.blurCamera.GetComponent<BlurOptimized>(), 0, 0f, 1);
        StartCoroutine(delayedInspectEndActions(separateTrayItems));
    }

    public void bagInspectFinalized () {
        currentBagInspect.enableContentColliders(false);
        bagInspectState = BagInspectState.NOTHING;
    }

    IEnumerator delayedInspectEndActions(bool separateTrayItems = false, float time = Misc.DEFAULT_ANIMATION_TIME, bool reverse = false) {
        yield return new WaitForSeconds(time);
        switchToGameCamera(reverse);
        PubSub.publish("inspect_inactive");
        bagInspectState = BagInspectState.BAG_OPEN;
        if (separateTrayItems) {
            currentBagInspect.separateTrayItems();
        }
    }

    private void switchToGameCamera (bool reverse = false) {
        Game.instance.inspectCamera.gameObject.SetActive(reverse);
        Game.instance.gameCamera.gameObject.SetActive(!reverse);
    }

    private void moveBagsAsideForTray (BagProperties bagProperties) {
        float trayDropPointX = Game.instance.getTrayDropPosition().x;
        float trayPlacingWidth = bagProperties.placingCube.transform.localScale.x;
        float trayRight = trayDropPointX + (trayPlacingWidth / 2f) * 1.5f;

        List<BagProperties> bagsToMove = activeBags.FindAll(bag => {
            float bagLeft = bag.transform.position.x - bag.placingCube.transform.localScale.x / 2f;
            return bagLeft <= trayRight;
        });


        List<GameObject> bagsToMoveObjects = new List<GameObject>();
        BagProperties frontmostBag = null;
        foreach (BagProperties bagToMove in bagsToMove) {
            if (frontmostBag == null || frontmostBag.transform.position.x < bagToMove.transform.position.x) {
                frontmostBag = bagToMove;
            }
            bagsToMoveObjects.Add(bagToMove.gameObject);
        }


        if (frontmostBag != null) {
            float frontMostBagLeft = frontmostBag.transform.position.x - frontmostBag.placingCube.transform.localScale.x / 2f;
            float movementNeeded = (frontMostBagLeft - (trayDropPointX - trayPlacingWidth / 2f)) + frontmostBag.placingCube.transform.localScale.x * 1.35f;

            string groupMovementId = "move_aside_for_tray_" + BagContentProperties.manualInspectTrayCounter;
            Vector3 moveVector = new Vector3(-movementNeeded, 0, 0);
            Misc.AnimateRelativeMovement(groupMovementId, bagsToMoveObjects, moveVector, 0.7f, true);
            StartCoroutine(showItemsAfterDelay(bagsToMoveObjects, false, 0.7f));
        }
    }

    IEnumerator showItemsAfterDelay(List<GameObject> bags, bool show, float delay) {
        yield return delay;
        List<BagProperties> allBags = bags.Select<GameObject, BagProperties>(bag => bag.GetComponent<BagProperties>()).ToList();
        foreach (BagProperties oneBag in allBags) {
            oneBag.showItems(show);
        }
    }

    IEnumerator placeItemsInBagAndDrop (BagProperties bagProperties, List<BagContentProperties> items) {
        Vector3 bagSize = bagProperties.placingCube.transform.localScale;
        foreach (BagContentProperties item in items) {
            item.transform.parent = bagProperties.contents.transform;
            Vector3 objectSize = item.objectSize;
            item.transform.localPosition = new Vector3(ItsRandom.randomPlusMinus(0f, (bagSize.x - objectSize.x) / 2f, RANDOM_TYPE), bagProperties.halfBagHeight, ItsRandom.randomPlusMinus(0f, (bagSize.z - objectSize.z) / 2f, RANDOM_TYPE));
            item.transform.localScale = Vector3.one;
            bagProperties.bagContents.Add(item);
            bagProperties.freezeContents(true);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.6f);
        dropBag(Vector3.zero);
    }

    IEnumerator placeItemsInBag (BagProperties bagProperties, int amount, List<BagContentProperties> toBePlacedInTrays, Person person) {
        int yieldEveryXthItem = 5;
        int yieldCounter = yieldEveryXthItem;
//        float lastCycleStart = Time.realtimeSinceStartup;
        Vector3 bagSize = bagProperties.placingCube.transform.localScale;
        bagProperties.placingCube.SetActive(true);
        Bounds bagBounds = bagProperties.placingCube.GetComponent<Collider>().bounds;
        bagProperties.placingCube.SetActive(false);
//        Debug.Log(bagBounds);

        // TODO - Hidden items, make sure to check person config on frequency of putting these items in
        // TODO - For now, always put one item
        GameObject hiddenObj = ItsRandom.pickRandom<BagContentType>(bagContentTypesHidden.ToList()).contentObj;
        GameObject hiddenItem = Instantiate(hiddenObj);
        hiddenItem.transform.parent = bagProperties.transform;
        BagContentProperties hiddenItemProperties = hiddenItem.GetComponent<BagContentProperties> ();
        // Randomize place in bottom of bag
        findPlaceForItemInBag(hiddenItemProperties, bagProperties, int.MaxValue, true);
        bagProperties.bagContents.Add(hiddenItemProperties);
        
        // TODO - This code block is only made for forcing an illegal item
        GameObject gunObj = ItsRandom.pickRandom<BagContentType>(bagContentTypesIllegal.ToList()).contentObj;
        GameObject gun = Instantiate(gunObj);
        gun.transform.parent = bagProperties.contents.transform;
        BagContentProperties gunProperties = gun.GetComponent<BagContentProperties> ();
        // Randomize place in bag
        findPlaceForItemInBag(gunProperties, bagProperties, int.MaxValue);
        bagProperties.bagContents.Add(gunProperties);
        // TODO - END

        for (int i = 0; i < amount; i++) {
            List<int> weights = bagContentTypesLegal.Select(obj => obj.frequency).ToList();
            List<GameObject> gameObjects = bagContentTypesLegal.Select(obj => obj.contentObj).ToList();
            // TODO - below are the REAL CODE - NOT ABOVE
//            List<int> weights = bagContentTypes.Select(obj => obj.frequency).ToList();
//            List<GameObject> gameObjects = bagContentTypes.Select(obj => obj.contentObj).ToList();
            GameObject randomGameObject = ItsRandom.pickRandomWithWeights(weights, gameObjects, RANDOM_TYPE);

            // Check if item should be in tray, or not instantiated by any other reason (eg. not place 3 guns in bag...)
            bool acceptItem = true;
            // TODO - Do this!
//            bool acceptItem = randomGameObject.GetComponent<BagContentInstantiationRules>()(toBePlacedInTrays, person);
            if (!acceptItem) {
                i--;
                continue;
            }

            GameObject contentPiece = Instantiate (randomGameObject);
            contentPiece.transform.parent = bagProperties.contents.transform;
            // Randomly rotate 90°-angle
            // TODO - When rotation turned on, objects seem to fall outside bag
//            contentPiece.transform.localRotation = Quaternion.Euler(0f, 90f * Misc.random.Next(), 0f);

            BagContentProperties bagContentProperties = contentPiece.GetComponent<BagContentProperties> ();

            // Randomize place in bag
            bool itemFitsInBag = findPlaceForItemInBag(bagContentProperties, bagProperties, 10);

            if (itemFitsInBag) {
                bagContentProperties.person = person;

                bagProperties.bagContents.Add(bagContentProperties);

                // Trigger "random"-functions on it
                RandomInterface[] randomInterfaces = contentPiece.GetComponents<RandomInterface>();
                foreach (RandomInterface randomInterface in randomInterfaces) {
                    randomInterface.run();
                }
            } else {
                Debug.Log("Item removed: " + contentPiece);
                contentPiece.transform.parent = null;
                Destroy(contentPiece);
            }
            
/*
            yieldCounter--;
            if (yieldCounter == 0) {
                yieldCounter = yieldEveryXthItem;
//            if (lastCycleStart + MAX_ITEM_PLACE_CYCLE_SECONDS < Time.realtimeSinceStartup) {
                Debug.Log("YIELD");
                yield return null;
//                // TODO - Compact items by code (move downwards)
//                yield return null;
//                lastCycleStart = Time.realtimeSinceStartup;
            }
*/
        }
        Debug.Log("Items in bag: " + bagProperties.bagContents.Count());
//        Debug.Break();
        yield return null;
    }

    IEnumerator shakeAndRotateBag (BagProperties bagProperties, float time = 1f) {
        GameObject bag = bagProperties.gameObject;
        Quaternion startRotation = bag.transform.rotation;
        Quaternion endRotation1 = Quaternion.Euler(0f, 0f, 20f)  * startRotation;
        Quaternion endRotation2 = Quaternion.Euler(0f, 0f, -20f)  * startRotation;
        float startTime = Time.time;
        float endTime1 = (time / 4f) * 1f;
        float endTime2 = (time / 4f) * 3f;
        float endTime3 = (time / 4f) * 4f;
        while (true) {
            float currentTime = Time.time - startTime;

            if (currentTime <= endTime1) {
                bag.transform.rotation = Quaternion.Slerp (startRotation, endRotation1, currentTime / endTime1);
            } else if (currentTime <= endTime2) {
                bag.transform.rotation = Quaternion.Slerp (endRotation1, endRotation2, (currentTime - endTime1) / (endTime2 - endTime1));
            } else {
                bag.transform.rotation = Quaternion.Slerp (endRotation2, startRotation, (currentTime - endTime2) / (endTime3 - endTime2));
            }

            if (currentTime >= endTime3) {
                break;
            }

            yield return null;
        }
    }

    IEnumerator positionLid (GameObject lid, float yDistance, float animationTime) {
        if (lid == null) {
            yield break;
        }
        lid.transform.position = new Vector3 (lid.transform.position.x, lid.transform.position.y + yDistance, lid.transform.position.z);
        lid.SetActive (true);
        yield return null;

        float startTime = Time.time;
        float endTime = startTime + animationTime;
        float startY = lid.transform.position.y;
        float endY = startY - yDistance;
        while (true) {
            yield return null;
            float currentTimeInRange = (Time.time - startTime) / animationTime;
            float currentY = Mathf.Lerp(startY, endY, currentTimeInRange);
            lid.transform.position = new Vector3 (lid.transform.position.x, currentY, lid.transform.position.z);
            if (currentTimeInRange >= 1f) {
                break;
            }
        }
    }

    public bool allowManualInspectOnCurrentBag() {
        return currentBagInspect.allowFurtherInspectionAction;
    }

    public bool allowNewTrayForBagContent() {
        return currentBagInspect.bagContents.FindIndex(item => item.actionTaken == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT) != -1;
    }

    public void bagFinished(BagProperties bagProperties) {
        activeBags.Remove(bagProperties);
    }

    public Coroutine packBagAndDropIt (Vector3 bagDropPosition, PersonBagDefinition bagDefinition, List<BagContentProperties> toBePlacedInTrays, Person person) {
        return StartCoroutine(packAndDropBag(bagDropPosition, bagDefinition, toBePlacedInTrays, person));
    }

    public IEnumerator packAndDropBag (Vector3 bagDropPosition, PersonBagDefinition bagDefinition, List<BagContentProperties> toBePlacedInTrays, Person person) {
        createBag(INIT_BAG_POINT);
        yield return placeItems(toBePlacedInTrays, person);
        yield return shuffleBag();
        yield return closeLid();
        currentBagPlacing.showItems(false);

        currentBagPlacing.bagDefinition = bagDefinition;
        currentBagPlacing.bagType = BagProperties.TYPE.DEFAULT;
        bagDefinition.addBag(currentBagPlacing);
        
        // Set teddybear color
        person.setTeddyBearColor(currentBagPlacing);

        dropBag(bagDropPosition);
    }

    private bool findPlaceForItemInBag(BagContentPropertiesBase item, BagProperties bagProperties, int tries = 10, bool inBottom = false) {
        Vector3 bagSize = (
            inBottom
                ? bagProperties.hiddenObjectsPlacingCube.transform.localScale
                : bagProperties.placingCube.transform.localScale
        );
        Vector3 objectSize = item.transform.localRotation * item.objectSize;

        bool objectsCollide = true;

        List<Collider> allOtherColliders = bagProperties.bagContents.Aggregate(new List<Collider>(), (accumulator, otherItem) => {
            accumulator.AddRange(otherItem.GetComponents<Collider>().ToList());
            return accumulator;
        });


        // Try to position object in bag for a number of times
        while (objectsCollide && tries-- > 0) {
            bool haveAdjustedPos = false;
            // Randomize position for object
            item.transform.localPosition = new Vector3(ItsRandom.randomPlusMinus(0f, (bagSize.x - objectSize.x) / 2f, RANDOM_TYPE), ItsRandom.randomPlusMinus(0f, (bagSize.y - objectSize.y) / 2f, RANDOM_TYPE), ItsRandom.randomPlusMinus(0f, (bagSize.z - objectSize.z) / 2f, RANDOM_TYPE))
                + (
                    inBottom
                        ? bagProperties.hiddenObjectsPlacingCube.transform.localPosition
                        : bagProperties.placingCube.transform.localPosition
                );

            if (inBottom) {
                objectsCollide = false;
                break;
            }

            while (true) {
                List<Collider> bagContentColliders = item.GetComponents<Collider>().ToList();
                Vector3 direction = Vector3.zero;
                float distance = 0f;
                
                objectsCollide = bagContentColliders.Find(ownCollider => {
                    return allOtherColliders.Find(otherCollider => {
                        return Physics.ComputePenetration(ownCollider, item.transform.localPosition, item.transform.localRotation, otherCollider, otherCollider.gameObject.transform.localPosition, otherCollider.gameObject.transform.localRotation, out direction, out distance);
                    }) != null;
                }) != null;

                if (objectsCollide) {
                    if (haveAdjustedPos) {
                        break;
                    }
                    // Move this object a bit and try again
                    item.transform.localPosition += direction * distance;
                    // Make sure we are still in the bag
                    if (Misc.isInside(item.transform.localPosition, objectSize, bagSize)) {
                        haveAdjustedPos = true;
                    } else {
                        break;
                    }
                } else {
                    goto afterCollideDetection;
                }
            }
        }

        afterCollideDetection:

        if (objectsCollide) {
            Debug.Log(item + ", NOK");
        }

        return !objectsCollide;
    }

    public void moveConveyor (float movement, XRayMachine xRayMachine) {
        if (bagInspectState == BagHandler.BagInspectState.NOTHING) {
            float xrayScanRight = xRayMachine.scanRight;
            float xrayScanLeft = xRayMachine.scanLeft;
            List<GameObject> bags = Misc.FindShallowStartsWith ("Bag_");
            foreach (GameObject bag in bags) {
                BagProperties bagProperties = bag.GetComponent<BagProperties>();
                if (bagProperties.isOnConveyor) {
                    float bagNewXPos =  bag.transform.position.x + movement;

                    GameObject triggerCube = bagProperties.contentsTriggerCube;
                    float centerOfTriggerCube = bag.transform.position.x + triggerCube.transform.localPosition.x;
                    float bagRightmostPos = centerOfTriggerCube + triggerCube.transform.localScale.x / 2f;
                    float bagLeftmostPos = centerOfTriggerCube - triggerCube.transform.localScale.x / 2f;

                    if (movement > 0 && bagRightmostPos < xrayScanLeft && bagRightmostPos + movement >= xrayScanLeft) {
                        bagProperties.showItems(true);
                    } else if (movement > 0 && bagLeftmostPos < xrayScanRight && bagLeftmostPos + movement >= xrayScanRight) {
                        bagProperties.showItems(false);
                    } else if (movement < 0 && bagLeftmostPos >= xrayScanRight && bagLeftmostPos + movement < xrayScanRight) {
                        bagProperties.showItems(true);
                    } else if (movement < 0 && bagRightmostPos >= xrayScanLeft && bagRightmostPos + movement < xrayScanLeft) {
                        bagProperties.showItems(false);
                    }

                    if (movement > 0 && bagNewXPos > xRayMachine.xPointOfNoReturn) {
                        bagProperties.bagFinished();
                    } else {
                        if (bagProperties.bagDefinition.person.showingPassport && bagProperties.bagDefinition.person.currentX >= xRayMachine.xPointOfNoBackingBags) {
                            bagProperties.bagDefinition.person.showPassport(false);
                        }
                        if (movement > 0 || (movement < 0 && bag.transform.position.x < xRayMachine.xPointOfNoBackingBags)) {
                            bag.transform.position = new Vector3(bag.transform.position.x + movement, bag.transform.position.y, bag.transform.position.z);
                        }
                    }
                }
            }
            PubSub.publish("belt_movement", movement);
        }
    }

    private const float INTERRUPT_ZOOM_THRESHOLD_MAGNITUDE = 0.05f;
    public void potentiallyStopZoomInspectItem(float movement) {
        if (movement >= INTERRUPT_ZOOM_THRESHOLD_MAGNITUDE) {
            Misc.StopAnimateFOV("zoom-item-inspect");
        }
    }

    public void zoomInspectItem(bool zoomIn, float movement = 0f) {
        if (bagInspectState == BagHandler.BagInspectState.ITEM_INSPECT) {
            // Inspect camera -> FOV 10
            // Release quicker to 20
            if (zoomIn) {
                if (movement < INTERRUPT_ZOOM_THRESHOLD_MAGNITUDE) {
                    Misc.AnimateFOVTo("zoom-item-inspect", Game.instance.inspectCamera, 10.0f, 0.75f);
                }
            } else {
                Misc.AnimateFOVTo("zoom-item-inspect", Game.instance.inspectCamera, 20.0f, 0.15f);
            }
        }
    }

}
