using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class BagHandler : MonoBehaviour, IPubSub {

    public static BagHandler instance;

    private BagProperties currentBagPlacing;
    private BagProperties currentBagInspect;

    public GameObject[] bags;
    public GameObject[] bagContents;

    public BagInspectState bagInspectState = BagInspectState.NOTHING;

    private static int BAG_CONTENTS_LAYER_MASK = -1;

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

    public void createBag (Vector3 bagDropPosition) {
        // TODO - Random bag
        GameObject bagGameObject = Instantiate (bags [0], bagDropPosition, Quaternion.identity);
        BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
        bagGameObject.transform.position = new Vector3 (bagDropPosition.x, bagDropPosition.y + bagProperties.halfBagHeight, bagDropPosition.z);

        currentBagPlacing = bagProperties;
        bagProperties.lid.SetActive (false);
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

    public void bagInspectItemEnded () {
        bagInspectState = BagInspectState.BUSY;
        Misc.AnimateBlurTo("blurCamera", Game.instance.blurCamera.GetComponent<BlurOptimized>(), 0, 0f, 1);
        StartCoroutine(delayedInspectEndActions());
    }

    public void bagInspectFinalized () {
        currentBagInspect.enableContentColliders(false);
        bagInspectState = BagInspectState.NOTHING;
    }

    IEnumerator delayedInspectEndActions(float time = Misc.DEFAULT_ANIMATION_TIME, bool reverse = false) {
        yield return new WaitForSeconds(time);
        switchToGameCamera(reverse);
        PubSub.publish("inspect_inactive");
        bagInspectState = BagInspectState.BAG_OPEN;
    }

    private void switchToGameCamera (bool reverse = false) {
        Game.instance.inspectCamera.gameObject.SetActive(reverse);
        Game.instance.gameCamera.gameObject.SetActive(!reverse);
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
            yield return new WaitForSeconds(0.1F);
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

}
