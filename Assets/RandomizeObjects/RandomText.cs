using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomText : MonoBehaviour, RandomInterface {

    public TextToRandomize[] textsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
		foreach (TextToRandomize textToRandomize in textsToRandomize) {
            textToRandomize.assign();
        }
    }
}
