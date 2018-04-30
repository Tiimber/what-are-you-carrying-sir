using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagProperties : MonoBehaviour {

    private static int idCounter = 0;
    private int id;

    public Material interiorMaterial;
	public GameObject contents;
	public GameObject actualBag;
	public GameObject lid;
	public GameObject lidRotationObj;
    public Vector3 lidRotationOpen;
	public GameObject placingCube;
	public GameObject[] initialColliders;
	public float halfBagHeight;

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
			meshCollider.convex = true;
		}
	}

	public void freezeContents () {
		foreach (Rigidbody rigidbody in contents.transform.GetComponentsInChildren<Rigidbody> ()) {
			rigidbody.useGravity = false;
			rigidbody.isKinematic = true;
		}
		foreach (Collider collider in contents.transform.GetComponentsInChildren<Collider> ()) {
			collider.enabled = false;
		}
	}

	public void disableInitialColliders () {
		foreach (GameObject colliderGameObject in initialColliders) {
			colliderGameObject.SetActive (false);
		}
	}

    public void animateLidState (bool open = false) {
        float currentRotationProgress = lidRotationObj.transform.rotation.eulerAngles.magnitude / lidRotationOpen.magnitude;
        float time = 1f * (open ? 1f - currentRotationProgress : currentRotationProgress);

        Quaternion toRotation = open ? Quaternion.Euler(lidRotationOpen) : Quaternion.identity;
		Misc.AnimateRotationTo(gameObject.name + "_" + id, lidRotationObj, toRotation, time);
    }

	public void OnTriggerEnter (Collider collider) {
		if (collider != null && collider.GetComponent<ConveyorObject> () != null) {
			setEnabledStateCollidersAndRigidbodies (false);
		}
	}
}
