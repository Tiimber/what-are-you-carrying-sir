using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignMeshAndPositionToObject : MonoBehaviour {

    public Mesh mesh;
    public Vector3 position;

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.transform.localPosition = position;
	}
}
