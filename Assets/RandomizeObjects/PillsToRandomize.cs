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
        int randomType = Misc.randomRange(0, 100);
        if (randomType <= FACTOR_FOR_WRONG_PILL_TYPE) {
            bool wrongSubbstanceLiquid = Misc.randomBool();
            if (wrongSubbstanceLiquid && !specifiedAsLiquid) {
                amount = 0;
                liquid = true;
            } else if (wrongSubbstanceLiquid && specifiedAsLiquid) {
                liquid = false;
            } else {
                organic = !specifiedAsOrganic;
            }
        }

        // Random "wrongness" - amount of pills
        int randomAmount = Misc.randomRange(0, 100);
        if (!liquid && (randomAmount <= FACTOR_FOR_WRONG_PILL_AMOUNT || specifiedAsLiquid)) {
            amount = Misc.randomRange(1, (!specifiedAsLiquid ? specifiedAmount : RANDOM_BASE_AMOUNT_PILLS) + 3);
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
        } else {
            pillsContainer.SetActive(false);
            pillsContainerXray.SetActive(false);
            liquidContainer.SetActive(true);
            liquidContainerXray.SetActive(true);
        }
    }
}
