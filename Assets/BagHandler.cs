using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class BagHandler : MonoBehaviour, IPubSub {

    public static BagHandler instance;

    private BagProperties currentBagPlacing;
    private BagProperties currentBagInspect;

    public GameObject[] bags;
    public GameObject tray;
    public GameObject[] bagContents;

    private List<BagProperties> activeBags = new List<BagProperties>();

    public BagInspectState bagInspectState = BagInspectState.NOTHING;

    private static int BAG_CONTENTS_LAYER_MASK = -1;

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
        }
    }

    public void createBag (Vector3 bagDropPosition) {
        // TODO - Random bag - Some distribution factor? Maybe not plastic tray except for certain content?
        int randomBagIndex = Misc.randomRange(0, bags.Length);
        GameObject bagGameObject = Instantiate (bags [randomBagIndex], bagDropPosition, Quaternion.identity);
        BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
        bagGameObject.transform.position = new Vector3 (bagDropPosition.x, bagDropPosition.y + bagProperties.halfBagHeight, bagDropPosition.z);

        currentBagPlacing = bagProperties;
        activeBags.Add(bagProperties);
        bagProperties.lid.SetActive (false);
    }

    public void createTrayWithContents (Vector3 dropPosition, List<BagContentProperties> items) {
        if (items != null && items.Count > 0) {
            GameObject bagGameObject = Instantiate (tray, dropPosition, Quaternion.identity);
            BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
            bagGameObject.transform.position = new Vector3 (dropPosition.x, dropPosition.y + bagProperties.halfBagHeight, dropPosition.z);

            currentBagPlacing = bagProperties;

            moveBagsAsideForTray (bagGameObject.GetComponent<BagProperties>());
            StartCoroutine (placeItemsInBagAndDrop (currentBagPlacing, items));

            // TODO - Need shuffle

            activeBags.Add(bagProperties);
        }
    }

    public void placeItems () {
        StartCoroutine (placeItemsInBag (currentBagPlacing, 3));
    }

    public void shuffleBag () {
        // Shake/rotate
        StartCoroutine (shakeAndRotateBag(currentBagPlacing));
    }

    public void closeLid () {
        GameObject lid = currentBagPlacing.lid;
        float bagLidMovement = 2f * currentBagPlacing.halfBagHeight;
        StartCoroutine (positionLid (lid, bagLidMovement, 0.5f));
        // TODO - 3 - Check collision, remove collision objects, make sure special substances are still in (if any)
    }

    public void dropBag () {
        currentBagPlacing.disableInitialColliders ();
        currentBagPlacing.freezeContents();
        currentBagPlacing.setGravity(true);
    }

	public PROPAGATION onMessage(string message, object data) {
		if (message == "Click") {
            if (Game.instance.cameraXPos == 2 && !Game.instance.zoomedOutState) {
                Vector3 position = (Vector3)data;

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
                            clickedBagProperties.animateLidState(true);
                            clickedBagProperties.enableContentColliders(true);
                            currentBagInspect = clickedBagProperties;
                            bagInspectState = BagInspectState.BAG_OPEN;
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
                            }
                        }
                    } else {
                        // Bag is empty, if clicked - close it and end inspect state
                        if (Physics.Raycast(ray, out hit)) {
//                        Debug.Log(hit.transform.gameObject.name);

                            BagProperties clickedBagProperties = hit.transform.GetComponent<BagProperties>();
                            if (clickedBagProperties == currentBagInspect) {
                                bagInspectState = BagInspectState.BUSY;
                                currentBagInspect.putBackOkContent();
//                                clickedBagProperties.animateLidState(false);
//                                clickedBagProperties.enableContentColliders(false);
//                                currentBagInspect = null;
                            }
                        }
                    }

                }
            }
        }

        return PROPAGATION.DEFAULT;
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
        }
    }

    IEnumerator placeItemsInBagAndDrop (BagProperties bagProperties, List<BagContentProperties> items) {
        Vector3 bagSize = bagProperties.placingCube.transform.localScale;
        foreach (BagContentProperties item in items) {
            item.transform.parent = bagProperties.contents.transform;
            Vector3 objectSize = item.objectSize;
            item.transform.localPosition = new Vector3(Misc.randomPlusMinus(0f, (bagSize.x - objectSize.x) / 2f), bagProperties.halfBagHeight, Misc.randomPlusMinus(0f, (bagSize.z - objectSize.z) / 2f));
            item.transform.localScale = Vector3.one;
            bagProperties.bagContents.Add(item);
            bagProperties.freezeContents(true);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.6f);
        dropBag();
    }

    IEnumerator placeItemsInBag (BagProperties bagProperties, int amount) {
        Vector3 bagSize = bagProperties.placingCube.transform.localScale;
        for (int i = 0; i < amount; i++) {
            int pickedBagContentNumber = Misc.randomRange(0, bagContents.Length);
            GameObject contentPiece = Instantiate (bagContents [pickedBagContentNumber]);
            contentPiece.transform.parent = bagProperties.contents.transform;
            BagContentProperties bagContentProperties = contentPiece.GetComponent<BagContentProperties> ();
            Vector3 objectSize = bagContentProperties.objectSize;
            contentPiece.transform.localPosition = new Vector3(Misc.randomPlusMinus(0f, (bagSize.x - objectSize.x) / 2f), bagProperties.halfBagHeight, Misc.randomPlusMinus(0f, (bagSize.z - objectSize.z) / 2f));
            bagProperties.bagContents.Add(bagContentProperties);

            // Trigger "random"-funnctions on it
            RandomInterface[] randomInterfaces = contentPiece.GetComponents<RandomInterface>();
            foreach (RandomInterface randomInterface in randomInterfaces) {
                randomInterface.run();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator shakeAndRotateBag (BagProperties bagProperties, float time = 2f) {
        GameObject bag = bagProperties.gameObject;
        Quaternion startRotation = bag.transform.rotation;
        Quaternion endRotation1 = Quaternion.Euler(0f, 0f, 30f)  * startRotation;
        Quaternion endRotation2 = Quaternion.Euler(0f, 0f, -30f)  * startRotation;
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

}
