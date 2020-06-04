using System.Linq;
using TMPro;
using UnityEngine;

[System.SerializableAttribute]
public class TextPairToRandomize {

    public TextMeshPro objectWithText1;
    public TextMeshPro objectWithText2;
    public string[] texts;

    public void assign() {
        var randomText = ItsRandom.pickRandom(texts.ToList());
        objectWithText1.text = randomText;
        objectWithText2.text = randomText;
    }
}
