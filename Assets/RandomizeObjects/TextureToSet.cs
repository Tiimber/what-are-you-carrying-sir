using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class TextureToSet {

    public PerRendererShaderTexture objectWithMaterial;
    public string category;

    public void assign(Texture texture, int materialIndex) {
        objectWithMaterial.texture = texture;
        objectWithMaterial.materialIndex = materialIndex;
    }
}
