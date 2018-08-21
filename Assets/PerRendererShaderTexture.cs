using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerRendererShaderTexture : MonoBehaviour {

    public Texture texture;
    public int materialIndex;

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

	// Use this for initialization
	void Start () {
        _propBlock = new MaterialPropertyBlock();
        _renderer = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        // Get the current value of the material properties in the renderer.
        _renderer.GetPropertyBlock(_propBlock, materialIndex);
        // Assign our new value.
        _propBlock.SetTexture("_MainTex", texture);
        // Apply the edited values to the renderer.
        _renderer.SetPropertyBlock(_propBlock, materialIndex);

    }
}
