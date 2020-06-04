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
    public PillBottle pillBottle;
    public DoctorsNote doctorsNote;

    public PillsToRandomize[] pillsToRandomize;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void run() {
        PillsToRandomize chosenPillsConfig = ItsRandom.pickRandom(pillsToRandomize.ToList());
        chosenPillsConfig.assign(pillBottle, objectWithMaterial, materialIndex, pillsContainer, pillsContainerXray, liquidContainer, liquidContainerXray, organicMaterialXray);
        if (!chosenPillsConfig.needsPrescription) {
	        SlideInOtherObjectInspectAction slideInOtherObjectInspectAction = this.gameObject.GetComponent<SlideInOtherObjectInspectAction>();
	        foreach (SlideInOtherObjectDefinition slideInOtherObjectDefinition in slideInOtherObjectInspectAction.slideInObjectDefinitions) {
		        if (slideInOtherObjectDefinition.gameObjectToSlideIn.name.StartsWith("Doctors Note")) {
			        slideInOtherObjectDefinition.enabled = false;
		        }
	        }
        }
    }
}
