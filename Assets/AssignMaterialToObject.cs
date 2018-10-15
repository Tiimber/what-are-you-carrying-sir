using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignMaterialToObject : MonoBehaviour {

    public Material material;
    public int materialIndex;

	// Use this for initialization
	void Start () {
        gameObject.GetComponent<Renderer>().materials[materialIndex] = material;
	}
}
