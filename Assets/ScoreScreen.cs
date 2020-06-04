using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ScoreScreen : MonoBehaviour {
    public TextMeshPro pointsText;
    public TextMeshPro errorWeapons;
    public TextMeshPro errorDrugs;
    public TextMeshPro errorArrest;
    public TextMeshPro errorWarning;

    private Dictionary<string, int> amountOfEach = new Dictionary<string, int>() {
        {"gun", 0},
        {"knife", 0},
        {"drugs", 0},
        {"false arrest", 0},
        {"warning", 0}
    };

    private const string CODE_KNIFE = "\U0001f52a";
    private const string CODE_GUN = "\U0001f52b";
    private const string CODE_PILL = "\U0001f48a";
    private const string CODE_POLICE = "\U0001f46e";
    private const string CODE_WARNING = "\U000026a0";

    public void Start() {
        clearStats();
    }

    public void update(Dictionary<string,int> mistakeSeverity, int points) {
        pointsText.text = "" + points;
        
        // "false arrest", "gun", "knife", "drugs", "warning"
        foreach (string mistakeType in mistakeSeverity.Keys.ToList()) {
            int amount = mistakeSeverity[mistakeType];
            int amountAlreadyAdded = amountOfEach[mistakeType];
            if (amount > amountAlreadyAdded) {
                for (int i = amountAlreadyAdded; i < amount; i++) {
                    if (mistakeType == "gun") {
                        errorWeapons.text += CODE_GUN;
                    } else if (mistakeType == "knife") {
                        errorWeapons.text += CODE_KNIFE;
                    } else if (mistakeType == "drugs") {
                        errorDrugs.text += CODE_PILL;
                    } else if (mistakeType == "false arrest") {
                        errorArrest.text += CODE_POLICE;
                    } else if (mistakeType == "false warning") {
                        errorWarning.text += CODE_WARNING;
                    }
                }

                amountOfEach[mistakeType] = amount;
            }
        }
        
    }

    public void clearStats() {
        errorWeapons.text = "";
        errorDrugs.text = "";
        errorArrest.text = "";
        errorWarning.text = "";

        foreach (string mistakeType in amountOfEach.Keys.ToList()) {
            amountOfEach[mistakeType] = 0;
        }
    }
}
