using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class PillsToRandomize {

    const float FACTOR_FOR_WRONG_PILLS = 0.05f;
    const float FACTOR_FOR_WRONG_TYPE = 0.05f;

    public Texture texture;
    public int amount;
    public bool liquid;

    public void assign(PerRendererShaderTexture objectWithMaterial, int materialIndex, GameObject pillsContainer) {
        objectWithMaterial.texture = texture;
        objectWithMaterial.materialIndex = materialIndex;

        // TODO - Random "wrongness"

        int numberOfPills = pillsContainer.transform.childCount;
        for (int i = amount; i < numberOfPills; i++) {
            pillsContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
