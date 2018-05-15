using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class BagContentProperties : MonoBehaviour {

    private static float ROTATION_SPEED = 500f;

    private static int idCounter = 0;
    private int id;

    private bool inspecting = false;

	public Vector3 objectSize;

    private Vector3 locationInBag;
    private Quaternion rotationInBag;
    private Transform parentBag;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnMouseDrag () {
        if (inspecting) {
            Debug.Log("Rotate");
            float rotX = Input.GetAxis("Mouse X") * ROTATION_SPEED * Mathf.Deg2Rad;
            float rotY = Input.GetAxis("Mouse Y") * ROTATION_SPEED * Mathf.Deg2Rad;

            Debug.Log(rotX + ", " + rotY);
            transform.Rotate(Game.instance.gameCamera.transform.rotation * Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }

    void Awake() {
        id = ++idCounter;
    }

    public void inspect () {
        locationInBag = this.transform.localPosition;
        rotationInBag = this.transform.localRotation;
        parentBag = this.transform;

        // Target object position
        Vector3 targetPosition = Game.instance.gameCamera.transform.position + Game.instance.gameCamera.transform.rotation * Vector3.forward;
        this.transform.parent = null;
        Misc.AnimateMovementTo("content-zoom-"+id, this.gameObject, targetPosition);

        // Camera blur
        Game.instance.gameCamera.gameObject.SetActive(false);
        Game.instance.inspectCamera.gameObject.SetActive(true);

        inspecting = true;
    }

}
