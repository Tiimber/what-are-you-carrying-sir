using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecificTexture : MonoBehaviour {

    public TextureToSet[] texturesToSet;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setTexture(string category, string idString) {
/*
        Debug.Log("Select image: " + idString);
        List<Texture> textures = Resources.LoadAll<Texture>("flags/").ToList();
        Debug.Log(textures.Count);
        foreach (Texture text in textures) {
            Debug.Log(text.name);
        }
*/

        Texture texture = Resources.Load<Texture>(idString);
        Debug.Log("Texture: " + texture.name);
        foreach (TextureToSet textureToSet in texturesToSet) {
            if (textureToSet.category == category) {
                textureToSet.assign(texture, 0);
            }
        }
    }
}
