using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {

    const float CONVEYOR_SPEED = 0.005f;
	const float CONVEYOR_BACK_PCT_SPEED = 0.8f;

	public BagProperties currentBagPlacing;

	public GameObject[] xrayMachines;
	public GameObject[] bags;
	public GameObject[] bagContents;

    public Camera gameCamera;

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
		// TODO - Pick xrayMachine depending on level instead...
		GameObject xrayMachineGameObject = Instantiate(xrayMachines[0], new Vector3(0f, 1f, 0f), Quaternion.identity);
		currentXrayMachine = xrayMachineGameObject.GetComponent<XRayMachine> ();
	}

	// Update is called once per frame
	void Update () {
		if ((Input.GetKey (KeyCode.LeftShift) || Input.GetKey (KeyCode.RightShift)) && Input.GetKey (KeyCode.Space)) {
			List<GameObject> bags = Misc.FindShallowStartsWith ("Bag_");
			foreach (GameObject bag in bags) {
				bag.transform.position = new Vector3 (bag.transform.position.x - CONVEYOR_SPEED * CONVEYOR_BACK_PCT_SPEED, bag.transform.position.y, bag.transform.position.z);
			}
		} else if (Input.GetKey(KeyCode.Space)) {
			List<GameObject> bags = Misc.FindShallowStartsWith ("Bag_");
			foreach (GameObject bag in bags) {
				bag.transform.position = new Vector3 (bag.transform.position.x + CONVEYOR_SPEED, bag.transform.position.y, bag.transform.position.z);
			}
		}

		// Create a new bag - disable lid
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			Vector3 bagDropPositionRelativeXrayMachine = currentXrayMachine.bagDropPoint;
			Vector3 bagDropPosition = Misc.getWorldPosForParentRelativePos (bagDropPositionRelativeXrayMachine, currentXrayMachine.transform);
			// TODO - Random bag
			GameObject bagGameObject = Instantiate (bags [0], bagDropPosition, Quaternion.identity);
			BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
			bagGameObject.transform.position = new Vector3 (bagDropPosition.x, bagDropPosition.y + bagProperties.halfBagHeight, bagDropPosition.z);

			currentBagPlacing = bagProperties;
			bagProperties.lid.SetActive (false);
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			StartCoroutine (placeItemsInBag (currentBagPlacing, 20));
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
			// Shake/rotate
			StartCoroutine (shakeAndRotateBag(currentBagPlacing));
		} else if (Input.GetKeyDown (KeyCode.Alpha4)) {
			GameObject lid = currentBagPlacing.lid;
			float bagLidMovement = 2f * currentBagPlacing.halfBagHeight;
			StartCoroutine (positionLid (lid, bagLidMovement, 0.5f));
			// TODO - 3 - Check collision, remove collision objects, make sure special substances are still in (if any)
		} else if (Input.GetKeyDown (KeyCode.Alpha5)) {
			currentBagPlacing.disableInitialColliders ();
			currentBagPlacing.freezeContents();
			currentBagPlacing.setGravity(true);
		} else if (Input.GetKeyDown (KeyCode.Alpha6)) {
            currentBagPlacing.animateLidState(true);
		} else if (Input.GetKeyDown (KeyCode.Alpha7)) {
            currentBagPlacing.animateLidState(false);
		} else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            if (zoomedOutState) {
                zoomedOutState = false;
                animateCameraChange();
            }
		} else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            if (!zoomedOutState) {
               zoomedOutState = true;
                animateCameraChange();
            }
		} else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (cameraXPos != 0) {
                cameraXPos--;
                animateCameraChange();
            }
		} else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (cameraXPos != 2) {
                cameraXPos++;
                animateCameraChange();
            }
        }
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