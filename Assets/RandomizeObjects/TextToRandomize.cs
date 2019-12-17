using System.Linq;
using TMPro;
using UnityEngine;

[System.SerializableAttribute]
public class TextToRandomize {

    public TextMeshPro objectWithText;
    public string[] texts;

    public void assign() {
        objectWithText.text = ItsRandom.pickRandom(texts.ToList());
    }
}
