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
//        Debug.Log("RANDOM MATERIAL: " + name + ", " + materialsToRandomize.Length);
		foreach (MaterialToRandomize materialToRandomize in materialsToRandomize) {
//            Debug.Log("random assign!");
            materialToRandomize.assign();
        }
    }
}
