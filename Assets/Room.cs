using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour {
    public TextMeshPro airportName;

    public float pauseScreenZoom;
    public Vector3 pauseScreenPos;
    public Vector3 pauseScreenRotation;

    public Dictionary<string, Dictionary<string, string>> airportNames = new Dictionary<string, Dictionary<string, string>>(){
        {"mma", new Dictionary<string, string>() {
                {"name", "Malmö - Puruts Airport"},
                {"locale", "se"},
                {"xraymachine", "Machine #1"}
            }
        },
        {"waycs", new Dictionary<string, string>() {
                {"name", "What are you carrying, sir?"},
                {"locale", "eu"},
                {"xraymachine", "Machine #1"}
            }
        }
    };

    public String setLocation(string airportIdentifier) {
        Dictionary<string, string> details = getAirportDetailsForIdentifier(airportIdentifier);
        GetComponent<SpecificTexture>().setTexture("flagPicture", "flags/" + details["locale"]);
        airportName.text = details["name"];
        return details["xraymachine"];
    }

    private Dictionary<String, String> getAirportDetailsForIdentifier(string identifier) {
        if (airportNames.ContainsKey(identifier)) {
            return airportNames[identifier];
        }
        return airportNames["waycs"];
    }

}
