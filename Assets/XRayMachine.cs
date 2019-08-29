using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRayMachine : MonoBehaviour {

	public Vector3 bagDropPoint;

	public Vector3 dropPointZoomOutPos;
    public Vector3 dropPointZoomOutRotation;
	public float dropPointZoomOutZoom;

	public Vector3 scanPointZoomOutPos;
    public Vector3 scanPointZoomOutRotation;
	public float scanPointZoomOutZoom;

	public Vector3 checkPointZoomOutPos;
    public Vector3 checkPointZoomOutRotation;
	public float checkPointZoomOutZoom;

	public Vector3 dropPointZoomInPos;
    public Vector3 dropPointZoomInRotation;
	public float dropPointZoomInZoom;

	public Vector3 scanPointZoomInPos;
    public Vector3 scanPointZoomInRotation;
	public float scanPointZoomInZoom;

	public Vector3 checkPointZoomInPos;
    public Vector3 checkPointZoomInRotation;
    public float checkPointZoomInZoom;

    public GameObject conveyor;
    public Vector3 connectingConveyorLeftPos;
    public Vector3 connectingConveyorRightPos;

    public float xPointOfNoReturn;
    public float xPointOfTrayInsertion;

    public GameObject scanAreaCube;

    [HideInInspector]
    public float scanLeft;
    [HideInInspector]
    public float scanRight;

	// Use this for initialization
	void Start () {
        float scanCenter = transform.position.x + scanAreaCube.transform.localPosition.x;
        scanRight = scanCenter + scanAreaCube.transform.localScale.x / 2f;
        scanLeft = scanCenter - scanAreaCube.transform.localScale.x / 2f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void attachConnectingConveyors() {
        if (conveyor != null) {
            GameObject leftConveyor = Instantiate(conveyor, connectingConveyorLeftPos, Quaternion.identity, transform);
            GameObject rightConveyor = Instantiate(conveyor, connectingConveyorRightPos, Quaternion.identity, transform);
        }

    }
}
