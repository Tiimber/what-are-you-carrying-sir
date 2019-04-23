using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagProperties : MonoBehaviour {

    private static int idCounter = 0;
    public int id;

    public Material interiorMaterial;
	public GameObject contents;
	public GameObject actualBag;
	public GameObject lid;
	public GameObject lidRotationObj;
    public Vector3 lidRotationOpen;
	public GameObject placingCube;
	public GameObject[] initialColliders;
	public Collider bottomCollider;
	public float halfBagHeight;
    public GameObject contentsTriggerCube;

    public List<BagContentProperties> bagContents = new List<BagContentProperties>();

    public bool allowFurtherInspectionAction = true;

    public bool isOnConveyor = false;
    public bool isOpen = false;

    public string bagDisplayName;

    public PersonBagDefinition bagDefinition;

    public TYPE bagType;

    public enum TYPE {
        DEFAULT,
        TRAY_AFTER_INSPECT,
        TRAY_PLACED_BY_PERSON,
    };

    void Awake() {
        id = ++idCounter;
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setEnabledStateCollidersAndRigidbodies (bool enable = true) {
		setGravity (enable);

		foreach (MeshCollider meshCollider in actualBag.GetComponentsInChildren<MeshCollider>()) {
			meshCollider.enabled = enable;
		}
	}

	public void setGravity (bool gravityOn = true) {
		Rigidbody rigidbody = GetComponent<Rigidbody> ();
		rigidbody.useGravity = gravityOn;
		rigidbody.isKinematic = !gravityOn;

		foreach (MeshCollider meshCollider in actualBag.GetComponentsInChildren<MeshCollider>()) {
			meshCollider.convex = gravityOn;
		}
	}

	public void freezeContents (bool reverse = false) {
		foreach (Rigidbody rigidbody in contents.transform.GetComponentsInChildren<Rigidbody> ()) {
			rigidbody.useGravity = reverse;
			rigidbody.isKinematic = !reverse;
		}

        enableContentColliders (reverse);
	}

    public void enableContentColliders (bool enable = true) {
        foreach (Collider collider in contents.transform.GetComponentsInChildren<Collider> ()) {
            collider.enabled = enable;
        }
    }

	public void disableInitialColliders (bool reverse = false) {
		foreach (GameObject colliderGameObject in initialColliders) {
			colliderGameObject.SetActive (reverse);
		}
	}

    public void animateLidState (bool open = false, bool calculateSmartTime = true) {
        if (lidRotationObj) {
            float currentRotationProgress = lidRotationObj.transform.localRotation.eulerAngles.magnitude / lidRotationOpen.magnitude;
            float time = 0.5f * (open ? 1f - currentRotationProgress : currentRotationProgress);

            Quaternion toRotation = open ? Quaternion.Euler(lidRotationOpen) : Quaternion.identity;
            if (calculateSmartTime) {
                Misc.AnimateRotationTo(gameObject.name + "_" + id, lidRotationObj, toRotation, time);
            } else {
                Misc.AnimateRotationTo(gameObject.name + "_" + id, lidRotationObj, toRotation);
            }
        }
    }

	public void OnTriggerEnter (Collider collider) {
		if (collider != null && collider.GetComponent<ConveyorObject> () != null) {
            isOnConveyor = true;
			setEnabledStateCollidersAndRigidbodies (false);

            // Calculate how much "up-force" we should apply on bag to move it on top of conveyor
            Vector3 pushBackDirection;
            float pushBackDistance;
            bool intersecting = Physics.ComputePenetration(bottomCollider, transform.position, transform.rotation, collider, collider.gameObject.transform.position, collider.gameObject.transform.rotation, out pushBackDirection, out pushBackDistance);
            if (intersecting) {
                transform.position += pushBackDistance * pushBackDirection;
            }
        }
	}

    public void putBackOkContent () {
        List<BagContentProperties> okItems = bagContents.FindAll(item => item.actionTaken == InspectUIButton.INSPECT_TYPE.OK);
        StartCoroutine(animateItemsAboveBagAndDrop(okItems));
        separateTrayItems();
    }

    private IEnumerator animateItemsAboveBagAndDrop (List<BagContentProperties> items) {
		foreach (BagContentProperties item in items) {
            item.animateToBag();
            yield return new WaitForSeconds(0.05f);
        }
        // TODO - It would be nice if we could solve so that the items are dropped (with gravity) back into bag... for now, we animate it to its original spot
        yield return new WaitForSeconds(BagContentProperties.TIME_ANIMATE_TO_DROP_POINT + BagContentProperties.TIME_TO_TARGET_POS);
//        disableInitialColliders(true);
//        freezeContents(true);
        animateLidState(calculateSmartTime: false);

        // Wait for lid to close
        yield return new WaitForSeconds(Misc.DEFAULT_ANIMATION_TIME);

        // Update current Bag state in BagHandler
        BagHandler.instance.bagInspectFinalized();
        showItems(false);
    }

    public void bagFinished () {
        Debug.Log("Bag Finished!");
        BagHandler.instance.bagFinished (this);
        // For each item in bag, score point depending on action
        int points = 0;
//        Debug.Log("Number of items: " + bagContents.Count);
        foreach (BagContentProperties bagContentProperties in bagContents) {
//            Debug.Log("Check content for correct action: " + bagContentProperties.gameObject.name);
            int pointsForItem = bagContentProperties.calculatePoints();
            if (pointsForItem == 0) {
                Debug.Log("Wrong action taken for: " + bagContentProperties.gameObject.name);
                bagDefinition.person.registerWrongAction(bagContentProperties);
                // Wrong action taken
                Generic.CONSEQUENCE selectedConsequence = bagContentProperties.getConsequence();
                Debug.Log(selectedConsequence);
                string consequence = Generic.increaseConsequenceCount(selectedConsequence);
                if (consequence != null) {
                    // TODO - Do something with this
                    Debug.Log("Consequence: " + consequence);
                }
            }
            points += pointsForItem;
        }
        string worstMistake = bagDefinition.person.getWorstMistake();
        if (worstMistake == "warning") {
            // TODO - Do something with personal warning
        } else if (worstMistake != "none") {
            Game.instance.registerMistake(worstMistake);
        }

        // Clear GameObjects (bag + contents)
        while (bagContents.Count > 0) {
            // TODO - Report removal of item to "person"
            Destroy(bagContents[0].gameObject);
            bagContents.RemoveAt(0);
        }
        Destroy(this.gameObject);
        // TODO - Report results of bag to "person"
    }

    public void separateTrayItems () {
        List<BagContentProperties> manualInspectItems = bagContents.FindAll(item => item.actionTaken == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT || item.actionTaken == InspectUIButton.INSPECT_TYPE.MANUAL_INSPECT_NEW);
        int lowestTrayIndex = int.MaxValue;
        foreach (BagContentProperties item in manualInspectItems) {
            lowestTrayIndex = Math.Min(item.manualInspectTrayNumber, lowestTrayIndex);
        }
        manualInspectItems = manualInspectItems.FindAll(item => item.manualInspectTrayNumber == lowestTrayIndex);
        bagContents.RemoveAll(item => manualInspectItems.Contains(item));
        BagHandler.instance.createTrayWithContents(Game.instance.getTrayDropPosition(), manualInspectItems, bagDefinition);
    }

    public void showItems (bool show = true) {
        if (lid != null) {
            contents.SetActive(show);
        }
    }

    public void resetActionOnItems() {
        List<BagContentProperties> allOKItems = bagContents.FindAll(item => item.actionTaken == InspectUIButton.INSPECT_TYPE.OK);
        foreach (BagContentProperties item in allOKItems) {
            item.actionTaken = InspectUIButton.INSPECT_TYPE.UNDEFINED;
        }
    }
}
