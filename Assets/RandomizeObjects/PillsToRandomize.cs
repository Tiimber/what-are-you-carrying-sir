using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class PillsToRandomize {

    static Color ORGANIC_PILL_COLOR = new Color(0.83137256f, 0.7058824f, 0.16078432f);
    private static List<Color[]> NON_ORGANIC_PILL_COLORS = new List<Color[]>() {
        new Color[] {
            new Color(1f, 0f, 0f)
        },
        new Color[] {
            new Color(0f, 1f, 0f), new Color(0f, 0f, 1f)
        }, 
        new Color[] {
            new Color(0f, 1f, 0f)
        }, 
        new Color[] {
            new Color(0.89f, 0.53f, 0.046f)
        }, 
        new Color[] {
            new Color(0f, 0f, 1f)
        },
    };

    const int RANDOM_BASE_AMOUNT_PILLS = 10;

    const int FACTOR_FOR_WRONG_PILL_AMOUNT = 15;
    const int FACTOR_FOR_WRONG_PILL_TYPE = 5; // liquid when should be pill, or organic pill when should be "normal"...

    public Texture texture;
    public int specifiedAmount;
    public bool specifiedAsOrganic;
    public bool specifiedAsLiquid;
    public string pillLabel;
    public bool needsPrescription;

    // Actually instantiated
    public int amount;
    public bool organic;
    public bool liquid;

    public void assign(PillBottle pillBottle, PerRendererShaderTexture objectWithMaterial, int materialIndex, GameObject pillsContainer, GameObject pillsContainerXray, GameObject liquidContainer, GameObject liquidContainerXray, Material organicMaterialXray) {
        objectWithMaterial.texture = texture;
        objectWithMaterial.materialIndex = materialIndex;

        // Set the name (and label - for inspect) on the pill bottle
        pillBottle.gameObject.name = "Bottle of '" + pillLabel + "'";
        pillBottle.gameObject.GetComponent<BagContentProperties>().displayName = "Bottle of '" + pillLabel + "'";

        liquid = specifiedAsLiquid;
        amount = specifiedAmount;
        organic = specifiedAsOrganic;

        // Random "wrongness" - pill type
        int randomType = ItsRandom.randomRange(0, 100);
        if (randomType <= FACTOR_FOR_WRONG_PILL_TYPE) {
            bool wrongSubstanceLiquid = ItsRandom.randomBool();
            if (wrongSubstanceLiquid && !specifiedAsLiquid) {
                amount = 0;
                liquid = true;
            } else if (wrongSubstanceLiquid && specifiedAsLiquid) {
                liquid = false;
            } else {
                organic = !specifiedAsOrganic;
            }
        }

        // Random "wrongness" - amount of pills
        int randomAmount = ItsRandom.randomRange(0, 100);
        if (!liquid && (randomAmount <= FACTOR_FOR_WRONG_PILL_AMOUNT || specifiedAsLiquid)) {
            amount = ItsRandom.randomRange(1, (!specifiedAsLiquid ? specifiedAmount : RANDOM_BASE_AMOUNT_PILLS) + 3);
        }

        if (amount > 0) {
            // Decide color pair for pills
            Color[] chosenColor = organic ? 
                new[] {ORGANIC_PILL_COLOR} : 
                ItsRandom.pickRandom(NON_ORGANIC_PILL_COLORS);

            for (int i = amount; i < pillsContainer.transform.childCount; i++) {
                pillsContainer.transform.GetChild(i).gameObject.SetActive(false);
                pillsContainerXray.transform.GetChild(i).gameObject.SetActive(false);
            }

            // TODO - Maybe make > some < of the pills organic (if wrong that should be detected)
            for (int i = 0; i < amount; i++) {
                if (organic) {
                    Material[] organicMaterials = new []{organicMaterialXray, organicMaterialXray};
                    // Debug.Log(pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials.Length);
                    // Debug.Log(organicMaterialXray.name);
                    // Debug.Log(pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials[0].name);
                    // Debug.Log(pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials[1].name);
                    // pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials[0] = organicMaterialXray;
                    // pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials[1] = organicMaterialXray;
                    pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials = organicMaterials;
                    // Debug.Log(pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials[0].name);
                    // Debug.Log(pillsContainerXray.transform.GetChild(i).GetChild(0).GetComponent<Renderer>().materials[1].name);
                }
                PerRendererShader[] perRendererShaders = pillsContainer.transform.GetChild(i).GetChild(0).GetComponents<PerRendererShader>();
                // Debug.Log("Colors: " + chosenColor[0].ToString() + ", " + (chosenColor.Length > 1 ? chosenColor[1] : chosenColor[0]).ToString());
                perRendererShaders[0].color = chosenColor[0];
                perRendererShaders[1].color = chosenColor.Length > 1 ? chosenColor[1] : chosenColor[0];
                perRendererShaders[0].enabled = true;
                perRendererShaders[1].enabled = true;

                pillBottle.colorHalf1 = chosenColor[0];
                pillBottle.colorHalf2 = chosenColor.Length > 1 ? chosenColor[1] : chosenColor[0];
            }

            // Remove liquid from gameObject
            GameObject.Destroy(liquidContainer);
            GameObject.Destroy(liquidContainerXray);
        } else {
            // Remove pills from gameObject
            GameObject.Destroy(pillsContainer);
            GameObject.Destroy(pillsContainerXray);
        }
    }
}
