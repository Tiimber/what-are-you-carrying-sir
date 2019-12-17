using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTextPairs : MonoBehaviour, RandomInterface {

    public TextPairToRandomize[] textPairsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
		foreach (TextPairToRandomize textPairToRandomize in textPairsToRandomize) {
            textPairToRandomize.assign();
        }
    }
}
