using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RandomPills : MonoBehaviour, RandomInterface {

    public PerRendererShaderTexture objectWithMaterial;
    public int materialIndex;
    public GameObject pillsContainer;
    public GameObject pillsContainerXray;
    public GameObject liquidContainer;
    public GameObject liquidContainerXray;
    public Material organicMaterialXray;

    public PillsToRandomize[] pillsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
        PillsToRandomize chosenPillsConfig = Misc.pickRandom(pillsToRandomize.ToList());
        chosenPillsConfig.assign(gameObject, objectWithMaterial, materialIndex, pillsContainer, pillsContainerXray, liquidContainer, liquidContainerXray, organicMaterialXray);
    }
}
