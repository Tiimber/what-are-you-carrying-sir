using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.SerializableAttribute]
public class TextureToRandomize {

    public PerRendererShaderTexture objectWithMaterial;
    public int materialIndex;
    public Texture[] textures;

    public void assign() {
        objectWithMaterial.texture = Misc.pickRandom(textures.ToList());
        objectWithMaterial.materialIndex = materialIndex;
    }
}
