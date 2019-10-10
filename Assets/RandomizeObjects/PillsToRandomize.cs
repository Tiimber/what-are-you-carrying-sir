using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class PillsToRandomize {

    static Color ORGANIC_PILL_COLOR = new Color(0.83137256f, 0.7058824f, 0.16078432f);

    const int RANDOM_BASE_AMOUNT_PILLS = 10;

    const int FACTOR_FOR_WRONG_PILL_AMOUNT = 15;
    const int FACTOR_FOR_WRONG_PILL_TYPE = 5; // liquid when should be pill, or organic pill when should be "normal"...

    public Texture texture;
    public int specifiedAmount;
    public bool specifiedAsOrganic;
    public bool specifiedAsLiquid;
    public string pillLabel;

    // Actually instantiated
    public int amount;
    public bool organic;
    public bool liquid;

    public void assign(GameObject pillBottle, PerRendererShaderTexture objectWithMaterial, int materialIndex, GameObject pillsContainer, GameObject pillsContainerXray, GameObject liquidContainer, GameObject liquidContainerXray, Material organicMaterialXray) {
        objectWithMaterial.texture = texture;
        objectWithMaterial.materialIndex = materialIndex;

        // Set the name (and label - for inspect) on the pill bottle
        pillBottle.name = "Bottle of '" + pillLabel + "'";
        pillBottle.GetComponent<BagContentProperties>().displayName = "Bottle of '" + pillLabel + "'";

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
            for (int i = amount; i < pillsContainer.transform.childCount; i++) {
                pillsContainer.transform.GetChild(i).gameObject.SetActive(false);
                pillsContainerXray.transform.GetChild(i).gameObject.SetActive(false);
            }

            // TODO - Maybe make > some < of the pills organic (if wrong that should be detected)
            if (organic) {
                for (int i = 0; i < amount; i++) {
                    PerRendererShader perRendererShader = pillsContainer.transform.GetChild(i).GetComponent<PerRendererShader>();
                    perRendererShader.color = ORGANIC_PILL_COLOR;
                    perRendererShader.enabled = true;
                    pillsContainerXray.transform.GetChild(i).GetComponent<Renderer>().material = organicMaterialXray;
                }
            }

            // Remove pills from gameObject
            GameObject.Destroy(liquidContainer);
            GameObject.Destroy(liquidContainerXray);
        } else {
            GameObject.Destroy(pillsContainer);
            GameObject.Destroy(pillsContainerXray);
        }
    }
}
