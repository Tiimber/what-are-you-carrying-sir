using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Room : MonoBehaviour {
    public TextMeshPro airportName;

    public float pauseScreenZoom;
    public Vector3 pauseScreenPos;
    public Vector3 pauseScreenRotation;

    private Dictionary<string, string> airportNames = new Dictionary<string, string>(){
        {"mma", "Malmö - Puruts Airport"}
    };

    public void setLocation(string locale, string airportIdentifier) {
        GetComponent<SpecificTexture>().setTexture("flagPicture", "flags/" + locale);
        airportName.text = getAirportNameForIdentifier(airportIdentifier);
    }

    private String getAirportNameForIdentifier(string identifier) {
        if (airportNames.ContainsKey(identifier)) {
            return airportNames[identifier];
        }
        return "What are you carrying, sir?";
    }

}
