using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomColor : MonoBehaviour, RandomInterface {

    public ColorToRandomize[] colorsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
		foreach (ColorToRandomize colorToRandomize in colorsToRandomize) {
            colorToRandomize.assign();
        }
    }
}
