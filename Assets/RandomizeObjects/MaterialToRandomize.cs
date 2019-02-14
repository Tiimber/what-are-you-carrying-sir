using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class MaterialToRandomize {

    public AssignMaterialToObject objectToSetMaterial;
    public int materialIndex;
    public Material[] materials;

    public void assign() {
        Material picked = Misc.pickRandom(materials.ToList());
        objectToSetMaterial.material = picked;
        objectToSetMaterial.materialIndex = materialIndex;
    }
}
