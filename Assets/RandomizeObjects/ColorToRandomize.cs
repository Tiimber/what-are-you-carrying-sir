using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class ColorToRandomize {

    public PerRendererShader objectWithMaterial;
    public Color[] colors;

    public void assign() {
        objectWithMaterial.color = ItsRandom.pickRandom(colors.ToList());
    }
}
