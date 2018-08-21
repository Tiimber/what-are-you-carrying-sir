using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTexture : MonoBehaviour, RandomInterface {

    public TextureToRandomize[] texturesToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
		foreach (TextureToRandomize textureToRandomize in texturesToRandomize) {
            textureToRandomize.assign();
        }
    }
}
