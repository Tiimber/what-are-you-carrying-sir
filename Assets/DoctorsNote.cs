using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DoctorsNote : MonoBehaviour {

    public List<TMP_FontAsset> signatureFonts;
    public List<Material> paperMaterials;
    
    public TextMeshPro title;
    public TextMeshPro text;
    public TextMeshPro medicineName;
    public TextMeshPro patientName;
    public TextMeshPro doctorName;
    public TextMeshPro doctorSignature;

    public GameObject paperMaterialObject;
    
    void Start() {
        assignProppertiesBasedOnBagContentProperties(GetComponent<BagContentPropertiesReference>().reference);
    }

    private void assignProppertiesBasedOnBagContentProperties(BagContentProperties bagContentProperties) {
        // Assign paper material
        Material[] materials = paperMaterialObject.GetComponent<Renderer>().materials;
        materials[0] = ItsRandom.pickRandom(paperMaterials);
        paperMaterialObject.GetComponent<Renderer>().materials = materials;

        
        // Medicine name
        string displayName = bagContentProperties.displayName;
        string pillName = displayName.Substring("Bottle of \"".Length);
        string medicineNameStr = pillName.Substring(0, pillName.Length - 1);
        medicineName.text = " - " + medicineNameStr;
        
        // Person name
        Person person = BagHandler.instance.currentBagInspect.bagDefinition.person;
        string patientNameStr = person.personName;
        patientName.text = patientNameStr;
        
        // Doctor name
        // TODO - Real logic ("real"/fake names)
        string doctorNameStr = ItsRandom.pickRandom(new List<string>() {
            "Nick Riviera",
            "Julius Hibbert",
            "Hannibal Lecter",
            "Saw U Apart",
            "Genital Fondler",
            "Pepper"
        });
        doctorName.text = "Dr. " + doctorNameStr;
        doctorSignature.font = ItsRandom.pickRandom(signatureFonts);
        doctorSignature.text = doctorNameStr;
    }
    
}
