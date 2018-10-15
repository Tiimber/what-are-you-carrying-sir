using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMeshAndPosition : MonoBehaviour, RandomInterface {

    public MeshAndPositionToRandomize[] meshAndPositionsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
		foreach (MeshAndPositionToRandomize meshAndPositionToRandomize in meshAndPositionsToRandomize) {
            meshAndPositionToRandomize.assign();
        }
    }
}
