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

    public static Game instance;

	private XRayMachine currentXrayMachine;

    void Awake () {
        Game.instance = this;
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
			Vector3 bagDropPositionRelativeXrayMachine = currentXrayMachine.dropPoint;
			Vector3 bagDropPosition = Misc.getWorldPosForParentRelativePos (bagDropPositionRelativeXrayMachine, currentXrayMachine.transform);
			// TODO - Random bag
			GameObject bagGameObject = Instantiate (bags [0], bagDropPosition, Quaternion.identity);
			BagProperties bagProperties = bagGameObject.GetComponent<BagProperties> ();
			bagGameObject.transform.position = new Vector3 (bagDropPosition.x, bagDropPosition.y + bagProperties.halfBagHeight, bagDropPosition.z);

			currentBagPlacing = bagProperties;
			bagProperties.lid.SetActive (false);
		} else if (Input.GetKeyDown (KeyCode.Alpha2)) {
			StartCoroutine (placeItemsInBag (currentBagPlacing, 50));
		} else if (Input.GetKeyDown (KeyCode.Alpha3)) {
			currentBagPlacing.lid.SetActive (true);
		}
		// TODO - 3 - Move down lid, check collision, remove collision objects, make sure special substances are still in (if any)
	}

	IEnumerator placeItemsInBag (BagProperties bagProperties, int amount) {
		// TODO - Plan the bag contents in some way
		for (int i = 0; i < amount; i++) {
			int pickedBagContentNumber = Misc.randomRange(0, bagContents.Length);
			GameObject contentPiece = Instantiate (bagContents [pickedBagContentNumber]);
			contentPiece.transform.parent = bagProperties.contents.transform;
			contentPiece.transform.localPosition = new Vector3(0f, bagProperties.halfBagHeight, 0f);
//				BagContentProperties bagContentProperties = contentPiece.GetComponent<BagContentProperties> ();
			yield return new WaitForSeconds(0.2F);
		}
	}
}