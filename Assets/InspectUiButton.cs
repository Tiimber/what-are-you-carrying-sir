using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectUIButton : MonoBehaviour {

    public enum INSPECT_TYPE {
        UNDEFINED,
        TRASHCAN,
        OK,
        MANUAL_INSPECT,
        POLICE,
    }

    public INSPECT_TYPE type;
    private Vector3 targetPosition = Vector3.zero;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Vector3 getTargetPosition () {
        if (targetPosition == Vector3.zero) {
            targetPosition = transform.parent.position;
        }
        return targetPosition;
    }

    public void trigger () {
        PubSub.publish("inspect_action", type);
    }
}
