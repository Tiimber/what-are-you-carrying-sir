using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMaterial : MonoBehaviour, RandomInterface {

    public MaterialToRandomize[] materialsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
		foreach (MaterialToRandomize materialToRandomize in materialsToRandomize) {
            materialToRandomize.assign();
        }
    }
}
